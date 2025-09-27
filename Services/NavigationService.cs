using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Windows.Controls;
using WPFBase.Interfaces;
using WPFBase.ViewModels;

namespace WPFBase.Services;

/// <summary>
/// Enhanced navigation service with view caching and memory management
/// </summary>
public class NavigationService : INavigationService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<NavigationEntry> _navigationStack = new();
    private readonly ConcurrentDictionary<Type, WeakReference> _viewCache = new();
    private readonly Dictionary<Type, Type> _viewModelToViewMapping = new();
    private UserControl? _currentView;
    private readonly int _maxCacheSize = 10;
    private bool _disposed;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        RegisterViewMappings();
    }

    public UserControl? CurrentView => _currentView;

    public bool CanGoBack => _navigationStack.Count > 1;

    public bool CanGoForward => false; // Can be extended for forward navigation

    public event EventHandler<NavigationEventArgs>? Navigated;

    private void RegisterViewMappings()
    {
        try
        {
            // Auto-discover View/ViewModel mappings via reflection
            var viewModelTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => 
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .Where(t => t.Namespace?.StartsWith("WPFBase.ViewModels") == true && 
                           t.Name.EndsWith("ViewModel") && 
                           !t.IsAbstract);

            foreach (var vmType in viewModelTypes)
            {
                var viewName = vmType.Name.Replace("ViewModel", "View");
                var viewType = Type.GetType($"WPFBase.Views.{viewName}");
                if (viewType != null)
                {
                    _viewModelToViewMapping[vmType] = viewType;
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail initialization
            System.Diagnostics.Debug.WriteLine($"Error registering view mappings: {ex}");
        }
    }

    public async Task NavigateToAsync<TViewModel>(object? parameter = null) where TViewModel : class
    {
        await NavigateToAsync(typeof(TViewModel), parameter);
    }

    public async Task NavigateToAsync(string viewModelTypeName, object? parameter = null)
    {
        var viewModelType = ResolveViewModelType(viewModelTypeName);
        await NavigateToAsync(viewModelType, parameter);
    }

    private async Task NavigateToAsync(Type viewModelType, object? parameter = null)
    {
        try
        {
            // Get or create view
            var (view, viewModel, isFromCache) = GetOrCreateView(viewModelType);

            // Handle current view lifecycle
            if (_currentView?.DataContext is ViewModelBase oldViewModel)
            {
                await oldViewModel.OnNavigatedFromAsync();
                
                // Dispose if not keeping in cache
                if (_navigationStack.Count > _maxCacheSize && oldViewModel is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            // Update navigation state
            _currentView = view;
            _navigationStack.Push(new NavigationEntry(viewModelType, view, parameter));

            // Handle new view lifecycle
            if (viewModel is ViewModelBase newViewModel)
            {
                await newViewModel.OnNavigatedToAsync(parameter);
            }

            // Cleanup old cached views if needed
            CleanupCache();

            // Raise navigation event
            Navigated?.Invoke(this, new NavigationEventArgs 
            { 
                ViewModelType = viewModelType, 
                Parameter = parameter,
                IsFromCache = isFromCache
            });
        }
        catch (Exception ex)
        {
            throw new NavigationException($"Failed to navigate to {viewModelType.Name}", ex);
        }
    }

    public async Task GoBackAsync()
    {
        if (!CanGoBack)
            return;

        // Remove current view from stack
        var currentEntry = _navigationStack.Pop();
        
        // Get previous view
        var previousEntry = _navigationStack.Peek();

        // Handle navigation lifecycle
        if (_currentView?.DataContext is ViewModelBase oldViewModel)
        {
            await oldViewModel.OnNavigatedFromAsync();
        }

        _currentView = previousEntry.View;

        if (previousEntry.View.DataContext is ViewModelBase newViewModel)
        {
            await newViewModel.OnNavigatedToAsync(previousEntry.Parameter);
        }

        // Dispose removed entry if needed
        if (currentEntry.View.DataContext is IDisposable disposable && 
            !_viewCache.ContainsKey(currentEntry.ViewModelType))
        {
            disposable.Dispose();
        }

        // Raise navigation event
        Navigated?.Invoke(this, new NavigationEventArgs 
        { 
            ViewModelType = previousEntry.ViewModelType, 
            Parameter = previousEntry.Parameter,
            IsFromCache = true
        });
    }

    public void ClearHistory()
    {
        while (_navigationStack.Count > 1)
        {
            var entry = _navigationStack.Pop();
            if (entry.View.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    private (UserControl view, object viewModel, bool isFromCache) GetOrCreateView(Type viewModelType)
    {
        // Check cache first
        if (_viewCache.TryGetValue(viewModelType, out var weakRef) && 
            weakRef.IsAlive && 
            weakRef.Target is UserControl cachedView)
        {
            return (cachedView, cachedView.DataContext!, true);
        }

        // Get View type
        if (!_viewModelToViewMapping.TryGetValue(viewModelType, out var viewType))
        {
            var viewName = viewModelType.Name.Replace("ViewModel", "View");
            viewType = Type.GetType($"WPFBase.Views.{viewName}") 
                ?? throw new InvalidOperationException($"View type '{viewName}' not found");
        }

        // Create new instances
        var viewModel = _serviceProvider.GetRequiredService(viewModelType);
        var view = (UserControl)Activator.CreateInstance(viewType)!;
        view.DataContext = viewModel;

        // Cache the view
        _viewCache[viewModelType] = new WeakReference(view);

        return (view, viewModel, false);
    }

    private Type ResolveViewModelType(string viewModelTypeName)
    {
        // Try exact match first
        var type = Type.GetType($"WPFBase.ViewModels.{viewModelTypeName}");
        if (type != null) return type;

        // Try with ViewModel suffix
        if (!viewModelTypeName.EndsWith("ViewModel"))
        {
            type = Type.GetType($"WPFBase.ViewModels.{viewModelTypeName}ViewModel");
            if (type != null) return type;
        }

        throw new InvalidOperationException($"ViewModel type '{viewModelTypeName}' not found");
    }

    private void CleanupCache()
    {
        // Remove dead references
        var deadKeys = _viewCache
            .Where(kvp => !kvp.Value.IsAlive)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in deadKeys)
        {
            _viewCache.TryRemove(key, out _);
        }

        // Enforce cache size limit
        if (_viewCache.Count > _maxCacheSize)
        {
            var toRemove = _viewCache.Keys
                .Except(_navigationStack.Select(e => e.ViewModelType))
                .Take(_viewCache.Count - _maxCacheSize)
                .ToList();

            foreach (var key in toRemove)
            {
                if (_viewCache.TryRemove(key, out var weakRef) && 
                    weakRef.IsAlive && 
                    weakRef.Target is UserControl view &&
                    view.DataContext is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        // Dispose all cached views
        foreach (var entry in _navigationStack)
        {
            if (entry.View.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        foreach (var weakRef in _viewCache.Values)
        {
            if (weakRef.IsAlive && weakRef.Target is UserControl view && 
                view.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _navigationStack.Clear();
        _viewCache.Clear();
        _disposed = true;
    }

    private class NavigationEntry
    {
        public Type ViewModelType { get; }
        public UserControl View { get; }
        public object? Parameter { get; }

        public NavigationEntry(Type viewModelType, UserControl view, object? parameter)
        {
            ViewModelType = viewModelType;
            View = view;
            Parameter = parameter;
        }
    }
}

/// <summary>
/// Navigation exception
/// </summary>
public class NavigationException : Exception
{
    public NavigationException(string message) : base(message) { }
    public NavigationException(string message, Exception innerException) : base(message, innerException) { }
}