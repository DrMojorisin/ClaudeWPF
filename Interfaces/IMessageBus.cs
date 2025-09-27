using System;

namespace WPFBase.Interfaces;

/// <summary>
/// Message bus for decoupled communication between components
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// Subscribe to a specific message type
    /// </summary>
    IDisposable Subscribe<TMessage>(Action<TMessage> handler) where TMessage : class;
    
    /// <summary>
    /// Subscribe to a specific message type with weak reference
    /// </summary>
    IDisposable SubscribeWeak<TMessage>(Action<TMessage> handler) where TMessage : class;
    
    /// <summary>
    /// Subscribe to a specific message type with async handler
    /// </summary>
    IDisposable SubscribeAsync<TMessage>(Func<TMessage, Task> handler) where TMessage : class;
    
    /// <summary>
    /// Publish a message to all subscribers
    /// </summary>
    void Publish<TMessage>(TMessage message) where TMessage : class;
    
    /// <summary>
    /// Publish a message asynchronously
    /// </summary>
    Task PublishAsync<TMessage>(TMessage message) where TMessage : class;
    
    /// <summary>
    /// Clear all subscriptions for a specific message type
    /// </summary>
    void ClearSubscriptions<TMessage>() where TMessage : class;
    
    /// <summary>
    /// Clear all subscriptions
    /// </summary>
    void ClearAllSubscriptions();
}

/// <summary>
/// Base class for messages
/// </summary>
public abstract class MessageBase
{
    /// <summary>
    /// Gets the timestamp when the message was created
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets the sender of the message
    /// </summary>
    public object? Sender { get; set; }
}

/// <summary>
/// Generic message with payload
/// </summary>
public class Message<T> : MessageBase
{
    /// <summary>
    /// Gets or sets the message payload
    /// </summary>
    public T? Payload { get; set; }
    
    public Message() { }
    
    public Message(T payload, object? sender = null)
    {
        Payload = payload;
        Sender = sender;
    }
}

// Note: NavigationMessage, StatusMessage, StatusMessageType and ProgressMessage moved to Models\Messages namespace