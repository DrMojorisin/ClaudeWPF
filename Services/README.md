# Services

This folder contains concrete implementations of service interfaces.

## Purpose
- Implement the interfaces defined in /Interfaces folder
- Provide actual business logic and functionality
- Handle external dependencies (file system, network, etc.)

## Key Services

### UI Services
- `DialogService.cs` - Windows dialogs, message boxes, file pickers
- `NavigationService.cs` - View navigation and routing
- `ThemeService.cs` - Theme switching and management

### Application Services
- `ConfigurationService.cs` - Read/write application configuration
- `UserSettingsService.cs` - User preferences persistence
- `LoggingService.cs` - Structured logging with Serilog

### Communication Services
- `MessageBus.cs` - In-app messaging and events
- `KeyboardShortcutService.cs` - Global hotkey management

### Advanced Services
- `DockingService.cs` - AvalonDock window management
- `PerformanceOptimizationService.cs` - Performance monitoring

## Registration
Services are registered in `App.xaml.cs`:

```csharp
private void ConfigureServices(IServiceCollection services)
{
    // Register as singleton (one instance)
    services.AddSingleton<IDialogService, DialogService>();

    // Register as transient (new instance each time)
    services.AddTransient<IDataService, DataService>();
}
```

## Creating New Services
1. Define interface in `/Interfaces`
2. Implement in this folder
3. Register in `App.xaml.cs`
4. Inject into ViewModels as needed

```csharp
public class MyService : IMyService
{
    private readonly ILogger _logger;

    public MyService(ILogger logger)
    {
        _logger = logger;
    }

    public async Task DoSomethingAsync()
    {
        _logger.LogInformation("Doing something...");
        // Implementation
    }
}
```