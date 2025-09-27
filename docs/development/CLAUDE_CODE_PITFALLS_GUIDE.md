# Claude Code Common Pitfalls and Solutions for WPFBase Framework

## 🚨 Critical Pitfalls That Break Everything

### 1. **Static Service Access** - FRAMEWORK KILLER

```csharp
// ❌ CRITICAL ERROR - This breaks the entire framework
public class BadViewModel : ViewModelBase
{
    public BadViewModel()
    {
        // This destroys testability and causes crashes
        var service = App.Current.Services.GetService<IDialogService>();
    }
    
    [RelayCommand]
    private async Task SomeAction()
    {
        // Static access in commands also breaks everything
        var nav = ServiceLocator.Current.GetService<INavigationService>();
    }
}
```

**Why This Kills Your App:**
- Breaks all unit tests (can't mock static dependencies)
- Causes crashes during app startup/shutdown
- Makes debugging impossible
- Violates dependency injection principles

**✅ CORRECT Solution:**
```csharp
public class GoodViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    
    public GoodViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }
    
    [RelayCommand]
    private async Task SomeAction()
    {
        await _dialogService.ShowInformationAsync("Success");
    }
}
```

### 2. **UI Thread Violations** - GUARANTEED CRASH

```csharp
// ❌ CRITICAL ERROR - Will crash your app
[RelayCommand]
private async Task BadAsyncOperation()
{
    await Task.Run(() =>
    {
        // This WILL crash with "Cross-thread operation not valid"
        Title = "Loading...";
        SomeProperty = newValue;
        Items.Add(newItem);
    });
}
```

**Why This Crashes:**
- WPF properties must be accessed on UI thread
- ObservableCollections are not thread-safe
- Property change notifications fail across threads

**✅ CORRECT Solution:**
```csharp
[RelayCommand]
private async Task GoodAsyncOperation()
{
    IsBusy = true; // UI thread - safe
    try
    {
        // Background work
        var result = await Task.Run(() => DoHeavyComputation());
        
        // Back on UI thread automatically - safe to update UI
        Title = "Completed";
        SomeProperty = result;
        Items.Clear();
        foreach (var item in result.Items)
        {
            Items.Add(item);
        }
    }
    finally
    {
        IsBusy = false; // UI thread - safe
    }
}
```

### 3. **Memory Leaks** - SLOW DEATH

```csharp
// ❌ CRITICAL ERROR - Creates memory leaks
public class LeakyViewModel : ViewModelBase
{
    private readonly IMessageBus _messageBus;
    
    public LeakyViewModel(IMessageBus messageBus)
    {
        _messageBus = messageBus;
        
        // Never disposed - MEMORY LEAK!
        _messageBus.Subscribe<StatusMessage>(OnStatus);
        
        // Event handlers without cleanup - MEMORY LEAK!
        SomeService.SomeEvent += OnSomeEvent;
        
        // Timer without disposal - MEMORY LEAK!
        var timer = new Timer(1000);
        timer.Elapsed += OnTimerElapsed;
        timer.Start();
    }
    
    private void OnStatus(StatusMessage msg) { }
    private void OnSomeEvent(object sender, EventArgs e) { }
    private void OnTimerElapsed(object sender, EventArgs e) { }
}
```

**Why This Kills Performance:**
- Subscriptions hold references preventing garbage collection
- Memory usage grows continuously
- Eventually causes OutOfMemoryException
- App becomes slower and slower

**✅ CORRECT Solution:**
```csharp
public class ProperViewModel : ViewModelBase
{
    private readonly IMessageBus _messageBus;
    private readonly IDisposable _messageSubscription;
    private readonly Timer _timer;
    
    public ProperViewModel(IMessageBus messageBus, ISomeService someService)
    {
        _messageBus = messageBus;
        
        // Store subscription for disposal
        _messageSubscription = _messageBus.Subscribe<StatusMessage>(OnStatus);
        
        // Use WeakEventManager for event handlers
        WeakEventManager<ISomeService, EventArgs>.AddHandler(
            someService, nameof(someService.SomeEvent), OnSomeEvent);
        
        // Store timer for disposal
        _timer = new Timer(1000);
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }
    
    protected override void OnDispose()
    {
        _messageSubscription?.Dispose();
        _timer?.Dispose();
        base.OnDispose();
    }
    
    private void OnStatus(StatusMessage msg) { }
    private void OnSomeEvent(object sender, EventArgs e) { }
    private void OnTimerElapsed(object sender, EventArgs e) { }
}
```

### 4. **Wrong Base Class Inheritance** - BREAKS MVVM

```csharp
// ❌ CRITICAL ERROR - Breaks MVVM pattern completely
public class BrokenViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    
    // Manual property implementation - breaks source generators
    private string _title;
    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
        }
    }
    
    // Manual command implementation - no async support
    public ICommand SaveCommand => new RelayCommand(Save);
    private void Save() { }
}
```

**Why This Breaks Everything:**
- Doesn't integrate with framework services
- No IsBusy support
- No validation support
- No navigation lifecycle
- Breaks source generators
- No async command support

**✅ CORRECT Solution:**
```csharp
public partial class ProperViewModel : ViewModelBase // or ValidatableViewModelBase
{
    // Source generators handle everything automatically
    [ObservableProperty]
    private string _title = string.Empty;
    
    [RelayCommand]
    private async Task SaveAsync()
    {
        IsBusy = true; // Built-in busy state
        try
        {
            await DoSaveAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

## ⚠️ Common Development Pitfalls

### 5. **Missing Service Registration** - RUNTIME CRASHES

```csharp
// ❌ PROBLEM - Service not registered
public class MyViewModel : ViewModelBase
{
    // Constructor expects IMyService but it's not registered
    public MyViewModel(IMyService service) { }
}

// App.xaml.cs ConfigureServices - MISSING registration
private void ConfigureServices(IServiceCollection services)
{
    // services.AddSingleton<IMyService, MyService>(); // MISSING!
}
```

**Runtime Error:**
```
System.InvalidOperationException: Unable to resolve service for type 'IMyService'
```

**✅ SOLUTION - Always Register Services:**
```csharp
private void ConfigureServices(IServiceCollection services)
{
    // REQUIRED - Register service
    services.AddSingleton<IMyService, MyService>();
    
    // REQUIRED - Register ViewModel
    services.AddTransient<MyViewModel>();
}
```

### 6. **Async Command Without Proper Error Handling** - SILENT FAILURES

```csharp
// ❌ PROBLEM - Exceptions get swallowed
[RelayCommand]
private async Task BadAsyncCommand()
{
    // If this throws, user never knows
    await _service.DoSomethingThatMightFailAsync();
}
```

**✅ SOLUTION - Always Handle Errors:**
```csharp
[RelayCommand]
private async Task GoodAsyncCommand()
{
    IsBusy = true;
    try
    {
        await _service.DoSomethingThatMightFailAsync();
        await _dialogService.ShowInformationAsync("Operation completed successfully");
    }
    catch (BusinessException ex)
    {
        await _dialogService.ShowWarningAsync(ex.Message);
    }
    catch (Exception ex)
    {
        _loggingService.LogError(ex, "Failed to execute command");
        await _dialogService.ShowErrorAsync("Operation failed", "Error", ex);
    }
    finally
    {
        IsBusy = false;
    }
}
```

### 7. **View/ViewModel Name Mismatch** - NAVIGATION FAILURES

```csharp
// ❌ PROBLEM - Names don't match
// File: ViewModels/CustomerListViewModel.cs
public partial class CustomerListViewModel : ViewModelBase { }

// File: Views/CustomerView.xaml  <- WRONG NAME!
<UserControl x:Class="MyApp.Views.CustomerView">
```

**Navigation Fails:**
```csharp
// This will fail to find the view
await _navigationService.NavigateToAsync<CustomerListViewModel>();
```

**✅ SOLUTION - Consistent Naming:**
```csharp
// ViewModels/CustomerListViewModel.cs
public partial class CustomerListViewModel : ViewModelBase { }

// Views/CustomerListView.xaml
<UserControl x:Class="MyApp.Views.CustomerListView">
```

### 8. **Validation Property Setup Wrong** - VALIDATION DOESN'T WORK

```csharp
// ❌ PROBLEM - Wrong property setup
public partial class FormViewModel : ValidatableViewModelBase
{
    // Using [ObservableProperty] doesn't call validation
    [ObservableProperty]
    [Required]
    private string _email = string.Empty; // Validation never triggers!
}
```

**✅ SOLUTION - Manual Property for Validation:**
```csharp
public partial class FormViewModel : ValidatableViewModelBase
{
    private string _email = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email
    {
        get => _email;
        set => SetPropertyWithValidation(ref _email, value); // Triggers validation
    }
}
```

### 9. **Blocking UI Thread** - FROZEN INTERFACE

```csharp
// ❌ PROBLEM - Blocks UI thread
[RelayCommand]
private void BadSyncOperation()
{
    // This freezes the UI for 5 seconds
    Thread.Sleep(5000);
    
    // This also blocks UI
    var result = _service.GetDataAsync().Result; // .Result blocks!
    
    // This blocks too
    _service.GetDataAsync().Wait(); // .Wait() blocks!
}
```

**✅ SOLUTION - Always Use Async Properly:**
```csharp
[RelayCommand]
private async Task GoodAsyncOperation()
{
    IsBusy = true; // Shows loading indicator
    try
    {
        // Properly awaited - doesn't block UI
        await Task.Delay(5000);
        
        // Properly awaited - doesn't block UI
        var result = await _service.GetDataAsync();
    }
    finally
    {
        IsBusy = false; // Hides loading indicator
    }
}
```

### 10. **Resource Leaks in Long-Running Operations** - PERFORMANCE DEGRADATION

```csharp
// ❌ PROBLEM - Resources not disposed
[RelayCommand]
private async Task BadFileOperation()
{
    var file = File.Open("data.txt", FileMode.Read); // Never disposed!
    var reader = new StreamReader(file); // Never disposed!
    var data = await reader.ReadToEndAsync();
    // File handles leak!
}
```

**✅ SOLUTION - Always Use 'using' Statements:**
```csharp
[RelayCommand]
private async Task GoodFileOperation()
{
    try
    {
        using var file = File.Open("data.txt", FileMode.Read);
        using var reader = new StreamReader(file);
        var data = await reader.ReadToEndAsync();
        // Resources automatically disposed
    }
    catch (IOException ex)
    {
        await _dialogService.ShowErrorAsync($"File error: {ex.Message}");
    }
}
```

## 🔧 Framework-Specific Gotchas

### 11. **Wrong MessageBus Usage** - MESSAGES LOST

```csharp
// ❌ PROBLEM - Publishing before subscribing
public class BadViewModel : ViewModelBase
{
    public BadViewModel(IMessageBus messageBus)
    {
        // Publish immediately - no one is listening yet!
        messageBus.Publish(new StatusMessage("Ready"));
        
        // Subscribe after publishing - misses the message
        messageBus.Subscribe<StatusMessage>(OnStatus);
    }
}
```

**✅ SOLUTION - Subscribe Before Publishing:**
```csharp
public class GoodViewModel : ViewModelBase
{
    private readonly IDisposable _subscription;
    
    public GoodViewModel(IMessageBus messageBus)
    {
        // Subscribe first
        _subscription = messageBus.Subscribe<StatusMessage>(OnStatus);
        
        // Then publish or do it in OnNavigatedTo
        _ = Task.Run(async () =>
        {
            await Task.Delay(100); // Let other ViewModels initialize
            messageBus.Publish(new StatusMessage("Ready"));
        });
    }
    
    protected override void OnDispose()
    {
        _subscription?.Dispose();
        base.OnDispose();
    }
}
```

### 12. **Docking Service Misuse** - LAYOUT CORRUPTION

```csharp
// ❌ PROBLEM - Opening documents without proper cleanup
[RelayCommand]
private async Task BadOpenDocument()
{
    // Creates multiple instances of same document
    var docVM = new TextDocumentViewModel();
    await _dockingService.ShowDocumentAsync(docVM, "TextDocument.xaml", "Doc");
    
    // Opening same file again creates duplicate
    await _dockingService.ShowDocumentAsync(docVM, "TextDocument.xaml", "Doc");
}
```

**✅ SOLUTION - Check If Already Open:**
```csharp
[RelayCommand]
private async Task GoodOpenDocument(string filePath)
{
    // Check if already open
    if (_dockingService.IsDocumentOpen(filePath))
    {
        await _dockingService.ActivateDocumentAsync(filePath);
        return;
    }
    
    // Create new document
    var docVM = new TextDocumentViewModel { FilePath = filePath };
    await _dockingService.ShowDocumentAsync(docVM, "TextDocument.xaml", Path.GetFileName(filePath));
}
```

### 13. **Configuration Service Misuse** - DATA LOSS

```csharp
// ❌ PROBLEM - Not saving configuration changes
[RelayCommand]
private async Task BadUpdateSettings()
{
    _configurationService.SetValue("Theme", "Dark");
    _configurationService.SetValue("FontSize", 14);
    // Changes lost on app restart - never saved!
}
```

**✅ SOLUTION - Always Save Changes:**
```csharp
[RelayCommand]
private async Task GoodUpdateSettings()
{
    try
    {
        _configurationService.SetValue("Theme", "Dark");
        _configurationService.SetValue("FontSize", 14);
        
        // CRITICAL - Save changes to disk
        await _configurationService.SaveAsync();
        
        await _dialogService.ShowInformationAsync("Settings saved successfully");
    }
    catch (Exception ex)
    {
        _loggingService.LogError(ex, "Failed to save settings");
        await _dialogService.ShowErrorAsync("Failed to save settings", "Error", ex);
    }
}
```

## 🔍 Debugging Common Issues

### **Build Error: "partial class must have same accessibility"**
```
Error CS0262: Partial declarations of 'MyViewModel' have conflicting accessibility modifiers
```
**Solution:** Ensure all partial declarations use same access modifier:
```csharp
public partial class MyViewModel : ViewModelBase  // public here
{
    // Generated code is also public
}
```

### **Runtime Error: "Object reference not set to an instance"**
**Most Common Causes:**
1. Service not registered in DI container
2. Not calling base constructor with required parameters
3. Using ObservableProperty without building project first

### **UI Not Updating Despite Property Changes**
**Causes:**
1. Not inheriting from ViewModelBase
2. Using field instead of property in XAML binding
3. Property change notifications not firing

**Solution:**
```csharp
// Wrong - field binding
<TextBox Text="{Binding _name}" />

// Correct - property binding  
<TextBox Text="{Binding Name}" />
```

### **Commands Not Working**
**Causes:**
1. Not building project after adding [RelayCommand]
2. CanExecute method returns false
3. Command binding to wrong property name

**Solution:**
```csharp
[RelayCommand(CanExecute = nameof(CanSave))]
private async Task SaveAsync() { }
private bool CanSave() => !IsBusy; // Make sure this returns true when intended

// XAML binding
<Button Command="{Binding SaveCommand}" /> <!-- SaveCommand, not SaveAsync -->
```

## 🛡️ Production Hardening Patterns

### **Always Use Try-Finally for IsBusy**
```csharp
[RelayCommand]
private async Task ProductionAsyncCommand()
{
    IsBusy = true;
    try
    {
        await DoWorkAsync();
    }
    finally
    {
        IsBusy = false; // ALWAYS reset, even if exception occurs
    }
}
```

### **Always Validate Parameters**
```csharp
[RelayCommand]
private async Task ProcessItem(MyItem item)
{
    if (item == null)
    {
        await _dialogService.ShowWarningAsync("No item selected");
        return;
    }
    
    // Process item
}
```

### **Always Log Exceptions**
```csharp
[RelayCommand]
private async Task ProductionCommand()
{
    try
    {
        await DoWorkAsync();
    }
    catch (Exception ex)
    {
        _loggingService.LogError(ex, "Failed to execute production command");
        await _dialogService.ShowErrorAsync("Operation failed", "Error", ex);
    }
}
```

### **Always Dispose Subscriptions**
```csharp
public partial class ProductionViewModel : ViewModelBase
{
    private readonly List<IDisposable> _subscriptions = new();
    
    public ProductionViewModel(IMessageBus messageBus)
    {
        _subscriptions.Add(messageBus.Subscribe<StatusMessage>(OnStatus));
        _subscriptions.Add(messageBus.Subscribe<NavigationMessage>(OnNavigation));
    }
    
    protected override void OnDispose()
    {
        foreach (var subscription in _subscriptions)
        {
            subscription?.Dispose();
        }
        _subscriptions.Clear();
        base.OnDispose();
    }
}
```

## 🎯 Claude Code Specific Patterns

### **Always Follow This Template for New ViewModels:**
```csharp
public partial class [FeatureName]ViewModel : ViewModelBase
{
    private readonly I[RequiredService] _[serviceName];
    private readonly IDialogService _dialogService;
    
    public [FeatureName]ViewModel(I[RequiredService] [serviceName], IDialogService dialogService)
    {
        _[serviceName] = [serviceName] ?? throw new ArgumentNullException(nameof([serviceName]));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        
        Title = "[Feature Name]"; // REQUIRED for docking
    }
    
    [ObservableProperty]
    private string _[propertyName] = string.Empty;
    
    [RelayCommand]
    private async Task [ActionName]Async()
    {
        IsBusy = true;
        try
        {
            // Implementation
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Failed to {[action]}", "Error", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

### **Always Follow This Template for New Services:**
```csharp
public interface I[ServiceName]
{
    Task<[ReturnType]> [MethodName]Async([Parameters]);
}

public class [ServiceName] : I[ServiceName]
{
    private readonly ILoggingService _logging;
    
    public [ServiceName](ILoggingService logging)
    {
        _logging = logging ?? throw new ArgumentNullException(nameof(logging));
    }
    
    public async Task<[ReturnType]> [MethodName]Async([Parameters])
    {
        try
        {
            _logging.LogInformation("Starting {Operation}", nameof([MethodName]));
            // Implementation
            return result;
        }
        catch (Exception ex)
        {
            _logging.LogError(ex, "Failed to execute {Operation}", nameof([MethodName]));
            throw;
        }
    }
}

// REQUIRED - Register in App.xaml.cs
services.AddSingleton<I[ServiceName], [ServiceName]>();
```

Following these patterns religiously will prevent 95% of common issues and ensure your WPF application built with this framework runs reliably in production.