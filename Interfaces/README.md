# Interfaces

This folder contains service contracts and interface definitions for dependency injection.

## Purpose
- Define contracts for services
- Enable dependency injection and testability
- Separate interface from implementation

## Key Interfaces

### Core Services
- `IDialogService.cs` - Show dialogs, file pickers, message boxes
- `INavigationService.cs` - Navigate between views
- `IMessageBus.cs` - Publish/subscribe messaging
- `IThemeService.cs` - Theme management and switching

### Application Services
- `IConfigurationService.cs` - Application configuration management
- `IUserSettingsService.cs` - User preferences and settings
- `IKeyboardShortcutService.cs` - Keyboard shortcut management

### Advanced Services
- `IDockingService.cs` - Docking window management (Visual Studio style)
- `IPerformanceOptimizationService.cs` - Performance monitoring and optimization

## Architecture Pattern
```
ViewModel → Interface → Implementation
```

## Usage in ViewModels
```csharp
public class MyViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;

    public MyViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    private async Task ShowMessage()
    {
        await _dialogService.ShowInformationAsync("Hello World!");
    }
}
```

## Benefits
- **Testability**: Easy to mock for unit tests
- **Flexibility**: Swap implementations without changing dependents
- **Clean Architecture**: Clear separation of concerns