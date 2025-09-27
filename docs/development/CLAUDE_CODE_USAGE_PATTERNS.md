# Claude Code Framework Usage Patterns for WPFBase

## 🎯 Essential Patterns for Claude Code Automation

This guide provides proven patterns that Claude Code should follow when working with the WPFBase framework to ensure consistent, reliable, and maintainable code generation.

## 📋 Pattern Categories

### 1. **MVVM Component Creation Patterns**

#### **Pattern A: List/Master View Pattern**
```csharp
// ViewModels/[Entity]ListViewModel.cs
public partial class CustomerListViewModel : ViewModelBase
{
    private readonly ICustomerService _customerService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    
    public CustomerListViewModel(
        ICustomerService customerService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        
        Title = "Customer List";
        Customers = new ObservableCollection<Customer>();
        _ = LoadCustomersAsync();
    }
    
    [ObservableProperty]
    private ObservableCollection<Customer> customers = new();
    
    [ObservableProperty]
    private Customer? selectedCustomer;
    
    [ObservableProperty]
    private string searchText = string.Empty;
    
    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        IsBusy = true;
        try
        {
            var customerList = await _customerService.GetAllAsync();
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
    
    [RelayCommand]
    private async Task AddCustomerAsync()
    {
        await _navigationService.NavigateToAsync<CustomerEditViewModel>();
    }
    
    [RelayCommand(CanExecute = nameof(HasSelectedCustomer))]
    private async Task EditCustomerAsync()
    {
        if (SelectedCustomer == null) return;
        await _navigationService.NavigateToAsync<CustomerEditViewModel>(SelectedCustomer.Id);
    }
    
    [RelayCommand(CanExecute = nameof(HasSelectedCustomer))]
    private async Task DeleteCustomerAsync()
    {
        if (SelectedCustomer == null) return;
        
        var confirmed = await _dialogService.ShowConfirmationAsync(
            $"Are you sure you want to delete {SelectedCustomer.FullName}?",
            "Confirm Delete");
            
        if (!confirmed) return;
        
        IsBusy = true;
        try
        {
            await _customerService.DeleteAsync(SelectedCustomer.Id);
            Customers.Remove(SelectedCustomer);
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
    
    private bool HasSelectedCustomer() => SelectedCustomer != null && !IsBusy;
    
    partial void OnSearchTextChanged(string value)
    {
        _ = SearchCustomersAsync(value);
    }
    
    private async Task SearchCustomersAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            await LoadCustomersAsync();
            return;
        }
        
        IsBusy = true;
        try
        {
            var filteredCustomers = await _customerService.SearchAsync(searchTerm);
            Customers.Clear();
            foreach (var customer in filteredCustomers)
            {
                Customers.Add(customer);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Search failed", "Error", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

#### **Pattern B: Edit/Detail Form Pattern**
```csharp
// ViewModels/[Entity]EditViewModel.cs
public partial class CustomerEditViewModel : ValidatableViewModelBase
{
    private readonly ICustomerService _customerService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private int? _customerId;
    
    public CustomerEditViewModel(
        ICustomerService customerService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        
        Title = "Edit Customer";
    }
    
    private string _firstName = string.Empty;
    [Required]
    [StringLength(50)]
    public string FirstName
    {
        get => _firstName;
        set => SetPropertyWithValidation(ref _firstName, value);
    }
    
    private string _lastName = string.Empty;
    [Required]
    [StringLength(50)]
    public string LastName
    {
        get => _lastName;
        set => SetPropertyWithValidation(ref _lastName, value);
    }
    
    private string _email = string.Empty;
    [Required]
    [EmailAddress]
    public string Email
    {
        get => _email;
        set => SetPropertyWithValidation(ref _email, value);
    }
    
    private string? _phone;
    [Phone]
    public string? Phone
    {
        get => _phone;
        set => SetPropertyWithValidation(ref _phone, value);
    }
    
    protected override async Task OnNavigatedToAsync(object? parameter)
    {
        if (parameter is int id)
        {
            _customerId = id;
            Title = "Edit Customer";
            await LoadCustomerAsync(id);
        }
        else
        {
            _customerId = null;
            Title = "Add Customer";
        }
    }
    
    private async Task LoadCustomerAsync(int id)
    {
        IsBusy = true;
        try
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer != null)
            {
                FirstName = customer.FirstName;
                LastName = customer.LastName;
                Email = customer.Email;
                Phone = customer.Phone;
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Failed to load customer", "Error", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (!ValidateAllProperties())
        {
            await _dialogService.ShowWarningAsync("Please fix validation errors before saving");
            return;
        }
        
        IsBusy = true;
        try
        {
            var customer = new Customer
            {
                Id = _customerId ?? 0,
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Phone = Phone
            };
            
            if (_customerId.HasValue)
            {
                await _customerService.UpdateAsync(customer);
            }
            else
            {
                await _customerService.CreateAsync(customer);
            }
            
            await _dialogService.ShowInformationAsync("Customer saved successfully");
            await _navigationService.GoBackAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Failed to save customer", "Error", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private async Task CancelAsync()
    {
        if (HasChanges())
        {
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "You have unsaved changes. Are you sure you want to cancel?",
                "Confirm Cancel");
            if (!confirmed) return;
        }
        
        await _navigationService.GoBackAsync();
    }
    
    private bool CanSave() => !HasErrors && !IsBusy;
    private bool HasChanges() => !string.IsNullOrWhiteSpace(FirstName) || 
                                !string.IsNullOrWhiteSpace(LastName) || 
                                !string.IsNullOrWhiteSpace(Email);
}
```

### 2. **Service Implementation Patterns**

#### **Pattern C: CRUD Service Pattern**
```csharp
// Interfaces/I[Entity]Service.cs
public interface ICustomerService
{
    Task<List<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync(int id);
    Task<List<Customer>> SearchAsync(string searchTerm);
    Task<Customer> CreateAsync(Customer customer);
    Task<Customer> UpdateAsync(Customer customer);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

// Services/[Entity]Service.cs
public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _repository;
    private readonly ILoggingService _logging;
    private readonly IMessageBus _messageBus;
    
    public CustomerService(
        IRepository<Customer> repository,
        ILoggingService logging,
        IMessageBus messageBus)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logging = logging ?? throw new ArgumentNullException(nameof(logging));
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
    }
    
    public async Task<List<Customer>> GetAllAsync()
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
    
    public async Task<Customer?> GetByIdAsync(int id)
    {
        if (id <= 0) throw new ArgumentException("ID must be positive", nameof(id));
        
        try
        {
            _logging.LogInformation("Retrieving customer with ID: {CustomerId}", id);
            return await _repository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logging.LogError(ex, "Failed to retrieve customer {CustomerId}", id);
            throw;
        }
    }
    
    public async Task<List<Customer>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();
        
        try
        {
            _logging.LogInformation("Searching customers with term: {SearchTerm}", searchTerm);
            var allCustomers = await _repository.GetAllAsync();
            return allCustomers.Where(c => 
                c.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        catch (Exception ex)
        {
            _logging.LogError(ex, "Failed to search customers");
            throw;
        }
    }
    
    public async Task<Customer> CreateAsync(Customer customer)
    {
        if (customer == null) throw new ArgumentNullException(nameof(customer));
        
        try
        {
            _logging.LogInformation("Creating new customer: {CustomerName}", customer.FullName);
            var created = await _repository.CreateAsync(customer);
            
            _messageBus.PublishStatus($"Customer {created.FullName} created successfully", 
                StatusMessageType.Success);
            
            return created;
        }
        catch (Exception ex)
        {
            _logging.LogError(ex, "Failed to create customer");
            throw;
        }
    }
    
    public async Task<Customer> UpdateAsync(Customer customer)
    {
        if (customer == null) throw new ArgumentNullException(nameof(customer));
        if (customer.Id <= 0) throw new ArgumentException("Customer ID must be positive");
        
        try
        {
            _logging.LogInformation("Updating customer {CustomerId}: {CustomerName}", 
                customer.Id, customer.FullName);
            
            var updated = await _repository.UpdateAsync(customer);
            
            _messageBus.PublishStatus($"Customer {updated.FullName} updated successfully", 
                StatusMessageType.Success);
            
            return updated;
        }
        catch (Exception ex)
        {
            _logging.LogError(ex, "Failed to update customer {CustomerId}", customer.Id);
            throw;
        }
    }
    
    public async Task DeleteAsync(int id)
    {
        if (id <= 0) throw new ArgumentException("ID must be positive", nameof(id));
        
        try
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null)
                throw new InvalidOperationException($"Customer with ID {id} not found");
            
            _logging.LogInformation("Deleting customer {CustomerId}: {CustomerName}", 
                id, customer.FullName);
            
            await _repository.DeleteAsync(id);
            
            _messageBus.PublishStatus($"Customer {customer.FullName} deleted successfully", 
                StatusMessageType.Success);
        }
        catch (Exception ex)
        {
            _logging.LogError(ex, "Failed to delete customer {CustomerId}", id);
            throw;
        }
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        if (id <= 0) return false;
        
        try
        {
            var customer = await _repository.GetByIdAsync(id);
            return customer != null;
        }
        catch (Exception ex)
        {
            _logging.LogError(ex, "Failed to check if customer {CustomerId} exists", id);
            throw;
        }
    }
}
```

### 3. **Document/Docking Patterns**

#### **Pattern D: Document ViewModel Pattern**
```csharp
// ViewModels/Documents/[DocumentType]DocumentViewModel.cs
public partial class TextDocumentViewModel : DocumentViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IFileService _fileService;
    
    [ObservableProperty]
    private string content = string.Empty;
    
    [ObservableProperty]
    private string fileName = "Untitled";
    
    public TextDocumentViewModel(IDialogService dialogService, IFileService fileService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        
        Title = "Untitled";
    }
    
    partial void OnContentChanged(string value)
    {
        IsDirty = true;
    }
    
    public override async Task<bool> SaveAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                var path = await _dialogService.ShowSaveFileDialogAsync(
                    "txt", "Text files|*.txt|All files|*.*");
                if (string.IsNullOrEmpty(path)) return false;
                
                FilePath = path;
                FileName = Path.GetFileName(path);
                Title = FileName;
            }
            
            await _fileService.WriteAllTextAsync(FilePath, Content);
            IsDirty = false;
            
            return true;
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Failed to save file: {ex.Message}", "Save Error");
            return false;
        }
    }
    
    public override async Task<bool> SaveAsAsync()
    {
        try
        {
            var path = await _dialogService.ShowSaveFileDialogAsync(
                "txt", "Text files|*.txt|All files|*.*");
            if (string.IsNullOrEmpty(path)) return false;
            
            FilePath = path;
            FileName = Path.GetFileName(path);
            Title = FileName;
            
            return await SaveAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Failed to save file: {ex.Message}", "Save Error");
            return false;
        }
    }
    
    public override async Task<bool> LoadAsync(string filePath)
    {
        try
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            Title = FileName;
            
            Content = await _fileService.ReadAllTextAsync(filePath);
            IsDirty = false;
            
            return true;
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Failed to load file: {ex.Message}", "Load Error");
            return false;
        }
    }
    
    public override async Task<bool> CanCloseAsync()
    {
        if (!IsDirty) return true;
        
        var result = await _dialogService.ShowYesNoCancelAsync(
            $"Do you want to save changes to {FileName}?",
            "Save Changes");
        
        return result switch
        {
            DialogResult.Yes => await SaveAsync(),
            DialogResult.No => true,
            _ => false
        };
    }
}
```

### 4. **Tool Window Patterns**

#### **Pattern E: Tool Window ViewModel Pattern**
```csharp
// ViewModels/Tools/[ToolName]ToolViewModel.cs
public partial class PropertiesToolViewModel : ViewModelBase
{
    private readonly IMessageBus _messageBus;
    private readonly List<IDisposable> _subscriptions = new();
    
    [ObservableProperty]
    private object? selectedObject;
    
    [ObservableProperty]
    private ObservableCollection<PropertyInfo> properties = new();
    
    public PropertiesToolViewModel(IMessageBus messageBus)
    {
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        
        Title = "Properties";
        
        _subscriptions.Add(_messageBus.Subscribe<SelectionChangedMessage>(OnSelectionChanged));
    }
    
    private void OnSelectionChanged(SelectionChangedMessage message)
    {
        SelectedObject = message.SelectedObject;
        UpdateProperties();
    }
    
    private void UpdateProperties()
    {
        Properties.Clear();
        
        if (SelectedObject == null) return;
        
        var type = SelectedObject.GetType();
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .OrderBy(p => p.Name);
        
        foreach (var prop in props)
        {
            Properties.Add(new PropertyInfo
            {
                Name = prop.Name,
                Type = prop.PropertyType.Name,
                Value = GetPropertyValue(prop)
            });
        }
    }
    
    private string GetPropertyValue(PropertyInfo property)
    {
        try
        {
            var value = property.GetValue(SelectedObject);
            return value?.ToString() ?? "<null>";
        }
        catch
        {
            return "<error>";
        }
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

### 5. **Validation Patterns**

#### **Pattern F: Complex Form Validation Pattern**
```csharp
// Validators/[Entity]Validator.cs
public class CustomerValidator : AbstractValidator<CustomerEditViewModel>
{
    public CustomerValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .Length(1, 50).WithMessage("First name must be 1-50 characters")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("First name contains invalid characters");
            
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .Length(1, 50).WithMessage("Last name must be 1-50 characters")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("Last name contains invalid characters");
            
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email must be less than 100 characters");
            
        RuleFor(x => x.Phone)
            .Matches(@"^\+?[\d\s()-]+$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}

// ViewModels with FluentValidation
public partial class CustomerEditViewModel : FluentValidatableViewModelBase<CustomerEditViewModel>
{
    public CustomerEditViewModel(
        ICustomerService customerService,
        CustomerValidator validator) : base(validator)
    {
        // Constructor implementation
    }
    
    // Properties automatically validated by FluentValidation
    [ObservableProperty]
    private string firstName = string.Empty;
    
    [ObservableProperty]
    private string lastName = string.Empty;
    
    [ObservableProperty]
    private string email = string.Empty;
    
    [ObservableProperty]
    private string? phone;
}
```

### 6. **Progress and Status Patterns**

#### **Pattern G: Long-Running Operation Pattern**
```csharp
[RelayCommand]
private async Task ProcessLargeDatasetAsync()
{
    IsBusy = true;
    var progress = new Progress<ProgressInfo>(OnProgressUpdate);
    
    try
    {
        _messageBus.PublishStatus("Starting data processing...", StatusMessageType.Information);
        
        await _dataService.ProcessLargeDatasetAsync(progress, CancellationToken.None);
        
        _messageBus.PublishStatus("Data processing completed successfully", StatusMessageType.Success);
    }
    catch (OperationCanceledException)
    {
        _messageBus.PublishStatus("Data processing was cancelled", StatusMessageType.Warning);
    }
    catch (Exception ex)
    {
        _loggingService.LogError(ex, "Failed to process large dataset");
        _messageBus.PublishStatus("Data processing failed", StatusMessageType.Error);
        await _dialogService.ShowErrorAsync("Failed to process data", "Error", ex);
    }
    finally
    {
        IsBusy = false;
    }
}

private void OnProgressUpdate(ProgressInfo progress)
{
    _messageBus.PublishProgress(progress.Status, progress.Percentage, progress.Details);
}
```

### 7. **Registration Patterns**

#### **Pattern H: Service Registration Pattern**
```csharp
// App.xaml.cs - ConfigureServices method
private void ConfigureServices(IServiceCollection services)
{
    // Core Framework Services (already registered)
    
    // Entity Services
    services.AddSingleton<ICustomerService, CustomerService>();
    services.AddSingleton<IOrderService, OrderService>();
    services.AddSingleton<IProductService, ProductService>();
    
    // Repositories
    services.AddSingleton<IRepository<Customer>, InMemoryRepository<Customer>>();
    services.AddSingleton<IRepository<Order>, InMemoryRepository<Order>>();
    services.AddSingleton<IRepository<Product>, InMemoryRepository<Product>>();
    
    // ViewModels - List Views
    services.AddTransient<CustomerListViewModel>();
    services.AddTransient<OrderListViewModel>();
    services.AddTransient<ProductListViewModel>();
    
    // ViewModels - Edit Views
    services.AddTransient<CustomerEditViewModel>();
    services.AddTransient<OrderEditViewModel>();
    services.AddTransient<ProductEditViewModel>();
    
    // ViewModels - Documents
    services.AddTransient<TextDocumentViewModel>();
    services.AddTransient<SpreadsheetDocumentViewModel>();
    
    // ViewModels - Tools
    services.AddTransient<PropertiesToolViewModel>();
    services.AddTransient<OutputToolViewModel>();
    
    // Views
    services.AddTransient<CustomerListView>();
    services.AddTransient<CustomerEditView>();
    services.AddTransient<TextDocumentView>();
    services.AddTransient<PropertiesToolView>();
    
    // Validators
    services.AddSingleton<CustomerValidator>();
    services.AddSingleton<OrderValidator>();
}
```

### 8. **Testing Patterns**

#### **Pattern I: ViewModel Testing Pattern**
```csharp
// Tests/ViewModels/[Entity]ViewModelTests.cs
public class CustomerListViewModelTests
{
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly CustomerListViewModel _viewModel;
    
    public CustomerListViewModelTests()
    {
        _mockCustomerService = new Mock<ICustomerService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockNavigationService = new Mock<INavigationService>();
        
        _viewModel = new CustomerListViewModel(
            _mockCustomerService.Object,
            _mockDialogService.Object,
            _mockNavigationService.Object);
    }
    
    [Fact]
    public async Task LoadCustomersCommand_ShouldPopulateCustomers()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com" },
            new() { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" }
        };
        _mockCustomerService.Setup(x => x.GetAllAsync()).ReturnsAsync(customers);
        
        // Act
        await _viewModel.LoadCustomersCommand.ExecuteAsync(null);
        
        // Assert
        Assert.Equal(2, _viewModel.Customers.Count);
        Assert.Equal("John", _viewModel.Customers[0].FirstName);
        Assert.Equal("Jane", _viewModel.Customers[1].FirstName);
        Assert.False(_viewModel.IsBusy);
    }
    
    [Fact]
    public async Task DeleteCustomerCommand_WithConfirmation_ShouldDeleteCustomer()
    {
        // Arrange
        var customer = new Customer { Id = 1, FirstName = "John", LastName = "Doe" };
        _viewModel.Customers.Add(customer);
        _viewModel.SelectedCustomer = customer;
        _mockDialogService.Setup(x => x.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        
        // Act
        await _viewModel.DeleteCustomerCommand.ExecuteAsync(null);
        
        // Assert
        _mockCustomerService.Verify(x => x.DeleteAsync(1), Times.Once);
        Assert.Empty(_viewModel.Customers);
    }
    
    [Fact]
    public async Task DeleteCustomerCommand_WithoutConfirmation_ShouldNotDeleteCustomer()
    {
        // Arrange
        var customer = new Customer { Id = 1, FirstName = "John", LastName = "Doe" };
        _viewModel.Customers.Add(customer);
        _viewModel.SelectedCustomer = customer;
        _mockDialogService.Setup(x => x.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        
        // Act
        await _viewModel.DeleteCustomerCommand.ExecuteAsync(null);
        
        // Assert
        _mockCustomerService.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        Assert.Single(_viewModel.Customers);
    }
}
```

### 9. **XAML View Patterns**

#### **Pattern J: List View XAML Pattern**
```xml
<!-- Views/[Entity]ListView.xaml -->
<UserControl x:Class="MyApp.Views.CustomerListView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d">
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Toolbar -->
        <ToolBar Grid.Row="0">
            <Button Content="Add" Command="{Binding AddCustomerCommand}"/>
            <Button Content="Edit" Command="{Binding EditCustomerCommand}"/>
            <Button Content="Delete" Command="{Binding DeleteCustomerCommand}"/>
            <Separator/>
            <Button Content="Refresh" Command="{Binding LoadCustomersCommand}"/>
        </ToolBar>
        
        <!-- Search -->
        <Grid Grid.Row="1" Margin="5">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>
            
            <TextBlock Text="Search:" VerticalAlignment="Center" Margin="0,0,5,0"/>
            <TextBox Grid.Column="1" Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"/>
            <Button Grid.Column="2" Content="Clear" Command="{Binding ClearSearchCommand}" Margin="5,0,0,0"/>
        </Grid>
        
        <!-- Data Grid -->
        <DataGrid Grid.Row="2" 
                  ItemsSource="{Binding Customers}"
                  SelectedItem="{Binding SelectedCustomer}"
                  AutoGenerateColumns="False"
                  CanUserAddRows="False"
                  IsReadOnly="True">
            <DataGrid.Columns>
                <DataGridTextColumn Header="First Name" Binding="{Binding FirstName}" Width="*"/>
                <DataGridTextColumn Header="Last Name" Binding="{Binding LastName}" Width="*"/>
                <DataGridTextColumn Header="Email" Binding="{Binding Email}" Width="2*"/>
                <DataGridTextColumn Header="Phone" Binding="{Binding Phone}" Width="*"/>
            </DataGrid.Columns>
        </DataGrid>
        
        <!-- Status Bar -->
        <StatusBar Grid.Row="3">
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

## 🎯 Quick Reference for Claude Code

When Claude Code creates new components in this framework, always follow these patterns:

1. **ViewModels**: Use appropriate base class (ViewModelBase, ValidatableViewModelBase, DocumentViewModelBase)
2. **Services**: Create interface first, implement with proper error handling and logging
3. **Registration**: Always register services and ViewModels in App.xaml.cs
4. **Error Handling**: Use try-catch-finally with IsBusy management
5. **Validation**: Use DataAnnotations or FluentValidation consistently
6. **Navigation**: Use INavigationService, never direct view instantiation
7. **Dialogs**: Use IDialogService, never MessageBox directly
8. **Messaging**: Use IMessageBus for decoupled communication
9. **Resources**: Always dispose subscriptions and resources
10. **Testing**: Create corresponding test files for all new components

These patterns ensure consistent, maintainable, and reliable code that integrates seamlessly with the WPFBase framework.