# WPFBase Framework Integration Guide

## 🚀 Quick Integration Steps

### Step 1: Copy Framework Files
```bash
# Copy the entire WPFBase directory to your project
cp -r WPFBase/ /path/to/your/project/MyApp/
```

### Step 2: Update Project Configuration

**Update `MyApp.csproj`:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net9.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>
    <StartupObject>MyApp.App</StartupObject> <!-- Update namespace -->
  </PropertyGroup>

  <!-- Copy all PackageReference items from WPFBase.csproj -->
  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="9.0.0" />
    <PackageReference Include="Serilog" Version="4.1.0" />
    <PackageReference Include="Serilog.Extensions.Logging" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.Debug" Version="3.0.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
    <PackageReference Include="Dirkster.AvalonDock" Version="4.72.1" />
    <PackageReference Include="Dirkster.AvalonDock.Themes.VS2013" Version="4.72.1" />
    <PackageReference Include="ValueConverters" Version="3.0.26" />
    <PackageReference Include="WPF-UI" Version="3.0.5" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="coverlet.collector" Version="6.0.2" />
    <PackageReference Include="FluentValidation" Version="11.10.0" />
  </ItemGroup>
</Project>
```

### Step 3: Update Namespaces (Claude Code Task)

**Ask Claude Code to run this namespace update:**

"Update all namespace declarations in the copied WPFBase files from `WPFBase` to `MyApp` (or your project namespace)"

**Files requiring namespace updates:**
- All `.cs` files
- All `.xaml` files (`x:Class` attributes)
- `App.xaml.cs` StartupObject reference

### Step 4: Configure Your Application

**Update `App.xaml.cs`:**
```csharp
namespace MyApp // Updated namespace
{
    public partial class App : Application
    {
        // Framework handles everything - no changes needed to methods
        // Just update the namespace
    }
}
```

### Step 5: Build and Run
```bash
dotnet restore
dotnet build
dotnet run
```

## 🔧 Customization Options

### Add Your Business Services

**1. Create Service Interface:**
```csharp
// MyApp/Interfaces/IMyBusinessService.cs
namespace MyApp.Interfaces
{
    public interface IMyBusinessService
    {
        Task<List<MyEntity>> GetDataAsync();
    }
}
```

**2. Create Service Implementation:**
```csharp
// MyApp/Services/MyBusinessService.cs
namespace MyApp.Services
{
    public class MyBusinessService : IMyBusinessService
    {
        private readonly ILoggingService _logging;
        
        public MyBusinessService(ILoggingService logging)
        {
            _logging = logging ?? throw new ArgumentNullException(nameof(logging));
        }
        
        public async Task<List<MyEntity>> GetDataAsync()
        {
            try
            {
                _logging.LogInformation("Loading business data");
                // Your implementation
                return data;
            }
            catch (Exception ex)
            {
                _logging.LogError(ex, "Failed to load business data");
                throw;
            }
        }
    }
}
```

**3. Register in DI Container:**
```csharp
// App.xaml.cs - ConfigureServices method
private void ConfigureServices(IServiceCollection services)
{
    // Framework services are already registered
    
    // Add your business services here
    services.AddSingleton<IMyBusinessService, MyBusinessService>();
    services.AddTransient<MyBusinessViewModel>();
    services.AddTransient<MyBusinessView>();
}
```

### Add Your ViewModels

```csharp
// MyApp/ViewModels/MyBusinessViewModel.cs
namespace MyApp.ViewModels
{
    public partial class MyBusinessViewModel : ViewModelBase
    {
        private readonly IMyBusinessService _businessService;
        private readonly IDialogService _dialogService;
        
        public MyBusinessViewModel(
            IMyBusinessService businessService,
            IDialogService dialogService)
        {
            _businessService = businessService ?? throw new ArgumentNullException(nameof(businessService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            
            Title = "My Business Feature";
        }
        
        [ObservableProperty]
        private ObservableCollection<MyEntity> items = new();
        
        [RelayCommand]
        private async Task LoadDataAsync()
        {
            IsBusy = true;
            try
            {
                var data = await _businessService.GetDataAsync();
                Items.Clear();
                foreach (var item in data)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Failed to load data", "Error", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
```

## 📁 Recommended Project Structure

```
MyApp/
├── Commands/           # Framework commands (copied from WPFBase)
├── Controls/           # Framework controls + your custom controls
├── Converters/         # Framework converters + your custom converters
├── Data/              # Framework repository pattern + your data models
├── Extensions/        # Framework extensions + your extensions
├── Interfaces/        # Framework interfaces + your business interfaces
├── Models/           
│   ├── Business/      # Your business models
│   ├── Examples/      # Framework examples (can remove in production)
│   └── Messages/      # Framework messages + your custom messages
├── Resources/         # Framework resources + your resources
├── Services/          # Framework services + your business services
├── Themes/           # Framework themes + your custom themes
├── Validators/       # Framework validators + your business validators
├── ViewModels/       # Framework ViewModels + your ViewModels
│   ├── Business/     # Your business ViewModels
│   ├── Documents/    # Framework + your documents
│   └── Tools/        # Framework + your tools
├── Views/            # Framework views + your views
│   ├── Business/     # Your business views
│   ├── Documents/    # Framework + your documents
│   └── Tools/        # Framework + your tools
└── Tests/            # Framework tests + your tests
    ├── Business/     # Your business logic tests
    └── Integration/  # Your integration tests
```

## 🗂️ Files You Can Safely Remove

**After integration, you can remove these example files:**

```bash
# Remove example files (optional)
rm -rf Models/Examples/
rm -rf Validators/Examples/
rm -rf ViewModels/HomeViewModel.cs
rm -rf Views/HomeView.xaml*
rm -rf Views/ConverterExamples.xaml*
rm -rf Views/ValidationExampleView.xaml*
rm -rf ViewModels/ValidationExampleViewModel.cs
rm -rf Validators/ValidationExampleValidator.cs

# Keep these framework files - they're required
# - All Services/
# - All Interfaces/
# - ViewModelBase, ValidatableViewModelBase, etc.
# - MessageBus, ConfigurationService, etc.
# - All Tests/ (for framework validation)
```

## ⚙️ Configuration Files

**Create `appsettings.json` for your app:**
```json
{
  "ApplicationSettings": {
    "Name": "MyApp",
    "Version": "1.0.0"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    },
    "LogFilePath": "logs/myapp.log",
    "EnableFileLogging": true
  },
  "Features": {
    "EnableDeveloperMode": true,
    "EnableTelemetry": false
  },
  "UI": {
    "Theme": "Dark",
    "FontSize": 12
  }
}
```

## 🧪 Verify Integration

**Run the test suite to ensure everything works:**
```bash
# Run all framework tests
dotnet test

# Expected: 400+ tests should pass
# This validates the framework is properly integrated
```

**Create a simple test for your integration:**
```csharp
// Tests/Integration/BasicIntegrationTest.cs
[Fact]
public void App_Should_Start_Without_Errors()
{
    // This test validates basic DI container setup
    var services = new ServiceCollection();
    
    // Use the same ConfigureServices as your App.xaml.cs
    var app = new App();
    var configMethod = typeof(App).GetMethod("ConfigureServices", 
        BindingFlags.NonPublic | BindingFlags.Instance);
    configMethod?.Invoke(app, new object[] { services });
    
    var serviceProvider = services.BuildServiceProvider();
    
    // Test that core services can be resolved
    var navigationService = serviceProvider.GetService<INavigationService>();
    var dialogService = serviceProvider.GetService<IDialogService>();
    
    Assert.NotNull(navigationService);
    Assert.NotNull(dialogService);
}
```

## 🚨 Common Integration Issues

### Issue: Build Errors After Copying

**Solution:** Ensure all namespaces are updated consistently.

```bash
# Quick fix - find and replace in all files:
# Find: "namespace WPFBase"
# Replace: "namespace MyApp"

# Find: "x:Class="WPFBase."
# Replace: "x:Class="MyApp."
```

### Issue: Services Not Found at Runtime

**Solution:** Check `ConfigureServices` method includes all registrations.

### Issue: Views Not Found

**Solution:** Ensure View/ViewModel naming convention is followed and both are registered.

## 🎯 Success Checklist

- ✅ All files copied to your project
- ✅ Namespaces updated throughout
- ✅ Project file updated with all dependencies
- ✅ Your business services added
- ✅ Your ViewModels created following framework patterns
- ✅ All tests pass
- ✅ Application runs without errors

**You now have a production-ready WPF application with Claude Code-optimized patterns!**