# Claude Code Debugging and Troubleshooting Guide for WPFBase Framework

## 🔍 Quick Diagnostic Checklist

When Claude Code generates code that doesn't work, run through this checklist:

### ✅ **Immediate Build Checks**

1. **Build the project first** - Source generators need compilation
2. **Check for missing `using` statements** 
3. **Verify service registration in App.xaml.cs**
4. **Confirm ViewModel inherits from correct base class**
5. **Ensure View/ViewModel naming matches**

### ✅ **Runtime Failure Checks**

1. **Check DI container registrations**
2. **Verify async/await patterns** 
3. **Look for UI thread violations**
4. **Check for null reference exceptions in constructors**
5. **Verify navigation parameter types**

## 🚨 Build Error Solutions

### **Error: "Partial class must have same accessibility"**
```
CS0262: Partial declarations of 'MyViewModel' have conflicting accessibility modifiers
```

**Cause:** Access modifier mismatch between partial class declarations.

**Solution:**
```csharp
// Make sure both parts have same modifier
public partial class MyViewModel : ViewModelBase  // public here
{
    // Source-generated code is also public
}
```

### **Error: "ObservableProperty not found"**
```
CS0246: The type or namespace name 'ObservableProperty' could not be found
```

**Cause:** Missing using statement or CommunityToolkit.Mvvm not referenced.

**Solution:**
```csharp
using CommunityToolkit.Mvvm.ComponentModel;

public partial class MyViewModel : ViewModelBase
{
    [ObservableProperty] // Now recognized
    private string _data = string.Empty;
}
```

### **Error: "RelayCommand not found"**
```
CS0246: The type or namespace name 'RelayCommand' could not be found
```

**Cause:** Missing using statement for command attributes.

**Solution:**
```csharp
using CommunityToolkit.Mvvm.Input;

public partial class MyViewModel : ViewModelBase
{
    [RelayCommand] // Now recognized
    private async Task DoSomethingAsync() { }
}
```

### **Error: "Could not resolve service"**
```
System.InvalidOperationException: Unable to resolve service for type 'IMyService'
```

**Cause:** Service not registered in DI container.

**Solution:**
```csharp
// App.xaml.cs - ConfigureServices method
private void ConfigureServices(IServiceCollection services)
{
    // Add the missing registration
    services.AddSingleton<IMyService, MyService>();
}
```

### **Error: "Circular dependency detected"**
```
System.InvalidOperationException: A circular dependency was detected for the service
```

**Cause:** Service A depends on B, and B depends on A.

**Solution:**
```csharp
// Break the cycle by using IServiceProvider or factory pattern
public class ServiceA
{
    private readonly Func<ServiceB> _serviceBFactory;
    
    public ServiceA(Func<ServiceB> serviceBFactory)
    {
        _serviceBFactory = serviceBFactory;
    }
}
```

### **Error: "Properties not generated"**
```
CS1061: 'MyViewModel' does not contain a definition for 'Data'
```

**Cause:** [ObservableProperty] fields not generating properties.

**Solution:**
1. **Build the project** - Source generators run during compilation
2. **Check field naming** - Must start with underscore:
```csharp
[ObservableProperty]
private string _data = string.Empty; // Generates: public string Data { get; set; }
```

### **Error: "Commands not generated"**
```
CS1061: 'MyViewModel' does not contain a definition for 'SaveCommand'
```

**Cause:** [RelayCommand] methods not generating commands.

**Solution:**
1. **Build the project** - Commands generated during compilation
2. **Check method naming** - Async methods should end with "Async":
```csharp
[RelayCommand]
private async Task SaveAsync() { } // Generates: SaveCommand

[RelayCommand] 
private void Save() { } // Generates: SaveCommand
```

## 💥 Runtime Error Solutions

### **Error: "Cross-thread operation not valid"**
```
System.InvalidOperationException: The calling thread cannot access this object because a different thread owns it
```

**Cause:** Accessing UI elements from background thread.

**Solution:**
```csharp
// ❌ WRONG - UI access from background thread
[RelayCommand]
private async Task BadAsyncOperation()
{
    await Task.Run(() =>
    {
        Title = "Loading..."; // CRASH!
    });
}

// ✅ CORRECT - UI updates on UI thread
[RelayCommand]
private async Task GoodAsyncOperation()
{
    IsBusy = true; // UI thread - safe
    try
    {
        var result = await Task.Run(() => DoWork()); // Background thread
        Title = result; // Back on UI thread - safe
    }
    finally
    {
        IsBusy = false; // UI thread - safe
    }
}
```

### **Error: "Object reference not set to an instance"**

**Common Causes:**

1. **Service not registered:**
```csharp
// Check App.xaml.cs - is service registered?
services.AddSingleton<IMyService, MyService>();
```

2. **Constructor parameter null:**
```csharp
public MyViewModel(IMyService service)
{
    _service = service ?? throw new ArgumentNullException(nameof(service));
    // This will show exactly what's null
}
```

3. **ObservableCollection not initialized:**
```csharp
[ObservableProperty]
private ObservableCollection<Item> items = new(); // Initialize here

// Or in constructor:
public MyViewModel()
{
    Items = new ObservableCollection<Item>();
}
```

### **Error: "Navigation failed"**

**Debugging Steps:**

1. **Check ViewModel registration:**
```csharp
// App.xaml.cs
services.AddTransient<MyViewModel>();
```

2. **Check View registration:**
```csharp
// App.xaml.cs
services.AddTransient<MyView>();
```

3. **Check naming convention:**
```csharp
// Must match: MyViewModel -> MyView
public class MyViewModel : ViewModelBase { }
// Views/MyView.xaml
```

4. **Check navigation call:**
```csharp
// Correct navigation
await _navigationService.NavigateToAsync<MyViewModel>();

// Navigation with parameter
await _navigationService.NavigateToAsync<MyViewModel>(parameter);
```

### **Error: "Commands not enabled"**

**Causes and Solutions:**

1. **CanExecute returns false:**
```csharp
[RelayCommand(CanExecute = nameof(CanSave))]
private async Task SaveAsync() { }

private bool CanSave() => !IsBusy; // Make sure this returns true
```

2. **Command not notifying CanExecute changes:**
```csharp
// Call this when conditions change:
SaveCommand.NotifyCanExecuteChanged();

// Or use ObservableProperty which notifies automatically:
[ObservableProperty]
private bool _isBusy; // Automatically calls NotifyCanExecuteChanged
```

## 🔧 Debugging Techniques

### **1. Enable Detailed Logging**

```csharp
// Add to App.xaml.cs or main method
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() // Show debug messages
    .WriteTo.Debug() // Output to VS debug window
    .WriteTo.File("debug.log") // Output to file
    .CreateLogger();
```

### **2. Use Exception Breakpoints**

In Visual Studio:
1. Debug -> Windows -> Exception Settings
2. Check "Common Language Runtime Exceptions"
3. Run in debug mode - will break on any exception

### **3. Check Source Generated Code**

View generated code to understand what's happening:
1. Build project
2. In Solution Explorer: Dependencies -> Analyzers -> CommunityToolkit.Mvvm.SourceGenerators
3. Expand to see generated files

### **4. Debug Navigation Issues**

```csharp
// Add logging to navigation service calls
_loggingService.LogDebug("Navigating to {ViewModelType}", typeof(MyViewModel).Name);
await _navigationService.NavigateToAsync<MyViewModel>();
_loggingService.LogDebug("Navigation completed");
```

### **5. Debug DI Container Issues**

```csharp
// Check what's registered in container
var serviceProvider = services.BuildServiceProvider();
var registeredServices = services.Select(s => s.ServiceType.Name);
_loggingService.LogDebug("Registered services: {Services}", string.Join(", ", registeredServices));
```

## 🐛 Common Framework-Specific Issues

### **Issue: Validation Not Working**

**Problem:** Properties with validation attributes not showing errors.

**Solution:**
```csharp
// ❌ WRONG - Using [ObservableProperty] with validation
public partial class MyViewModel : ValidatableViewModelBase
{
    [ObservableProperty]
    [Required] // This won't work!
    private string _email = string.Empty;
}

// ✅ CORRECT - Manual property for validation
public partial class MyViewModel : ValidatableViewModelBase
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

### **Issue: Docking Not Working**

**Problem:** Documents or tools not showing in docking interface.

**Debugging:**
1. **Check if DockingService is registered:**
```csharp
services.AddSingleton<IDockingService, DockingService>();
```

2. **Check ViewModel Title property:**
```csharp
public MyDocumentViewModel()
{
    Title = "My Document"; // REQUIRED for docking
}
```

3. **Check docking method calls:**
```csharp
// For documents:
await _dockingService.ShowDocumentAsync(viewModel, "MyDocumentView.xaml", "Title");

// For tools:
await _dockingService.ShowToolAsync(viewModel, "Tool Title", AnchorableShowStrategy.Left);
```

### **Issue: MessageBus Not Working**

**Problem:** Published messages not received by subscribers.

**Debugging:**
1. **Check subscription timing:**
```csharp
// ❌ WRONG - Publishing before anyone subscribes
public MyViewModel(IMessageBus messageBus)
{
    messageBus.Publish(new MyMessage()); // No one listening yet!
    _subscription = messageBus.Subscribe<MyMessage>(OnMessage);
}

// ✅ CORRECT - Subscribe first, then publish
public MyViewModel(IMessageBus messageBus)
{
    _subscription = messageBus.Subscribe<MyMessage>(OnMessage);
    // Publish later or in OnNavigatedTo
}
```

2. **Check message types match exactly:**
```csharp
// Publisher
messageBus.Publish(new StatusMessage("Hello"));

// Subscriber - must be exact same type
messageBus.Subscribe<StatusMessage>(OnStatusMessage); // Works
messageBus.Subscribe<object>(OnMessage); // Won't receive StatusMessage
```

## 🔍 Performance Debugging

### **Memory Leaks**

**Symptoms:** Application memory usage grows over time.

**Debugging:**
1. **Check disposable subscriptions:**
```csharp
public class MyViewModel : ViewModelBase
{
    private readonly IDisposable _subscription;
    
    public MyViewModel(IMessageBus messageBus)
    {
        _subscription = messageBus.Subscribe<MyMessage>(OnMessage);
    }
    
    protected override void OnDispose()
    {
        _subscription?.Dispose(); // REQUIRED!
        base.OnDispose();
    }
}
```

2. **Check event handler cleanup:**
```csharp
// Use WeakEventManager for external events
WeakEventManager<SomeService, EventArgs>.AddHandler(
    someService, nameof(someService.SomeEvent), OnSomeEvent);
```

### **UI Freezing**

**Symptoms:** Interface becomes unresponsive.

**Debugging:**
1. **Check for blocking async calls:**
```csharp
// ❌ WRONG - Blocks UI thread
var result = SomeAsyncMethod().Result;
var result2 = SomeAsyncMethod().GetAwaiter().GetResult();

// ✅ CORRECT - Properly async
var result = await SomeAsyncMethod();
```

2. **Check for long-running synchronous operations:**
```csharp
// ❌ WRONG - Blocks UI thread
[RelayCommand]
private void ProcessData()
{
    for (int i = 0; i < 1000000; i++)
    {
        // Long running work on UI thread
    }
}

// ✅ CORRECT - Move to background thread
[RelayCommand]
private async Task ProcessDataAsync()
{
    IsBusy = true;
    try
    {
        await Task.Run(() =>
        {
            for (int i = 0; i < 1000000; i++)
            {
                // Long running work on background thread
            }
        });
    }
    finally
    {
        IsBusy = false;
    }
}
```

## 📱 Testing and Validation

### **Unit Test Debugging**

**Problem:** Tests fail when code works in application.

**Common Causes:**
1. **Async test not properly awaited:**
```csharp
[Fact]
public async Task TestAsync() // Must be async
{
    await viewModel.LoadDataCommand.ExecuteAsync(null); // Must await
    Assert.Equal("Expected", viewModel.Data);
}
```

2. **Mock setup incorrect:**
```csharp
// Setup must match exact call
mockService.Setup(x => x.GetDataAsync(It.IsAny<int>())).ReturnsAsync("test");

// Then call:
await service.GetDataAsync(123); // This will work

// But this won't:
await service.GetDataAsync(); // No parameter provided
```

3. **Service not mocked:**
```csharp
// All dependencies must be mocked or provided
var mockService = new Mock<IMyService>();
var mockDialog = new Mock<IDialogService>();

var viewModel = new MyViewModel(
    mockService.Object,
    mockDialog.Object // Don't forget any dependencies!
);
```

## 🎯 Debugging Workflow for Claude Code

When Claude Code generates problematic code:

1. **Build First** - Many issues are resolved by compilation
2. **Check Logs** - Framework logs extensively 
3. **Run Tests** - Comprehensive test suite catches issues early
4. **Use Debugger** - Set breakpoints in ViewModels and Services
5. **Check Documentation** - Refer to pattern guides
6. **Simplify Code** - Remove complexity to isolate issues
7. **Compare with Working Examples** - Use framework patterns as reference

## 📞 Getting Help

If you're still stuck after following this guide:

1. **Check the pattern guides** - Your issue might be a pattern violation
2. **Review test cases** - They show correct usage patterns
3. **Enable debug logging** - Framework logs extensive information
4. **Create minimal reproduction** - Isolate the problem to the smallest code sample

Remember: The WPFBase framework is thoroughly tested and production-proven. Most issues stem from deviation from established patterns rather than framework bugs.