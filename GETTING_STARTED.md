# Getting Started with WPFBase

This guide will walk you through setting up and using the WPFBase framework to build modern WPF applications.

## 📋 Prerequisites

Before you begin, ensure you have:

- **Windows 10 or 11**
- **.NET 9.0 SDK** - [Download here](https://dotnet.microsoft.com/download)
- **IDE**: Visual Studio 2022, VS Code, or JetBrains Rider
- **Git** (optional) - for version control

### Verify Installation
```bash
# Check .NET version
dotnet --version

# Should show 9.0.x or later
```

## 🚀 Step 1: Get WPFBase

### Option A: Clone Repository
```bash
git clone https://github.com/yourusername/WPFBase.git
cd WPFBase
```

### Option B: Download ZIP
1. Download the ZIP file from GitHub
2. Extract to your project directory
3. Open terminal in the extracted folder

## 🛠️ Step 2: Build and Run

```bash
# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

**Expected Result**: A WPF window opens showing the sample application with navigation and theming.

## 📱 Step 3: Explore the Sample Application

The sample app demonstrates:
- **Navigation** between different views
- **Theme switching** (Light/Dark)
- **Dialog services** (info, error, confirmation)
- **Docking windows** (like Visual Studio)
- **MVVM patterns** with modern source generators

Try these features:
1. Click navigation buttons to switch views
2. Toggle between Light/Dark themes
3. Open dialogs and tool windows
4. Dock and undock windows

## 🏗️ Step 4: Create Your First Feature

Let's build a simple customer list feature to understand the framework patterns.

### 4.1 Create the Model
```csharp
// Models/Customer.cs
namespace WPFBase.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
```

### 4.2 Create the Service Interface
```csharp
// Interfaces/ICustomerService.cs
namespace WPFBase.Interfaces
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllAsync();
        Task<Customer> GetByIdAsync(int id);
        Task<Customer> CreateAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(int id);
    }
}
```

### 4.3 Create the Service Implementation
```csharp
// Services/CustomerService.cs
using WPFBase.Interfaces;
using WPFBase.Models;

namespace WPFBase.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly List<Customer> _customers = new();
        private int _nextId = 1;

        public CustomerService()
        {
            // Sample data
            _customers.AddRange(new[]
            {
                new Customer { Id = _nextId++, Name = "John Doe", Email = "john@example.com" },
                new Customer { Id = _nextId++, Name = "Jane Smith", Email = "jane@example.com" },
                new Customer { Id = _nextId++, Name = "Bob Johnson", Email = "bob@example.com" }
            });
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            // Simulate async operation
            await Task.Delay(100);
            return _customers.ToList();
        }

        public async Task<Customer> GetByIdAsync(int id)
        {
            await Task.Delay(50);
            return _customers.FirstOrDefault(c => c.Id == id)
                ?? throw new ArgumentException($"Customer with ID {id} not found");
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            await Task.Delay(100);
            customer.Id = _nextId++;
            customer.CreatedDate = DateTime.Now;
            _customers.Add(customer);
            return customer;
        }

        public async Task UpdateAsync(Customer customer)
        {
            await Task.Delay(100);
            var existing = await GetByIdAsync(customer.Id);
            existing.Name = customer.Name;
            existing.Email = customer.Email;
        }

        public async Task DeleteAsync(int id)
        {
            await Task.Delay(100);
            var customer = await GetByIdAsync(id);
            _customers.Remove(customer);
        }
    }
}
```

### 4.4 Create the ViewModel
```csharp
// ViewModels/CustomerListViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WPFBase.Interfaces;
using WPFBase.Models;

namespace WPFBase.ViewModels
{
    public partial class CustomerListViewModel : ViewModelBase
    {
        private readonly ICustomerService _customerService;
        private readonly IDialogService _dialogService;

        public CustomerListViewModel(ICustomerService customerService, IDialogService dialogService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            Title = "Customer List";
        }

        [ObservableProperty]
        private ObservableCollection<Customer> customers = new();

        [ObservableProperty]
        private Customer? selectedCustomer;

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
            try
            {
                var name = await _dialogService.ShowInputAsync("Enter customer name:", "Add Customer");
                if (string.IsNullOrWhiteSpace(name)) return;

                var email = await _dialogService.ShowInputAsync("Enter customer email:", "Add Customer");
                if (string.IsNullOrWhiteSpace(email)) return;

                var customer = new Customer { Name = name, Email = email };
                var created = await _customerService.CreateAsync(customer);
                Customers.Add(created);

                await _dialogService.ShowInformationAsync($"Customer '{name}' added successfully!", "Success");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Failed to add customer", "Error", ex);
            }
        }

        [RelayCommand(CanExecute = nameof(CanDeleteCustomer))]
        private async Task DeleteCustomerAsync()
        {
            if (SelectedCustomer == null) return;

            var result = await _dialogService.ShowConfirmationAsync(
                $"Are you sure you want to delete '{SelectedCustomer.Name}'?",
                "Confirm Delete");

            if (!result) return;

            try
            {
                await _customerService.DeleteAsync(SelectedCustomer.Id);
                Customers.Remove(SelectedCustomer);
                SelectedCustomer = null;

                await _dialogService.ShowInformationAsync("Customer deleted successfully!", "Success");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Failed to delete customer", "Error", ex);
            }
        }

        private bool CanDeleteCustomer() => SelectedCustomer != null;

        // Update command state when selection changes
        partial void OnSelectedCustomerChanged(Customer? value)
        {
            DeleteCustomerCommand.NotifyCanExecuteChanged();
        }
    }
}
```

### 4.5 Create the View
```xml
<!-- Views/CustomerListView.xaml -->
<UserControl x:Class="WPFBase.Views.CustomerListView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d"
             d:DesignHeight="450" d:DesignWidth="800">
    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Title -->
        <TextBlock Grid.Row="0" Text="Customer Management"
                   FontSize="24" FontWeight="Bold" Margin="0,0,0,20"/>

        <!-- Buttons -->
        <StackPanel Grid.Row="1" Orientation="Horizontal" Margin="0,0,0,20">
            <Button Content="Load Customers"
                    Command="{Binding LoadCustomersCommand}"
                    IsEnabled="{Binding IsNotBusy}"
                    MinWidth="120" Margin="0,0,10,0"/>

            <Button Content="Add Customer"
                    Command="{Binding AddCustomerCommand}"
                    IsEnabled="{Binding IsNotBusy}"
                    MinWidth="120" Margin="0,0,10,0"/>

            <Button Content="Delete Customer"
                    Command="{Binding DeleteCustomerCommand}"
                    MinWidth="120"/>
        </StackPanel>

        <!-- Customer List -->
        <DataGrid Grid.Row="2"
                  ItemsSource="{Binding Customers}"
                  SelectedItem="{Binding SelectedCustomer}"
                  AutoGenerateColumns="False"
                  IsReadOnly="True"
                  SelectionMode="Single">
            <DataGrid.Columns>
                <DataGridTextColumn Header="ID" Binding="{Binding Id}" Width="50"/>
                <DataGridTextColumn Header="Name" Binding="{Binding Name}" Width="*"/>
                <DataGridTextColumn Header="Email" Binding="{Binding Email}" Width="*"/>
                <DataGridTextColumn Header="Created" Binding="{Binding CreatedDate, StringFormat='{}{0:yyyy-MM-dd HH:mm}'}" Width="150"/>
            </DataGrid.Columns>
        </DataGrid>

        <!-- Loading Indicator -->
        <Border Grid.RowSpan="3"
                Background="Black"
                Opacity="0.3"
                Visibility="{Binding IsBusy, Converter={StaticResource BooleanToVisibilityConverter}}"/>

        <TextBlock Grid.RowSpan="3"
                   Text="Loading..."
                   FontSize="18"
                   FontWeight="Bold"
                   Foreground="White"
                   HorizontalAlignment="Center"
                   VerticalAlignment="Center"
                   Visibility="{Binding IsBusy, Converter={StaticResource BooleanToVisibilityConverter}}"/>
    </Grid>
</UserControl>
```

```csharp
// Views/CustomerListView.xaml.cs
namespace WPFBase.Views
{
    public partial class CustomerListView : UserControl
    {
        public CustomerListView()
        {
            InitializeComponent();
        }
    }
}
```

### 4.6 Register Services and Views
Add to `App.xaml.cs` in the `ConfigureServices` method:

```csharp
// Register the service
services.AddSingleton<ICustomerService, CustomerService>();

// Register the ViewModel and View
services.AddTransient<CustomerListViewModel>();
services.AddTransient<CustomerListView>();
```

### 4.7 Add Navigation
In your main ViewModel (e.g., `MainWindowViewModel`), add navigation:

```csharp
[RelayCommand]
private async Task ShowCustomersAsync()
{
    await _navigationService.NavigateToAsync<CustomerListViewModel>();
}
```

And in the main view, add a button:
```xml
<Button Content="Customers" Command="{Binding ShowCustomersCommand}"/>
```

## 🎯 Step 5: Test Your Feature

1. **Build and run** the application
2. **Click the Customers button** to navigate to your new view
3. **Load customers** to see the sample data
4. **Add a customer** using the Add button
5. **Select and delete** a customer
6. **Test error handling** by breaking the service temporarily

## 📚 Step 6: Learn More Patterns

Now that you have a working feature, explore these framework patterns:

### Validation
Add validation to your Customer model:
```csharp
public partial class CustomerEditViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(100)]
    private string name = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [EmailAddress]
    private string email = string.Empty;
}
```

### Theme Management
Switch themes programmatically:
```csharp
[RelayCommand]
private async Task ToggleThemeAsync()
{
    await _themeService.ToggleThemeAsync();
}
```

### Message Bus
Communicate between ViewModels:
```csharp
// Publish a message
_messageBus.Publish(new CustomerCreatedMessage { Customer = newCustomer });

// Subscribe to messages
_messageBus.Subscribe<CustomerCreatedMessage>(msg =>
{
    // Handle the message
    RefreshCustomerList();
});
```

### Docking Windows
Show your view as a dockable tool window:
```csharp
[RelayCommand]
private async Task ShowCustomerToolAsync()
{
    await _dockingService.ShowToolAsync(
        _serviceProvider.GetRequiredService<CustomerListViewModel>(),
        "Customer Management",
        AnchorableShowStrategy.Right);
}
```

## 🔍 Next Steps

1. **Read folder README files** for detailed documentation on each component
2. **Explore the sample code** in ViewModels and Services folders
3. **Check the test projects** to understand testing patterns
4. **Look at `/docs/development`** for advanced topics
5. **Build your own features** using the patterns you've learned

## 🆘 Troubleshooting

### Common Issues

**Build Errors:**
- Ensure .NET 9.0 SDK is installed
- Run `dotnet restore` to get packages
- Check for missing using statements

**Runtime Errors:**
- Verify services are registered in `App.xaml.cs`
- Check constructor dependencies are available
- Look at debug output for detailed error messages

**UI Not Updating:**
- Ensure ViewModels inherit from `ViewModelBase`
- Use `[ObservableProperty]` for data-bound properties
- Check XAML binding paths are correct

### Getting Help

1. **Check folder README files** for component-specific help
2. **Review sample code** for working examples
3. **Look at test projects** for usage patterns
4. **Report issues** on GitHub if you find bugs

## 🎉 Congratulations!

You've successfully:
- ✅ Set up the WPFBase framework
- ✅ Built your first MVVM feature
- ✅ Used modern C# patterns with source generators
- ✅ Integrated with the service layer
- ✅ Learned the core framework patterns

You're now ready to build robust WPF applications with modern architecture patterns!