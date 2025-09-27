# Models

This folder contains data models, DTOs, and plain objects used throughout the application.

## Purpose
- Define data structures
- Transfer objects between layers
- Configuration and settings objects

## Files
- `AppSettings.cs` - Application configuration settings
- `UserPreferences.cs` - User-specific preferences
- `WindowStateInfo.cs` - Window position and state information

## Types of Models

### Configuration Models
Objects that hold application settings and configuration:
```csharp
public class AppSettings
{
    public string Theme { get; set; }
    public int FontSize { get; set; }
    public bool EnableLogging { get; set; }
}
```

### Data Transfer Objects (DTOs)
Objects for transferring data between services:
```csharp
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

### View Models vs Models
- **Models**: Plain data objects, no UI logic
- **ViewModels**: Inherit from ViewModelBase, contain UI logic and commands

## Best Practices
- Keep models simple and focused
- Use validation attributes when appropriate
- Avoid business logic in models
- Consider immutability for configuration objects