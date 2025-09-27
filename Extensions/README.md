# Extensions

This folder contains C# extension methods to add functionality to existing types.

## Purpose
- Extend functionality of built-in types
- Provide reusable utility methods
- Simplify common operations

## Files
- `EnumerableExtensions.cs` - Extensions for IEnumerable<T>
- `ObservableCollectionExtensions.cs` - Extensions for ObservableCollection<T>
- `ServiceCollectionExtensions.cs` - Extensions for dependency injection setup

## Usage Examples

### ObservableCollection Extensions
```csharp
// Add multiple items at once
myCollection.AddRange(newItems);

// Replace all items
myCollection.ReplaceWith(newItems);
```

### Enumerable Extensions
```csharp
// Safe operations
var items = collection.SafeToList(); // Never returns null

// Batch processing
items.Batch(10).ForEach(batch => ProcessBatch(batch));
```

### Service Collection Extensions
```csharp
// In App.xaml.cs
services.AddWPFBaseServices(); // Registers all framework services
```

## Creating New Extensions
```csharp
public static class MyExtensions
{
    public static void DoSomething(this MyType obj)
    {
        // Extension method implementation
    }
}
```