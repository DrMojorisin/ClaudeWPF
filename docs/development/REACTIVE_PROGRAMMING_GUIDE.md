# WPFBase Reactive Programming Integration Guide

## Overview

This guide provides comprehensive documentation for integrating reactive programming patterns into the WPFBase framework using System.Reactive (Rx.NET). The reactive extensions provide powerful tools for handling asynchronous data streams, events, and complex UI interactions while maintaining clean, testable code.

## Table of Contents

1. [Installation and Setup](#installation-and-setup)
2. [Core Concepts](#core-concepts)
3. [ReactiveViewModelBase](#reactiveviewmodelbase)
4. [Reactive Collections](#reactive-collections)
5. [Reactive Services](#reactive-services)
6. [State Management](#state-management)
7. [Performance Optimizations](#performance-optimizations)
8. [Migration Guide](#migration-guide)
9. [Best Practices](#best-practices)
10. [Common Patterns](#common-patterns)
11. [Troubleshooting](#troubleshooting)

## Installation and Setup

### Prerequisites

The reactive programming features are already included in WPFBase with the following packages:
- `System.Reactive` (6.0.1)
- `System.Reactive.Linq` (6.0.0)

### Service Registration

Add reactive services to your `App.xaml.cs`:

```csharp
private void ConfigureServices(IServiceCollection services)
{
    // Existing services...

    // Reactive services
    services.AddSingleton<IReactiveMessageBus>(provider =>
        new ReactiveMessageBus(provider.GetRequiredService<IMessageBus>()));

    services.AddSingleton<IReactiveDialogService>(provider =>
        new ReactiveDialogService(provider.GetRequiredService<IDialogService>()));

    services.AddSingleton<IReactiveStateStore<AppState>>(provider =>
        new ReactiveStateStore<AppState>(new AppState()));
}
```

## Core Concepts

### Observable Streams

Observables represent data that changes over time:

```csharp
// Property changes as observable stream
IObservable<string> searchTermChanges = WhenPropertyChanges<string>(nameof(SearchTerm));

// User input with debouncing
searchTermChanges
    .Where(term => term.Length >= 2)
    .Debounce(TimeSpan.FromMilliseconds(300))
    .DistinctUntilChanged()
    .Subscribe(term => PerformSearch(term));
```

### Reactive Commands

Commands that provide execution state and error handling:

```csharp
// Create reactive command with execution tracking
var saveCommand = CreateReactiveCommand(
    async () => await SaveDataAsync(),
    canExecute: IsValidObservable);

// Monitor command execution
saveCommand.IsExecuting.Subscribe(isExecuting => IsBusy = isExecuting);
saveCommand.ThrownExceptions.Subscribe(ex => HandleError(ex));
```

## ReactiveViewModelBase

### Basic Usage

Inherit from `ReactiveViewModelBase` instead of `ViewModelBase`:

```csharp
public partial class MyViewModel : ReactiveViewModelBase
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _isEnabled = true;

    public MyViewModel()
    {
        Title = "My Reactive View";
        SetupReactiveProperties();
    }

    private void SetupReactiveProperties()
    {
        // React to name changes
        WhenPropertyChanges<string>(nameof(Name))
            .Where(name => !string.IsNullOrEmpty(name))
            .Subscribe(name => ValidateName(name))
            .DisposeWith(_disposables);

        // Combine multiple properties
        Observable.CombineLatest(
            WhenPropertyChanges<string>(nameof(Name)),
            WhenPropertyChanges<bool>(nameof(IsEnabled)),
            (name, enabled) => !string.IsNullOrEmpty(name) && enabled)
            .Subscribe(canSave => SaveCommand.NotifyCanExecuteChanged())
            .DisposeWith(_disposables);
    }
}
```

### Property Change Streams

```csharp
// Monitor specific property changes
WhenPropertyChanges<string>(nameof(SearchTerm))
    .Debounce(TimeSpan.FromMilliseconds(300))
    .Subscribe(term => PerformSearch(term));

// Monitor any property change
WhenAnyPropertyChanges
    .Subscribe(_ => MarkAsDirty());

// Monitor busy state
WhenBusyChanges
    .Subscribe(isBusy => UpdateProgressIndicator(isBusy));
```

### Reactive Commands

```csharp
private void SetupCommands()
{
    // Simple command
    LoadDataCommand = CreateReactiveCommand(
        async () => await LoadDataAsync());

    // Command with parameter
    SaveItemCommand = CreateReactiveCommand<Item>(
        async item => await SaveItemAsync(item));

    // Command with can execute
    DeleteCommand = CreateReactiveCommand(
        async () => await DeleteSelectedItemAsync(),
        canExecute: HasSelectedItemObservable);
}
```

### Reactive Validation

```csharp
private void SetupValidation()
{
    // Create validation rule
    CreateValidationRule(
        WhenPropertyChanges<string>(nameof(Email)),
        () => IsValidEmail(Email)
            ? ValidationResult.Success()
            : ValidationResult.Error("Invalid email format"),
        nameof(Email));

    // Overall validation state
    IsValidObservable
        .Subscribe(isValid => CanSubmit = isValid)
        .DisposeWith(_disposables);
}
```

## Reactive Collections

### Basic ReactiveCollection

```csharp
public class ProductListViewModel : ReactiveViewModelBase
{
    private readonly ReactiveCollection<Product> _products = new();

    public ReactiveCollection<Product> Products => _products;

    private void SetupCollectionObservables()
    {
        // React to items being added
        _products.ItemsAdded
            .Subscribe(product => OnProductAdded(product));

        // React to count changes
        _products.CountChanged
            .Subscribe(count => UpdateStatusMessage($"Total products: {count}"));

        // React to any collection change
        _products.AnyChange
            .Subscribe(_ => MarkCollectionAsDirty());
    }
}
```

### Filtered Collections

```csharp
private void SetupFilteredCollection()
{
    // Create filtered view
    var filteredProducts = _allProducts.CreateFilteredView(ProductMatchesFilter);

    // Update filter based on search term
    WhenPropertyChanges<string>(nameof(SearchTerm))
        .Debounce(TimeSpan.FromMilliseconds(300))
        .Subscribe(_ => filteredProducts.SetFilter(ProductMatchesFilter));
}

private bool ProductMatchesFilter(Product product)
{
    return string.IsNullOrEmpty(SearchTerm) ||
           product.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase);
}
```

### Sorted Collections

```csharp
private void SetupSortedCollection()
{
    var sortedProducts = _filteredProducts.CreateSortedView();

    // Update sort when criteria changes
    Observable.CombineLatest(
        WhenPropertyChanges<SortBy>(nameof(SortBy)),
        WhenPropertyChanges<bool>(nameof(SortDescending)),
        (sortBy, descending) => new { SortBy = sortBy, Descending = descending })
        .Subscribe(sort => UpdateSort(sortedProducts, sort.SortBy, sort.Descending));
}
```

### Virtual Collections

```csharp
public class LargeDataViewModel : ReactiveViewModelBase
{
    private readonly VirtualReactiveCollection<DataItem> _virtualCollection;

    public LargeDataViewModel(IDataService dataService)
    {
        _virtualCollection = new VirtualReactiveCollection<DataItem>(
            async (startIndex, count) => await dataService.GetDataAsync(startIndex, count),
            totalCount: 1000000);
    }

    public async Task LoadWindow(int startIndex, int count)
    {
        await _virtualCollection.SetWindowAsync(startIndex, count);
    }
}
```

## Reactive Services

### ReactiveMessageBus

```csharp
public class MessagingViewModel : ReactiveViewModelBase
{
    private readonly IReactiveMessageBus _messageBus;

    public MessagingViewModel(IReactiveMessageBus messageBus)
    {
        _messageBus = messageBus;
        SetupMessageObservables();
    }

    private void SetupMessageObservables()
    {
        // Subscribe to specific message types
        _messageBus.MessagesOfType<StatusMessage>()
            .Subscribe(status => UpdateStatus(status.Text));

        // Monitor error messages
        _messageBus.MessagesOfType<ErrorMessage>()
            .Subscribe(error => HandleError(error));

        // Create filtered message pipeline
        _messageBus.CreateMessagePipeline<UserMessage, string>(
            filter: msg => msg.Priority == Priority.High,
            transform: msg => msg.Content,
            debounce: TimeSpan.FromMilliseconds(500))
            .Subscribe(content => ProcessHighPriorityMessage(content));
    }
}
```

### ReactiveDialogService

```csharp
public class DialogViewModel : ReactiveViewModelBase
{
    private readonly IReactiveDialogService _dialogService;

    private void SetupDialogObservables()
    {
        // Monitor dialog patterns
        _dialogService.GetUsagePatterns(TimeSpan.FromMinutes(5))
            .Where(pattern => pattern.DialogsPerMinute > 10)
            .Subscribe(pattern => LogHighDialogUsage(pattern));

        // Batch confirmations
        var confirmations = new[]
        {
            ("Delete all items?", "Confirm Delete"),
            ("This cannot be undone. Continue?", "Final Confirmation")
        };

        _dialogService.ShowConfirmationSeries(confirmations)
            .Subscribe(allConfirmed =>
            {
                if (allConfirmed) DeleteAllItems();
            });
    }
}
```

## State Management

### Setting Up State Store

```csharp
// Define application state
public record AppState
{
    public List<User> Users { get; init; } = new();
    public User? CurrentUser { get; init; }
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }
}

// Register in DI container
services.AddSingleton<IReactiveStateStore<AppState>>(
    new ReactiveStateStore<AppState>(new AppState()));
```

### Using State Store in ViewModels

```csharp
public class StateAwareViewModel : ReactiveViewModelBase
{
    private readonly IReactiveStateStore<AppState> _store;

    public StateAwareViewModel(IReactiveStateStore<AppState> store)
    {
        _store = store;
        SetupStateSubscriptions();
        RegisterReducers();
    }

    // State-derived observables
    public IObservable<User?> CurrentUser => _store.Select(state => state.CurrentUser);
    public IObservable<bool> IsLoading => _store.Select(state => state.IsLoading);
    public IObservable<List<User>> Users => _store.Select(state => state.Users);

    private void SetupStateSubscriptions()
    {
        CurrentUser.Subscribe(user => OnCurrentUserChanged(user));
        IsLoading.Subscribe(loading => IsBusy = loading);
    }

    private void RegisterReducers()
    {
        _store.RegisterReducer<User>("USER_SELECTED", (state, user) =>
            state with { CurrentUser = user });

        _store.RegisterReducer<List<User>>("USERS_LOADED", (state, users) =>
            state with { Users = users, IsLoading = false });

        _store.RegisterEffect<object>("LOAD_USERS", async (state, _) =>
        {
            var users = await userService.GetUsersAsync();
            await _store.DispatchAsync("USERS_LOADED", users);
            return null;
        });
    }
}
```

### Time Travel Debugging

```csharp
public class DebuggingViewModel : ReactiveViewModelBase
{
    private readonly IReactiveStateStore<AppState> _store;

    // Time travel commands
    [RelayCommand]
    private void GoBack() => _store.TravelBack();

    [RelayCommand]
    private void GoForward() => _store.TravelForward();

    [RelayCommand]
    private void GoToPresent() => _store.TravelToPresent();

    // History information
    public IObservable<StateHistory<AppState>> History => _store.History;
    public IObservable<bool> CanGoBack => History.Select(h => h.CanGoBack);
    public IObservable<bool> CanGoForward => History.Select(h => h.CanGoForward);
}
```

## Performance Optimizations

### Smart Debouncing

```csharp
// Adaptive debouncing based on input frequency
searchTermObservable
    .SmartDebounce(
        minTimeout: TimeSpan.FromMilliseconds(200),
        maxTimeout: TimeSpan.FromMilliseconds(800))
    .Subscribe(term => PerformSearch(term));
```

### Backpressure Handling

```csharp
// Throttling with backpressure
dataStream
    .ThrottleWithBackpressure(
        TimeSpan.FromMilliseconds(100),
        BackpressureStrategy.Latest)
    .Subscribe(data => ProcessData(data));
```

### Rate Limiting

```csharp
// Rate limiting with token bucket
apiCallStream
    .RateLimit(maxItems: 10, TimeSpan.FromMinutes(1))
    .Subscribe(request => MakeApiCall(request));
```

### Lazy Loading

```csharp
// Lazy loading with caching
var expensiveData = ReactivePerformanceOptimizations.LazyLoad(
    () => dataService.GetExpensiveDataAsync(),
    refreshInterval: TimeSpan.FromMinutes(5));

expensiveData.Subscribe(data => UpdateUI(data));
```

### Memory Management

```csharp
// Automatic disposal on memory pressure
heavyDataStream
    .WithMemoryPressureDisposal(MemoryPressureLevel.High)
    .Subscribe(data => ProcessHeavyData(data));

// Weak subscriptions
eventStream
    .WeakSubscribe(this, value => HandleEvent(value))
    .DisposeWith(_disposables);
```

## Migration Guide

### From Traditional ViewModels

#### Before (Traditional):
```csharp
public class OldViewModel : ViewModelBase
{
    private string _searchTerm = string.Empty;
    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (SetProperty(ref _searchTerm, value))
            {
                // Manual debouncing with timer
                _searchTimer?.Stop();
                _searchTimer = new Timer(300) { AutoReset = false };
                _searchTimer.Elapsed += (s, e) => PerformSearch(value);
                _searchTimer.Start();
            }
        }
    }

    private Timer? _searchTimer;
}
```

#### After (Reactive):
```csharp
public partial class NewViewModel : ReactiveViewModelBase
{
    [ObservableProperty]
    private string _searchTerm = string.Empty;

    private void SetupReactiveSearch()
    {
        WhenPropertyChanges<string>(nameof(SearchTerm))
            .Where(term => !string.IsNullOrEmpty(term))
            .Debounce(TimeSpan.FromMilliseconds(300))
            .DistinctUntilChanged()
            .Subscribe(term => PerformSearch(term))
            .DisposeWith(_disposables);
    }
}
```

### From ObservableCollection to ReactiveCollection

#### Before:
```csharp
public ObservableCollection<Item> Items { get; } = new();

private void OnItemsChanged()
{
    Items.CollectionChanged += (s, e) =>
    {
        // Manual handling of collection changes
        UpdateItemCount();
        ValidateItems();
    };
}
```

#### After:
```csharp
public ReactiveCollection<Item> Items { get; } = new();

private void SetupCollectionObservables()
{
    Items.CountChanged.Subscribe(count => ItemCount = count);
    Items.ItemsAdded.Subscribe(item => ValidateItem(item));
    Items.AnyChange.Subscribe(_ => MarkAsDirty());
}
```

### From Manual Event Handling to Reactive Streams

#### Before:
```csharp
public event EventHandler<StatusChangedEventArgs>? StatusChanged;

private void OnStatusChanged(string status)
{
    StatusChanged?.Invoke(this, new StatusChangedEventArgs(status));
}
```

#### After:
```csharp
private readonly Subject<string> _statusSubject = new();
public IObservable<string> StatusChanges => _statusSubject.AsObservable();

private void OnStatusChanged(string status)
{
    _statusSubject.OnNext(status);
}
```

## Best Practices

### 1. Subscription Management

Always dispose of subscriptions:

```csharp
public partial class MyViewModel : ReactiveViewModelBase
{
    private void SetupObservables()
    {
        someObservable
            .Subscribe(value => HandleValue(value))
            .DisposeWith(_disposables); // Critical for memory management
    }
}
```

### 2. Error Handling

Use reactive error handling patterns:

```csharp
dataStream
    .Catch<DataItem, Exception>(ex =>
    {
        LogError(ex);
        return Observable.Empty<DataItem>(); // or fallback value
    })
    .Retry(3)
    .Subscribe(data => ProcessData(data));
```

### 3. Threading and Schedulers

Be explicit about scheduling:

```csharp
backgroundOperation
    .ObserveOn(Scheduler.Default) // Background thread
    .SelectMany(async data => await ProcessDataAsync(data))
    .ObserveOnDispatcher() // Back to UI thread
    .Subscribe(result => UpdateUI(result));
```

### 4. Performance Considerations

Use appropriate operators for performance:

```csharp
// Good: Use operators to filter before heavy operations
dataStream
    .Where(data => data.IsValid)
    .Buffer(TimeSpan.FromSeconds(1))
    .Where(batch => batch.Any())
    .SelectMany(batch => ProcessBatchAsync(batch))
    .Subscribe(result => HandleResult(result));
```

### 5. Testing Reactive Code

Use test schedulers for testing:

```csharp
[Test]
public void Should_Debounce_Search_Terms()
{
    var testScheduler = new TestScheduler();
    var viewModel = new SearchViewModel(testScheduler);

    // Arrange
    viewModel.SearchTerm = "test";
    testScheduler.AdvanceBy(TimeSpan.FromMilliseconds(100).Ticks);

    viewModel.SearchTerm = "test2";
    testScheduler.AdvanceBy(TimeSpan.FromMilliseconds(300).Ticks);

    // Assert
    Assert.That(viewModel.SearchExecutionCount, Is.EqualTo(1));
}
```

## Common Patterns

### 1. Master-Detail Navigation

```csharp
public class MasterDetailViewModel : ReactiveViewModelBase
{
    [ObservableProperty]
    private Item? _selectedItem;

    public IObservable<Item?> SelectedItemChanges => WhenPropertyChanges<Item?>(nameof(SelectedItem));

    public IObservable<DetailViewModel?> DetailViewModel => SelectedItemChanges
        .Select(item => item != null ? new DetailViewModel(item) : null);
}
```

### 2. Form Validation

```csharp
public class FormViewModel : ReactiveViewModelBase
{
    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    public IObservable<bool> IsFormValid { get; private set; } = null!;

    private void SetupValidation()
    {
        var emailValid = WhenPropertyChanges<string>(nameof(Email))
            .Select(email => IsValidEmail(email));

        var passwordValid = WhenPropertyChanges<string>(nameof(Password))
            .Select(pwd => pwd.Length >= 8);

        IsFormValid = Observable.CombineLatest(
            emailValid,
            passwordValid,
            (email, password) => email && password);
    }
}
```

### 3. Auto-Save Functionality

```csharp
public class AutoSaveViewModel : ReactiveViewModelBase
{
    private void SetupAutoSave()
    {
        WhenAnyPropertyChanges
            .Where(_ => IsDirty)
            .Debounce(TimeSpan.FromSeconds(2))
            .SelectMany(_ => Observable.FromAsync(SaveAsync))
            .Subscribe(_ => IsDirty = false)
            .DisposeWith(_disposables);
    }
}
```

### 4. Real-time Data Updates

```csharp
public class RealTimeViewModel : ReactiveViewModelBase
{
    private void SetupRealTimeUpdates()
    {
        Observable.Interval(TimeSpan.FromSeconds(5))
            .SelectMany(_ => Observable.FromAsync(FetchLatestDataAsync))
            .ObserveOnDispatcher()
            .Subscribe(data => UpdateData(data))
            .DisposeWith(_disposables);
    }
}
```

## Troubleshooting

### Common Issues

#### 1. Memory Leaks
**Problem**: Subscriptions not disposed
**Solution**: Always use `DisposeWith(_disposables)`

#### 2. UI Thread Violations
**Problem**: Updating UI from background thread
**Solution**: Use `ObserveOnDispatcher()` or `ObserveOn(uiScheduler)`

#### 3. Performance Issues
**Problem**: Too many events firing
**Solution**: Use debouncing, throttling, or sampling

#### 4. Subscription Never Fires
**Problem**: Cold observable not subscribed
**Solution**: Ensure observable is hot or use `.Publish().RefCount()`

### Debugging Tips

1. **Use Do() operator for logging**:
```csharp
dataStream
    .Do(data => Debug.WriteLine($"Processing: {data}"))
    .Subscribe(ProcessData);
```

2. **Monitor subscription count**:
```csharp
observable
    .Do(_ => Debug.WriteLine($"Subscribers: {subject.HasObservers}"))
    .Subscribe();
```

3. **Use timestamp for timing analysis**:
```csharp
dataStream
    .Timestamp()
    .Do(timestamped => Debug.WriteLine($"At {timestamped.Timestamp}: {timestamped.Value}"))
    .Select(timestamped => timestamped.Value)
    .Subscribe();
```

## Conclusion

The reactive programming integration in WPFBase provides powerful tools for building responsive, maintainable applications. By following the patterns and best practices outlined in this guide, you can create applications that handle complex asynchronous scenarios with clean, testable code.

Key benefits of the reactive approach:
- **Declarative**: Express what you want, not how to do it
- **Composable**: Combine simple operations into complex behaviors
- **Testable**: Easy to unit test with test schedulers
- **Maintainable**: Clear separation of concerns and data flow
- **Performant**: Built-in optimizations for memory and performance

Start with simple scenarios and gradually adopt more advanced patterns as you become comfortable with the reactive paradigm.