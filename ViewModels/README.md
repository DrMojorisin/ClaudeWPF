# ViewModels

This folder contains ViewModels that implement the MVVM pattern for the application.

## Purpose
- Provide data and commands for Views
- Handle user interaction logic
- Bridge between Models and Views
- Implement INotifyPropertyChanged for data binding

## Architecture
ViewModels use CommunityToolkit.Mvvm for modern, efficient MVVM implementation:

```csharp
public partial class MyViewModel : ViewModelBase
{
    [ObservableProperty]
    private string name = string.Empty;

    [RelayCommand]
    private async Task SaveAsync()
    {
        // Command implementation
    }
}
```

## Base Classes
- `ViewModelBase` - Basic ViewModel with INotifyPropertyChanged
- `ValidatableViewModelBase` - Adds validation support
- `DocumentViewModelBase` - For document-style views with save/load

## Modern Patterns (2024-2025)

### Observable Properties
Use `[ObservableProperty]` instead of manual property implementation:

```csharp
// ✅ Modern way
[ObservableProperty]
private string firstName = string.Empty;

// ❌ Old way
private string _firstName = string.Empty;
public string FirstName
{
    get => _firstName;
    set => SetProperty(ref _firstName, value);
}
```

### Commands
Use `[RelayCommand]` for automatic command generation:

```csharp
[RelayCommand(CanExecute = nameof(CanSave))]
private async Task SaveAsync()
{
    // Implementation
}

private bool CanSave() => !string.IsNullOrEmpty(Name);
```

### Validation
Inherit from `ObservableValidator` for built-in validation:

```csharp
public partial class FormViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [EmailAddress]
    private string email = string.Empty;
}
```

## Dependency Injection
ViewModels receive services through constructor injection:

```csharp
public MyViewModel(IDialogService dialogService, IDataService dataService)
{
    _dialogService = dialogService;
    _dataService = dataService;
}
```

## Organization
Consider organizing by feature or area:
- `/Documents` - Document-based ViewModels
- `/Dialogs` - Dialog ViewModels
- `/Tools` - Tool window ViewModels