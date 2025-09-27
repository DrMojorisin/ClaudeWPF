# Commands

This folder contains custom command implementations for the WPF application.

## Purpose
- Custom ICommand implementations
- Reusable command patterns
- Command base classes for common scenarios

## Files Expected
- `AsyncRelayCommand.cs` - Command with async support
- `DelegateCommand.cs` - Basic parameterized command
- Other custom command implementations

## Usage
Commands are used in ViewModels to handle user interactions like button clicks, menu selections, etc.

```csharp
// Example usage in ViewModel
public ICommand SaveCommand { get; }

public MyViewModel()
{
    SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
}
```