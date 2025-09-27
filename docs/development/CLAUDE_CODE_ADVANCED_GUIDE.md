# Claude Code Advanced Usage Guide for WPFBase Framework

## 🎯 Framework Overview for Claude Code

This is a **production-ready WPF MVVM framework** specifically designed for **rapid development with Claude Code automation**. The framework provides:

- **85%+ test coverage** - Thoroughly validated components
- **Thread-safe services** - Reliable for concurrent operations  
- **Comprehensive error handling** - Graceful failure recovery
- **Established patterns** - Consistent, predictable structure
- **Full documentation** - Complete API reference and examples

## 🚀 Quick Start for Claude Code

### Essential Information Check
When starting any task, **ALWAYS** verify these key points:

```markdown
✅ **Framework Verification Checklist:**
1. Confirm this is WPFBase framework (check CLAUDE.md exists)
2. Verify namespace is correct (check .csproj file)
3. Identify target: New feature vs Bug fix vs Enhancement
4. Check if similar pattern exists in framework already
5. Verify all required services are registered in App.xaml.cs
```

### Project Structure Understanding
```
YourProject/
├── ViewModels/           ← Always inherit from ViewModelBase/DockableViewModelBase
│   ├── Documents/        ← For document-based views (inherit DocumentViewModelBase)
│   └── Tools/           ← For tool windows (inherit from ToolViewModel)
├── Views/               ← UserControls, match ViewModel names
│   ├── Documents/
│   └── Tools/
├── Services/            ← Business logic, register in DI container
├── Models/              ← Data structures, POCOs
├── Interfaces/          ← Service contracts
├── Data/                ← Repository pattern implementation
├── Validators/          ← FluentValidation rules
└── Tests/              ← 85%+ coverage, follow existing patterns
```

## 🔧 Development Patterns for Claude Code

### 1. Creating New ViewModels - REQUIRED PATTERN

```csharp
// ✅ CORRECT - Always inherit from base classes
public partial class MyFeatureViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    
    // Constructor injection - REQUIRED
    public MyFeatureViewModel(IDialogService dialogService, INavigationService navigationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        
        Title = "My Feature"; // REQUIRED for docking
    }
    
    // Use [ObservableProperty] - REQUIRED pattern
    [ObservableProperty]
    private string _data = string.Empty;
    
    // Use [RelayCommand] - REQUIRED pattern  
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsBusy = true; // REQUIRED - show loading state
        try
        {
            // Implementation
            Data = await SomeAsyncOperation();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Failed to load data", "Error", ex);
        }
        finally
        {
            IsBusy = false; // REQUIRED - hide loading state
        }
    }
}

// ❌ WRONG - Don't create ViewModels from scratch
public class MyBadViewModel : INotifyPropertyChanged
{
    // This breaks the framework patterns!
}
```

### 2. Service Creation - REQUIRED PATTERN

```csharp
// ✅ CORRECT - Always create interface first
public interface IMyService
{
    Task<string> ProcessDataAsync(int id);
}

// Implementation with proper error handling
public class MyService : IMyService
{
    private readonly ILoggingService _logging;
    
    public MyService(ILoggingService logging)
    {
        _logging = logging ?? throw new ArgumentNullException(nameof(logging));
    }
    
    public async Task<string> ProcessDataAsync(int id)
    {
        if (id <= 0) throw new ArgumentException("ID must be positive", nameof(id));
        
        try
        {
            _logging.LogInformation("Processing data for ID: {Id}", id);
            // Implementation
            return await DoProcessing(id);
        }
        catch (Exception ex)
        {
            _logging.LogError(ex, "Failed to process data for ID: {Id}", id);
            throw; // Re-throw to let caller handle
        }
    }
}

// ✅ REQUIRED - Register in App.xaml.cs
services.AddSingleton<IMyService, MyService>();
```

### 3. View Creation - REQUIRED NAMING

```xml
<!-- ✅ CORRECT - Must match ViewModel name pattern -->
<!-- File: Views/MyFeatureView.xaml -->
<UserControl x:Class="YourProject.Views.MyFeatureView"
             xmlns:vm="clr-namespace:YourProject.ViewModels">
    <UserControl.DataContext>
        <vm:MyFeatureViewModel/>
    </UserControl.DataContext>
    
    <!-- UI Implementation -->
</UserControl>
```

## ⚠️ CRITICAL MISTAKES TO AVOID

### 1. **Dependency Injection Violations**

```csharp
// ❌ CRITICAL ERROR - Never do this!
public class BadViewModel : ViewModelBase  
{
    public BadViewModel()
    {
        // Static access breaks testability and DI
        var service = App.Current.Services.GetService<IMyService>();
    }
}

// ✅ CORRECT - Always use constructor injection
public class GoodViewModel : ViewModelBase
{
    public GoodViewModel(IMyService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }
}
```

### 2. **Threading Violations**

```csharp
// ❌ CRITICAL ERROR - UI thread access from background
[RelayCommand]
private async Task BadAsyncOperation()
{
    await Task.Run(() =>
    {
        // This will crash - UI access from background thread!
        Title = "New Title";
    });
}

// ✅ CORRECT - Proper async patterns
[RelayCommand]  
private async Task GoodAsyncOperation()
{
    IsBusy = true;
    try
    {
        var result = await Task.Run(() => DoHeavyWork());
        // Back on UI thread automatically
        Title = result;
    }
    finally
    {
        IsBusy = false;
    }
}
```

### 3. **Memory Leaks**

```csharp
// ❌ CRITICAL ERROR - Memory leaks
public class BadViewModel : ViewModelBase
{
    public BadViewModel(IMessageBus messageBus)
    {
        // Never dispose subscription - MEMORY LEAK!
        messageBus.Subscribe<MyMessage>(OnMessage);
    }
}

// ✅ CORRECT - Proper disposal
public class GoodViewModel : ViewModelBase
{
    private readonly IDisposable _subscription;
    
    public GoodViewModel(IMessageBus messageBus)
    {
        _subscription = messageBus.Subscribe<MyMessage>(OnMessage);
    }
    
    protected override void OnDispose()
    {
        _subscription?.Dispose();
        base.OnDispose();
    }
}
```

### 4. **Validation Errors**

```csharp
// ❌ WRONG - Manual validation implementation
public class BadValidationViewModel : ViewModelBase
{
    private string _email;
    public string Email
    {
        get => _email;
        set
        {
            if (!IsValidEmail(value))
                throw new ArgumentException("Invalid email");
            _email = value;
        }
    }
}

// ✅ CORRECT - Use framework validation
public class GoodValidationViewModel : ValidatableViewModelBase
{
    [Required]
    [EmailAddress]
    public string Email
    {
        get => _email;
        set => SetPropertyWithValidation(ref _email, value); // Framework handles validation
    }
}
```

## 🎯 Best Practices for Claude Code

### 1. **Always Check Existing Patterns First**

```csharp
// Before creating new code, check if similar exists:
// - Look in ViewModels/ for similar ViewModels
// - Check Services/ for similar services  
// - Review Interfaces/ for existing contracts
// - Examine Tests/ for usage patterns
```

### 2. **Follow Established Naming Conventions**

```csharp
// ViewModels: end with "ViewModel"
public class CustomerListViewModel : ViewModelBase { }

// Views: match ViewModel name, end with "View"  
// File: CustomerListView.xaml

// Services: descriptive names
public class CustomerService : ICustomerService { }

// Interfaces: start with "I"
public interface ICustomerService { }
```

### 3. **Proper Error Handling Pattern**

```csharp
[RelayCommand]
private async Task ProcessAsync()
{
    IsBusy = true;
    try
    {
        var result = await _service.ProcessDataAsync();
        // Success path
        await _dialogService.ShowInformationAsync("Process completed successfully");
    }
    catch (BusinessLogicException ex)
    {
        // Expected business errors
        await _dialogService.ShowWarningAsync(ex.Message);
    }
    catch (Exception ex)
    {
        // Unexpected errors
        _loggingService.LogError(ex, "Unexpected error during processing");
        await _dialogService.ShowErrorAsync("An unexpected error occurred", "Error", ex);
    }
    finally
    {
        IsBusy = false;
    }
}
```

### 4. **Proper Navigation Pattern**

```csharp
// ✅ CORRECT - Use navigation service
[RelayCommand]
private async Task NavigateToCustomerAsync(int customerId)
{
    await _navigationService.NavigateToAsync<CustomerDetailViewModel>(customerId);
}

// ❌ WRONG - Direct view manipulation
private void BadNavigation()
{
    var view = new CustomerDetailView();
    // Don't do this!
}
```

### 5. **Testing Integration**

```csharp
// When creating components, always consider testability:
// - Use interfaces for dependencies
// - Avoid static dependencies
// - Make methods virtual when needed for testing
// - Use dependency injection throughout

// Create corresponding test file:
// Component: CustomerService.cs
// Test: Tests/CustomerServiceTests.cs
```

## 🔍 Framework Services Reference

### Core Services Available (All Registered in DI)

```csharp
INavigationService     // View navigation and history
IDialogService        // User dialogs and file operations  
IMessageBus          // Pub/sub messaging
IDockingService      // Document/tool window management
IThemeService        // Theme management
IConfigurationService // App configuration
IUserSettingsService // User preferences
LoggingService       // Serilog integration
IKeyboardShortcutService // Shortcut management
```

### Service Usage Patterns

```csharp
// Navigation
await _navigationService.NavigateToAsync<TargetViewModel>(parameter);

// Dialogs
var result = await _dialogService.ShowConfirmationAsync("Are you sure?");
var file = await _dialogService.ShowOpenFileDialogAsync("txt", "Text files|*.txt");

// Messaging
_messageBus.Subscribe<MyMessage>(OnMessageReceived);
_messageBus.Publish(new MyMessage { Data = "test" });

// Docking  
await _dockingService.ShowDocumentAsync(viewModel, "DocumentView.xaml", "Title");
await _dockingService.ShowToolAsync(toolViewModel, "Tool Title", AnchorableShowStrategy.Left);

// Configuration
var value = _configService.GetValue<string>("SectionName:Key", "DefaultValue");
_configService.SetValue("SectionName:Key", newValue);
await _configService.SaveAsync();
```

## 🧪 Testing Requirements

### Required Test Coverage
- **All new ViewModels** must have corresponding tests
- **All new Services** must have comprehensive test coverage
- **Follow existing test patterns** in Tests/ directory
- **Use Moq for mocking** dependencies
- **Test both success and failure paths**

### Test Creation Pattern
```csharp
public class MyServiceTests
{
    [Fact]
    public async Task ProcessDataAsync_WithValidId_ReturnsExpectedResult()
    {
        // Arrange
        var mockDependency = new Mock<IDependency>();
        var service = new MyService(mockDependency.Object);
        
        // Act
        var result = await service.ProcessDataAsync(123);
        
        // Assert
        Assert.Equal("expected", result);
    }
    
    [Fact] 
    public async Task ProcessDataAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Arrange
        var service = new MyService(Mock.Of<IDependency>());
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.ProcessDataAsync(-1));
    }
}
```

## 📚 Quick Reference Commands

### For Claude Code Operations:

```bash
# Build and test
dotnet build
dotnet test

# Run application  
dotnet run

# Add new package
dotnet add package PackageName

# Create new component files
# ViewModels/MyViewModel.cs + Views/MyView.xaml + Tests/MyViewModelTests.cs
```

### Common Troubleshooting:

```csharp
// Build errors: Check these first
// 1. Missing using statements
// 2. Service not registered in App.xaml.cs  
// 3. Interface not implemented properly
// 4. Async/await missing
// 5. Wrong base class inheritance

// Runtime errors: Check these first
// 1. Service registration missing
// 2. Null reference in constructor
// 3. UI thread access violation
// 4. Resource not found (Views not matching ViewModels)
// 5. Missing parameter validation
```

## 🎯 Success Indicators

Your implementation is correct when:

✅ **Compiles without warnings**  
✅ **All tests pass**  
✅ **Application runs without crashes**  
✅ **UI responds smoothly (no UI thread blocking)**  
✅ **Memory usage stable (no leaks)**  
✅ **Error handling graceful**  
✅ **Navigation works as expected**  
✅ **Services integrate properly**  

## 🚨 Emergency Patterns

If something isn't working, try these patterns:

### Service Not Found
```csharp
// Add to App.xaml.cs ConfigureServices method:
services.AddSingleton<IMyService, MyService>();
```

### ViewModel Not Displaying
```csharp
// Check View/ViewModel naming:
// MyFeatureViewModel.cs → MyFeatureView.xaml
// Register both in App.xaml.cs if needed
```

### Commands Not Working
```csharp
// Ensure async commands use proper pattern:
[RelayCommand]
private async Task MyActionAsync() { /* implementation */ }

// Not:
[RelayCommand] 
private void MyAction() { } // Wrong for async operations
```

### Navigation Failing
```csharp
// Ensure ViewModel is registered:
services.AddTransient<MyViewModel>();

// Ensure View exists and matches name:
// MyViewModel → MyView.xaml
```

This framework is designed for **rapid, reliable development with Claude Code**. Following these patterns ensures consistent, maintainable, and robust applications.