# Converters

This folder contains XAML value converters for data binding transformations.

## Purpose
- Convert between different data types in XAML binding
- Transform values for display purposes
- Enable complex binding scenarios

## Files
- `BooleanToVisibilityConverter.cs` - Converts bool to Visibility (Visible/Collapsed)
- `InverseBooleanConverter.cs` - Inverts boolean values
- `NullToVisibilityConverter.cs` - Shows/hides elements based on null values

## Usage in XAML
```xml
<Window.Resources>
    <converters:BooleanToVisibilityConverter x:Key="BoolToVis" />
</Window.Resources>

<TextBox Visibility="{Binding IsVisible, Converter={StaticResource BoolToVis}}" />
```

## Creating New Converters
Implement `IValueConverter` interface:

```csharp
public class MyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Convert logic here
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Convert back logic (optional)
    }
}
```