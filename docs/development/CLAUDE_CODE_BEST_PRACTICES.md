# Claude Code Best Practices for WPFBase Framework

## 🎯 Overview

This guide provides **specific best practices** for Claude Code when working with the WPFBase framework. Following these practices ensures **consistent, reliable, and maintainable** code generation.

## 🚀 Pre-Development Checklist

Before writing any code, **ALWAYS** run through this checklist:

### ✅ **Framework Verification (MANDATORY)**

```markdown
1. [ ] Confirm WPFBase framework (check for CLAUDE.md)
2. [ ] Verify project namespace in .csproj file
3. [ ] Check App.xaml.cs for existing service registrations
4. [ ] Review similar patterns in existing codebase
5. [ ] Identify if this is: New feature | Bug fix | Enhancement
```

### ✅ **Requirements Analysis**

```markdown
1. [ ] What ViewModels are needed?
2. [ ] What Services are required?
3. [ ] What Views need to be created?
4. [ ] What validation is needed?
5. [ ] What error handling is required?
6. [ ] Are tests needed? (Answer: Always YES)
```

## 📋 Development Workflow

### 1. **Service-First Development**

Always start with services when adding new functionality:

```csharp
// Step 1: Create interface in /Interfaces
public interface ICustomerService
{
    Task<List<Customer>> GetCustomersAsync();
    Task<Customer?> GetCustomerByIdAsync(int id);
    Task<Customer> CreateCustomerAsync(Customer customer);
    Task<Customer> UpdateCustomerAsync(Customer customer);
    Task DeleteCustomerAsync(int id);
}

// Step 2: Create implementation in /Services
public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _repository;
    private readonly ILoggingService _logging;
    
    public CustomerService(IRepository<Customer> repository, ILoggingService logging)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logging = logging ?? throw new ArgumentNullException(nameof(logging));
    }
    
    public async Task<List<Customer>> GetCustomersAsync()
    {
        try
        {
            _logging.LogInformation("Retrieving all customers");
            var customers = await _repository.GetAllAsync();
            return customers.ToList();
        }
        catch (Exception ex)
        {
            _logging.LogError(ex, "Failed to retrieve customers");
            throw;
        }
    }
    
    // ... other methods with similar error handling pattern
}

// Step 3: Register in App.xaml.cs
services.AddScoped<ICustomerService, CustomerService>();
```

### 2. **ViewModel Development Pattern**

```csharp
// Always follow this exact pattern:
public partial class CustomerListViewModel : ViewModelBase
{
    private readonly ICustomerService _customerService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    
    // Constructor with full null checking
    public CustomerListViewModel(
        ICustomerService customerService,
        IDialogService dialogService, 
        INavigationService navigationService)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        
        Title = "Customer List"; // REQUIRED
        
        // Initialize collections
        Customers = new ObservableCollection<Customer>();
        
        // Load data on creation
        _ = LoadCustomersAsync();
    }
    
    // Observable collections for UI binding
    [ObservableProperty]
    private ObservableCollection<Customer> customers = new();
    
    [ObservableProperty]
    private Customer? selectedCustomer;
    
    [ObservableProperty]
    private string searchText = string.Empty;
    
    // Commands with proper async patterns
    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        IsBusy = true;
        try
        {
            var customerList = await _customerService.GetCustomersAsync();
            Customers.Clear();
            foreach (var customer in customerList)
            {
                Customers.Add(customer);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Failed to load customers", "Error", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand(CanExecute = nameof(CanEditCustomer))]
    private async Task EditCustomerAsync()
    {
        if (SelectedCustomer == null) return;
        
        await _navigationService.NavigateToAsync<CustomerDetailViewModel>(SelectedCustomer.Id);
    }
    
    private bool CanEditCustomer() => SelectedCustomer != null && !IsBusy;
    
    [RelayCommand]
    private async Task DeleteCustomerAsync(Customer customer)
    {
        if (customer == null) return;
        
        var confirmed = await _dialogService.ShowConfirmationAsync(
            $"Are you sure you want to delete {customer.FullName}?",
            "Confirm Delete");
            
        if (!confirmed) return;
        
        IsBusy = true;
        try
        {
            await _customerService.DeleteCustomerAsync(customer.Id);
            Customers.Remove(customer);
            await _dialogService.ShowInformationAsync("Customer deleted successfully");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Failed to delete customer", "Error", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

### 3. **View Creation Pattern**

```xml
<!-- File: Views/CustomerListView.xaml -->
<UserControl x:Class="YourProject.Views.CustomerListView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d" 
             d:DesignHeight="450" d:DesignWidth="800">
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Toolbar -->
        <ToolBar Grid.Row="0">
            <Button Content="Add Customer" Command="{Binding AddCustomerCommand}"/>
            <Button Content="Edit Customer" Command="{Binding EditCustomerCommand}"/>
            <Button Content="Delete Customer" Command="{Binding DeleteCustomerCommand}"
                    CommandParameter="{Binding SelectedCustomer}"/>
            <Separator/>
            <TextBox Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}" 
                     Width="200" ToolTip="Search customers..."/>
        </ToolBar>
        
        <!-- Data Grid -->
        <DataGrid Grid.Row="1" 
                  ItemsSource="{Binding Customers}"
                  SelectedItem="{Binding SelectedCustomer}"
                  AutoGenerateColumns="False"
                  CanUserAddRows="False">
            <DataGrid.Columns>
                <DataGridTextColumn Header="First Name" Binding="{Binding FirstName}"/>
                <DataGridTextColumn Header="Last Name" Binding="{Binding LastName}"/>
                <DataGridTextColumn Header="Email" Binding="{Binding Email}"/>
                <DataGridTextColumn Header="Phone" Binding="{Binding Phone}"/>
            </DataGrid.Columns>
        </DataGrid>
        
        <!-- Status Bar -->
        <StatusBar Grid.Row="2">
            <StatusBarItem>
                <TextBlock Text="{Binding Customers.Count, StringFormat='Total: {0} customers'}"/>
            </StatusBarItem>
            <StatusBarItem HorizontalAlignment="Right">
                <ProgressBar Width="100" Height="16" 
                             IsIndeterminate="{Binding IsBusy}" 
                             Visibility="{Binding IsBusy, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            </StatusBarItem>
        </StatusBar>
    </Grid>
</UserControl>
```

### 4. **Model Creation Pattern**

```csharp
// File: Models/Customer.cs
public class Customer
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required] 
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    public string? Phone { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}";
    
    // Helper methods
    public void MarkAsModified()
    {
        ModifiedDate = DateTime.UtcNow;
    }
}
```

### 5. **Validation Pattern**

```csharp
// File: Validators/CustomerValidator.cs
public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .Length(1, 50).WithMessage("First name must be 1-50 characters");
            
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .Length(1, 50).WithMessage("Last name must be 1-50 characters");
            
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .Must(BeUniqueEmail).WithMessage("Email already exists");
    }
    
    private bool BeUniqueEmail(string email)
    {
        // Implement uniqueness check
        return true; // Placeholder
    }
}

// ViewModel with validation
public partial class CustomerFormViewModel : FluentValidatableViewModelBase<CustomerFormViewModel>
{
    public CustomerFormViewModel(ICustomerService customerService, CustomerValidator validator) 
        : base(validator)
    {
        _customerService = customerService;
    }
    
    [ObservableProperty]
    private string firstName = string.Empty;
    
    [ObservableProperty] 
    private string lastName = string.Empty;
    
    // Validation happens automatically when properties change
}
```

## 🚨 Critical Anti-Patterns (NEVER DO THESE)

### ❌ **Static Dependencies**

```csharp
// WRONG - Breaks testability and DI
public class BadViewModel : ViewModelBase
{
    private void SomeMethod()
    {
        var service = App.Current.Services.GetService<IMyService>();
        // This is wrong!
    }
}

// CORRECT - Use dependency injection
public class GoodViewModel : ViewModelBase
{
    private readonly IMyService _service;
    
    public GoodViewModel(IMyService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }
}
```

### ❌ **UI Thread Violations**

```csharp
// WRONG - UI access from background thread
[RelayCommand]
private async Task BadLoadDataAsync()
{
    await Task.Run(() =>
    {
        // This will crash!
        Title = "Loading..."; 
    });
}

// CORRECT - Proper async pattern
[RelayCommand]
private async Task GoodLoadDataAsync()  
{
    IsBusy = true;
    try
    {
        var data = await Task.Run(() => GetDataFromService());
        // Back on UI thread - safe to update UI
        UpdateUIWithData(data);
    }
    finally
    {
        IsBusy = false;
    }
}
```

### ❌ **Missing Error Handling**

```csharp
// WRONG - No error handling
[RelayCommand]
private async Task BadSaveAsync()
{
    await _service.SaveDataAsync(data); // What if this fails?
}

// CORRECT - Comprehensive error handling
[RelayCommand]
private async Task GoodSaveAsync()
{
    IsBusy = true;
    try
    {
        await _service.SaveDataAsync(data);
        await _dialogService.ShowInformationAsync("Data saved successfully");
    }
    catch (ValidationException ex)
    {
        await _dialogService.ShowWarningAsync(ex.Message);
    }
    catch (Exception ex)
    {
        _loggingService.LogError(ex, "Failed to save data");
        await _dialogService.ShowErrorAsync("Failed to save data", "Error", ex);
    }
    finally
    {
        IsBusy = false;
    }
}
```

### ❌ **Memory Leaks**

```csharp
// WRONG - Subscription never disposed
public class BadViewModel : ViewModelBase
{
    public BadViewModel(IMessageBus messageBus)
    {
        messageBus.Subscribe<MyMessage>(OnMessage); // Memory leak!
    }
}

// CORRECT - Proper disposal
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

## 🧪 Testing Best Practices

### Always Create Tests for New Components

```csharp
// Test file: Tests/CustomerServiceTests.cs
public class CustomerServiceTests
{
    private readonly Mock<IRepository<Customer>> _mockRepository;
    private readonly Mock<ILoggingService> _mockLogging;
    private readonly CustomerService _service;
    
    public CustomerServiceTests()
    {
        _mockRepository = new Mock<IRepository<Customer>>();
        _mockLogging = new Mock<ILoggingService>();
        _service = new CustomerService(_mockRepository.Object, _mockLogging.Object);
    }
    
    [Fact]
    public async Task GetCustomersAsync_WhenCalled_ReturnsAllCustomers()
    {
        // Arrange
        var customers = new List<Customer> { new Customer { Id = 1, FirstName = "John" } };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(customers);
        
        // Act
        var result = await _service.GetCustomersAsync();
        
        // Assert
        Assert.Single(result);
        Assert.Equal("John", result[0].FirstName);
    }
    
    [Fact]
    public async Task GetCustomersAsync_WhenRepositoryThrows_LogsErrorAndRethrows()
    {
        // Arrange
        var exception = new InvalidOperationException("Database error");
        _mockRepository.Setup(r => r.GetAllAsync()).ThrowsAsync(exception);
        
        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetCustomersAsync());
            
        Assert.Same(exception, thrownException);
        _mockLogging.Verify(l => l.LogError(exception, "Failed to retrieve customers"), Times.Once);
    }
}
```

## 📚 Quick Reference Patterns

### **Command Patterns**

```csharp
// Simple command
[RelayCommand]
private async Task SimpleActionAsync() { }

// Command with parameter
[RelayCommand]
private async Task ActionWithParameterAsync(MyParameter param) { }

// Command with CanExecute
[RelayCommand(CanExecute = nameof(CanExecuteAction))]
private async Task ConditionalActionAsync() { }
private bool CanExecuteAction() => !IsBusy && SomeCondition;

// Command with progress
[RelayCommand]
private async Task ActionWithProgressAsync()
{
    IsBusy = true;
    var progress = new Progress<double>(p => ProgressValue = p);
    try
    {
        await _service.LongRunningOperationAsync(progress);
    }
    finally
    {
        IsBusy = false;
    }
}
```

### **Navigation Patterns**

```csharp
// Navigate to ViewModel
await _navigationService.NavigateToAsync<TargetViewModel>();

// Navigate with parameter
await _navigationService.NavigateToAsync<TargetViewModel>(parameter);

// Go back
if (_navigationService.CanGoBack)
    await _navigationService.GoBackAsync();
```

### **Dialog Patterns**

```csharp
// Confirmation
var result = await _dialogService.ShowConfirmationAsync("Are you sure?");

// Information
await _dialogService.ShowInformationAsync("Operation completed");

// Error with exception
await _dialogService.ShowErrorAsync("Operation failed", "Error", exception);

// File operations
var file = await _dialogService.ShowOpenFileDialogAsync("txt", "Text files|*.txt");
var savePath = await _dialogService.ShowSaveFileDialogAsync("txt", "Text files|*.txt");
```

### **Messaging Patterns**

```csharp
// Subscribe
var subscription = _messageBus.Subscribe<MyMessage>(OnMyMessage);

// Publish
_messageBus.Publish(new MyMessage { Data = "value" });

// Status messages
_messageBus.PublishStatus("Operation completed", StatusMessageType.Success);

// Progress messages
_messageBus.PublishProgress("Processing", 0.5, "50% complete");
```

## 🎯 Success Criteria

Your implementation is following best practices when:

✅ **All services use dependency injection**  
✅ **All ViewModels inherit from framework base classes**  
✅ **All async operations have proper error handling**  
✅ **All subscriptions are properly disposed**  
✅ **All new components have tests**  
✅ **UI thread is never blocked**  
✅ **IsBusy is used for long-running operations**  
✅ **Commands use RelayCommand attribute**  
✅ **Properties use ObservableProperty attribute**  
✅ **Validation uses framework patterns**  

Following these practices ensures **reliable, maintainable, and testable** code that integrates seamlessly with the WPFBase framework.