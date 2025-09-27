using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using WPFBase.Interfaces;
using WPFBase.Models.Messages;

namespace WPFBase.Services;

/// <summary>
/// Thread-safe message bus implementation
/// </summary>
public class MessageBus : IMessageBus, IDisposable
{
    private readonly ConcurrentDictionary<Type, ConcurrentBag<Subscription>> _subscriptions = new();
    private readonly ConditionalWeakTable<object, List<Subscription>> _weakSubscriptions = new();
    private readonly ReaderWriterLockSlim _subscriptionLock = new(LockRecursionPolicy.NoRecursion);
    private bool _disposed;

    public IDisposable Subscribe<TMessage>(Action<TMessage> handler) where TMessage : class
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        
        var subscription = new Subscription(
            new Action<object>(message => handler((TMessage)message)),
            typeof(TMessage),
            false
        );
        
        AddSubscription(typeof(TMessage), subscription);
        
        return new SubscriptionToken(() => RemoveSubscription(typeof(TMessage), subscription));
    }

    public IDisposable SubscribeWeak<TMessage>(Action<TMessage> handler) where TMessage : class
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        
        var weakReference = new WeakReference(handler.Target);
        var methodInfo = handler.Method;
        
        Action<object> weakHandler = message =>
        {
            var target = weakReference.Target;
            if (target != null)
            {
                methodInfo.Invoke(target, new object[] { message });
            }
        };
        
        var subscription = new Subscription(new Action<object>(weakHandler), typeof(TMessage), true);
        subscription.WeakReference = weakReference;
        
        AddSubscription(typeof(TMessage), subscription);
        
        return new SubscriptionToken(() => RemoveSubscription(typeof(TMessage), subscription));
    }

    public IDisposable SubscribeAsync<TMessage>(Func<TMessage, Task> handler) where TMessage : class
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        
        var subscription = new Subscription(
            new Func<object, Task>(async message => await handler((TMessage)message)),
            typeof(TMessage),
            false,
            true
        );
        
        AddSubscription(typeof(TMessage), subscription);
        
        return new SubscriptionToken(() => RemoveSubscription(typeof(TMessage), subscription));
    }

    public void Publish<TMessage>(TMessage message) where TMessage : class
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        
        var messageType = typeof(TMessage);
        var subscriptions = GetSubscriptions(messageType);
        
        foreach (var subscription in subscriptions)
        {
            try
            {
                if (subscription.IsWeak && subscription.WeakReference != null && !subscription.WeakReference.IsAlive)
                {
                    RemoveSubscription(messageType, subscription);
                    continue;
                }
                
                if (subscription.IsAsync)
                {
                    if (subscription.Handler is Func<object, Task> asyncHandler)
                    {
                        Task.Run(async () => await asyncHandler(message));
                    }
                }
                else
                {
                    subscription.Handler.DynamicInvoke(message);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't let one failed handler stop others
                System.Diagnostics.Debug.WriteLine($"Message handler error: {ex}");
            }
        }
    }

    public async Task PublishAsync<TMessage>(TMessage message) where TMessage : class
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        
        var messageType = typeof(TMessage);
        var subscriptions = GetSubscriptions(messageType);
        var tasks = new List<Task>();
        
        foreach (var subscription in subscriptions)
        {
            try
            {
                if (subscription.IsWeak && subscription.WeakReference != null && !subscription.WeakReference.IsAlive)
                {
                    RemoveSubscription(messageType, subscription);
                    continue;
                }
                
                if (subscription.IsAsync)
                {
                    if (subscription.Handler is Func<object, Task> asyncHandler)
                    {
                        tasks.Add(Task.Run(async () => await asyncHandler(message)));
                    }
                }
                else
                {
                    tasks.Add(Task.Run(() => subscription.Handler.DynamicInvoke(message)));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Message handler error: {ex}");
            }
        }
        
        await Task.WhenAll(tasks);
    }

    public void ClearSubscriptions<TMessage>() where TMessage : class
    {
        _subscriptions.TryRemove(typeof(TMessage), out _);
    }

    public void ClearAllSubscriptions()
    {
        _subscriptions.Clear();
    }

    private void AddSubscription(Type messageType, Subscription subscription)
    {
        _subscriptions.AddOrUpdate(messageType,
            new ConcurrentBag<Subscription> { subscription },
            (key, bag) =>
            {
                bag.Add(subscription);
                return bag;
            });
    }

    private void RemoveSubscription(Type messageType, Subscription subscription)
    {
        if (_subscriptions.TryGetValue(messageType, out var bag))
        {
            _subscriptionLock.EnterWriteLock();
            try
            {
                // Create new bag without the subscription
                var newBag = new ConcurrentBag<Subscription>(bag.Where(s => s != subscription));
                if (newBag.IsEmpty)
                {
                    _subscriptions.TryRemove(messageType, out _);
                }
                else
                {
                    _subscriptions.TryUpdate(messageType, newBag, bag);
                }
            }
            finally
            {
                _subscriptionLock.ExitWriteLock();
            }
        }
    }

    private List<Subscription> GetSubscriptions(Type messageType)
    {
        if (_subscriptions.TryGetValue(messageType, out var bag))
        {
            _subscriptionLock.EnterReadLock();
            try
            {
                return bag.ToList();
            }
            finally
            {
                _subscriptionLock.ExitReadLock();
            }
        }
        
        return new List<Subscription>();
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        ClearAllSubscriptions();
        _subscriptionLock?.Dispose();
        _disposed = true;
    }

    private class Subscription
    {
        public Delegate Handler { get; }
        public Type MessageType { get; }
        public bool IsWeak { get; }
        public bool IsAsync { get; }
        public WeakReference? WeakReference { get; set; }
        
        public Subscription(Delegate handler, Type messageType, bool isWeak, bool isAsync = false)
        {
            Handler = handler;
            MessageType = messageType;
            IsWeak = isWeak;
            IsAsync = isAsync;
        }
    }

    private class SubscriptionToken : IDisposable
    {
        private Action? _unsubscribe;
        
        public SubscriptionToken(Action unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }
        
        public void Dispose()
        {
            _unsubscribe?.Invoke();
            _unsubscribe = null;
        }
    }
}

/// <summary>
/// Extension methods for message bus
/// </summary>
public static class MessageBusExtensions
{
    /// <summary>
    /// Publish a status message
    /// </summary>
    public static void PublishStatus(this IMessageBus messageBus, string text, Models.Messages.StatusMessageType type = Models.Messages.StatusMessageType.Information)
    {
        messageBus.Publish(new StatusMessage 
        { 
            Text = text, 
            Type = type 
        });
    }
    
    /// <summary>
    /// Publish a progress message
    /// </summary>
    public static void PublishProgress(this IMessageBus messageBus, string operationName, double progress, string? statusText = null)
    {
        messageBus.Publish(new ProgressMessage
        {
            OperationName = operationName,
            Progress = progress,
            StatusText = statusText,
            IsCompleted = progress >= 100
        });
    }
}