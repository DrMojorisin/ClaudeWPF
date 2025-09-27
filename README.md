# WPFBase - Modern WPF MVVM Framework

A production-ready WPF MVVM framework built with modern .NET practices and comprehensive architecture patterns.

## 🚀 Features

### Core Architecture
- **Modern MVVM** with CommunityToolkit.Mvvm source generators
- **Dependency Injection** using Microsoft.Extensions.DependencyInjection
- **Async/Await** patterns throughout
- **Repository Pattern** with Unit of Work for data access

### UI & UX
- **AvalonDock** - Visual Studio-style docking windows
- **Theme Management** - Light/Dark themes with runtime switching
- **Value Converters** - Comprehensive library of XAML converters
- **Validation** - Both DataAnnotations and FluentValidation support
- **Custom Controls** - Reusable WPF controls

### Services
- **Navigation Service** - Type-safe navigation with parameters
- **Dialog Service** - Unified dialog management (messages, files, custom)
- **Message Bus** - Decoupled pub/sub communication
- **Configuration Service** - JSON-based configuration
- **User Settings** - Persistent user preferences
- **Keyboard Shortcuts** - Customizable keyboard shortcuts
- **Logging** - Structured logging with Serilog
- **Theme Service** - Runtime theme switching

## 📁 Project Structure

```
WPFBase/
├── Commands/           # Custom ICommand implementations
├── Controls/           # Reusable WPF controls
├── Converters/         # XAML value converters
├── Data/              # Repository and data access
├── Extensions/        # C# extension methods
├── Interfaces/        # Service contracts
├── Models/           # Data models and DTOs
├── Resources/        # Icons, images, assets
├── Services/         # Service implementations
├── Themes/          # Application themes and styles
├── Validators/      # FluentValidation validators
├── ViewModels/      # MVVM ViewModels
├── Views/           # XAML views and code-behind
└── scripts/         # Build and utility scripts
```

Each folder contains a README.md explaining its purpose and usage patterns.

## 🛠️ Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2022 / VS Code / Rider
- Windows 10/11

## 🚀 Quick Start

### 1. Clone or Download
```bash
git clone https://github.com/yourusername/WPFBase.git
cd WPFBase
```

### 2. Build and Run
```bash
# Restore packages
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

### 3. Create Your First Feature

**Create a ViewModel:**
```csharp
public partial class MyViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;

    public MyViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        Title = "My Feature";
    }

    [ObservableProperty]
    private string message = "Hello World!";

    [RelayCommand]
    private async Task ShowMessageAsync()
    {
        await _dialogService.ShowInformationAsync(Message, "Info");
    }
}
```

**Create a View:**
```xml
<UserControl x:Class="WPFBase.Views.MyView">
    <StackPanel Margin="20">
        <TextBox Text="{Binding Message, UpdateSourceTrigger=PropertyChanged}" />
        <Button Content="Show Message" Command="{Binding ShowMessageCommand}" />
    </StackPanel>
</UserControl>
```

**Register in DI Container (App.xaml.cs):**
```csharp
services.AddTransient<MyViewModel>();
services.AddTransient<MyView>();
```

## 🔧 Key Technologies

- **CommunityToolkit.Mvvm 8.4.0** - Modern MVVM with source generators
- **AvalonDock** - Professional docking windows
- **Serilog** - Structured logging
- **FluentValidation** - Business rule validation
- **WPF-UI** - Modern UI components
- **xUnit + Moq** - Comprehensive testing

## 📚 Architecture Patterns

### MVVM with Source Generators
Uses modern CommunityToolkit.Mvvm patterns:
- `[ObservableProperty]` for automatic property generation
- `[RelayCommand]` for automatic command generation
- `ObservableValidator` for validation support

### Service Layer
All services follow interface-based design:
- Easy to test with mocking
- Dependency injection throughout
- Clear separation of concerns

### Navigation & Dialogs
Type-safe navigation and comprehensive dialog system:
```csharp
// Navigate to a view
await _navigationService.NavigateToAsync<CustomerViewModel>();

// Show dialogs
await _dialogService.ShowInformationAsync("Message", "Title");
var result = await _dialogService.ShowConfirmationAsync("Are you sure?");
```

## 🧪 Testing

The framework includes comprehensive unit tests:

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific tests
dotnet test --filter "NavigationServiceTests"
```

## 📖 Documentation

Each folder contains detailed README files explaining:
- Purpose and usage
- Code examples
- Best practices
- Common patterns

## 🎯 Use Cases

This framework is perfect for:
- Enterprise desktop applications
- Document-based applications (with docking)
- Configuration tools and utilities
- Data management applications
- Rapid prototyping with production-ready code

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## 📄 License

MIT License - Use freely in commercial and open-source projects.

## 🆘 Support

- **Documentation**: Check folder README files for detailed usage
- **Issues**: Report bugs and feature requests in GitHub Issues
- **Examples**: See the included sample ViewModels and Views

---

**Built with modern .NET practices for robust, maintainable WPF applications.**