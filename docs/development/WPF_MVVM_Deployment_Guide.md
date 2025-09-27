# WPF MVVM Application with CommunityToolkit - Deployment Guide

## Project Overview
This WPF application demonstrates a modern MVVM architecture using:
- **CommunityToolkit.Mvvm 8.4.0** - For MVVM implementation with source generators
- **Microsoft.Extensions.DependencyInjection 9.0.0** - For dependency injection
- **.NET 9.0** - Latest framework features

## Architecture Components

### 1. MVVM Structure
```
WPFBase/
├── Models/           # Data models and business entities
├── Views/            # XAML views (UserControls and Windows)
├── ViewModels/       # ViewModels with business logic
├── Services/         # Application services
├── Interfaces/       # Service interfaces
├── Converters/       # Value converters for data binding
├── Commands/         # Custom command implementations
└── Resources/        # Styles, templates, and resources
    └── Styles/
```

### 2. Key Components

#### ViewModelBase
- Base class for all ViewModels
- Inherits from `ObservableObject` (CommunityToolkit)
- Provides common properties: `IsBusy`, `Title`
- Navigation lifecycle methods: `OnNavigatedToAsync`, `OnNavigatedFromAsync`

#### NavigationService
- Manages view navigation
- Maintains navigation stack
- Convention-based View/ViewModel resolution
- Supports parameter passing between views

#### Dependency Injection
- Configured in `App.xaml.cs`
- ServiceProvider manages object lifetime
- Supports singleton and transient registrations

## CommunityToolkit.Mvvm Features Used

### 1. ObservableObject
Base class providing INotifyPropertyChanged implementation

### 2. ObservableProperty Attribute
```csharp
[ObservableProperty]
private string title = string.Empty;
// Generates: public string Title { get; set; }
```

### 3. RelayCommand Attribute
```csharp
[RelayCommand]
private void IncrementCounter()
{
    Counter++;
}
// Generates: public IRelayCommand IncrementCounterCommand { get; }
```

### 4. NotifyPropertyChangedFor
```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(IsNotBusy))]
private bool isBusy;
```

### 5. Partial Methods for Property Changes
```csharp
partial void OnUserInputChanged(string value)
{
    // Called when UserInput property changes
}
```

## Building the Application

### Prerequisites
- Visual Studio 2022 or later
- .NET 9.0 SDK
- Windows 10/11

### Build Steps
```bash
# Restore packages
dotnet restore

# Build Debug
dotnet build

# Build Release
dotnet build -c Release

# Run application
dotnet run
```

## Deployment Options

### 1. Framework-Dependent Deployment (FDD)
Smaller size, requires .NET runtime on target machine
```bash
dotnet publish -c Release -r win-x64 --self-contained false
```

### 2. Self-Contained Deployment (SCD)
Includes .NET runtime, larger size, no dependencies
```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

### 3. Single File Deployment
Everything in one executable
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### 4. ReadyToRun (R2R)
Pre-compiled for faster startup
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishReadyToRun=true
```

### 5. Trimmed Deployment
Removes unused code for smaller size
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishTrimmed=true
```

## Advanced Deployment Configuration

### Add to .csproj for deployment settings:
```xml
<PropertyGroup>
    <TargetFramework>net9.0-windows</TargetFramework>
    <OutputType>WinExe</OutputType>
    <UseWPF>true</UseWPF>
    <ApplicationIcon>app.ico</ApplicationIcon>
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <PublishReadyToRun>true</PublishReadyToRun>
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
</PropertyGroup>
```

## Creating an Installer

### Using Inno Setup
1. Install Inno Setup
2. Create setup script:
```iss
[Setup]
AppName=WPFBase MVVM Application
AppVersion=1.0
DefaultDirName={pf}\WPFBase
DefaultGroupName=WPFBase
OutputDir=installer
OutputBaseFilename=WPFBaseSetup

[Files]
Source: "bin\Release\net9.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\WPFBase"; Filename: "{app}\WPFBase.exe"
```

### Using MSIX
1. Add Windows Application Packaging Project
2. Configure Package.appxmanifest
3. Build and create MSIX package

## Adding New Features

### 1. Create New ViewModel
```csharp
public partial class NewFeatureViewModel : ViewModelBase
{
    [ObservableProperty]
    private string data = string.Empty;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsBusy = true;
        // Load data
        IsBusy = false;
    }
}
```

### 2. Create Corresponding View
```xml
<UserControl x:Class="WPFBase.Views.NewFeatureView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <Grid>
        <TextBlock Text="{Binding Data}"/>
    </Grid>
</UserControl>
```

### 3. Register in DI Container
```csharp
// In App.xaml.cs ConfigureServices method
services.AddTransient<NewFeatureViewModel>();
```

### 4. Navigate to New View
```csharp
await _navigationService.NavigateToAsync<NewFeatureViewModel>();
```

## Best Practices

### 1. ViewModel Guidelines
- Keep ViewModels testable - no UI dependencies
- Use async/await for long operations
- Implement IDisposable when needed
- Use ObservableCollection for dynamic lists

### 2. Data Binding
- Use UpdateSourceTrigger=PropertyChanged for immediate updates
- Implement value converters in Converters folder
- Use x:Bind in UWP/WinUI for better performance

### 3. Command Implementation
- Use RelayCommand from CommunityToolkit
- Implement CanExecute for command availability
- Call NotifyCanExecuteChanged when state changes

### 4. Service Design
- Define interfaces for all services
- Keep services focused on single responsibility
- Use async methods for I/O operations

### 5. Navigation
- Pass parameters via NavigateToAsync
- Clean up resources in OnNavigatedFromAsync
- Handle navigation failures gracefully

## Performance Optimization

### 1. Virtualization
Enable UI virtualization for large lists:
```xml
<ListBox VirtualizingStackPanel.IsVirtualizing="True"
         VirtualizingStackPanel.VirtualizationMode="Recycling"/>
```

### 2. Async Loading
Load data asynchronously to keep UI responsive:
```csharp
public override async Task OnNavigatedToAsync(object? parameter)
{
    IsBusy = true;
    await LoadDataAsync();
    IsBusy = false;
}
```

### 3. Weak Events
Use weak event patterns to prevent memory leaks

### 4. Lazy Loading
Load data only when needed:
```csharp
private Lazy<ObservableCollection<Item>> _items = 
    new(() => new ObservableCollection<Item>());

public ObservableCollection<Item> Items => _items.Value;
```

## Debugging Tips

### 1. Data Binding Debugging
Add to App.xaml.cs:
```csharp
PresentationTraceSources.SetTraceLevel(
    this, 
    PresentationTraceLevel.High);
```

### 2. Design-Time Data
Use d:DataContext for design-time data:
```xml
<UserControl d:DataContext="{d:DesignInstance Type=vm:HomeViewModel, 
                             IsDesignTimeCreatable=True}">
```

### 3. Command Debugging
Log command execution:
```csharp
[RelayCommand]
private void ExecuteCommand()
{
    Debug.WriteLine($"Command executed at {DateTime.Now}");
    // Command logic
}
```

## Testing

### 1. Unit Testing ViewModels
```csharp
[TestMethod]
public void IncrementCounter_ShouldIncreaseValue()
{
    var vm = new HomeViewModel();
    var initial = vm.Counter;
    
    vm.IncrementCounterCommand.Execute(null);
    
    Assert.AreEqual(initial + 1, vm.Counter);
}
```

### 2. Integration Testing
Test navigation and service integration

### 3. UI Testing
Use tools like Appium or WinAppDriver

## Troubleshooting

### Common Issues and Solutions

1. **Binding not working**
   - Check DataContext is set
   - Verify property names match
   - Check Output window for binding errors

2. **Commands not executing**
   - Ensure Command binding is correct
   - Check CanExecute returns true
   - Verify DataContext has the command

3. **Memory leaks**
   - Unsubscribe from events
   - Dispose resources properly
   - Use weak references where appropriate

4. **Navigation failures**
   - Check View/ViewModel naming convention
   - Ensure types are registered in DI
   - Verify assemblies are loaded

## Resources

- [CommunityToolkit.Mvvm Documentation](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [WPF Best Practices](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/advanced/wpf-best-practices)
- [.NET Deployment Guide](https://docs.microsoft.com/en-us/dotnet/core/deploying/)
- [MVVM Pattern](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/enterprise-application-patterns/mvvm)

## Version History

### v1.0.0 (Current)
- Initial MVVM structure
- CommunityToolkit.Mvvm integration
- Navigation service implementation
- Sample Home view with demos
- Dependency injection setup