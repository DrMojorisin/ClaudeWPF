# Views

This folder contains XAML views and their code-behind files.

## Purpose
- Define the user interface layout and appearance
- Bind to ViewModels for data and commands
- Handle view-specific UI logic

## Architecture
Views follow MVVM pattern with minimal code-behind:

```xml
<UserControl x:Class="WPFBase.Views.MyView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <TextBox Text="{Binding Name, UpdateSourceTrigger=PropertyChanged}" />
        <Button Content="Save" Command="{Binding SaveCommand}" />
    </Grid>
</UserControl>
```

## File Structure
Each view consists of:
- `.xaml` file - UI layout and data binding
- `.xaml.cs` file - Code-behind (minimal, view-specific only)

## Best Practices

### Data Binding
```xml
<!-- ✅ Update on property change -->
<TextBox Text="{Binding Name, UpdateSourceTrigger=PropertyChanged}" />

<!-- ✅ Command binding -->
<Button Command="{Binding SaveCommand}" />

<!-- ✅ Converter usage -->
<TextBox Visibility="{Binding IsVisible, Converter={StaticResource BoolToVis}}" />
```

### Resource Organization
```xml
<UserControl.Resources>
    <Style x:Key="HeaderStyle" TargetType="TextBlock">
        <Setter Property="FontSize" Value="16" />
        <Setter Property="FontWeight" Value="Bold" />
    </Style>
</UserControl.Resources>
```

### Code-Behind Guidelines
Keep code-behind minimal:
- View-specific UI logic only
- No business logic
- No direct data access
- Focus on what ViewModels can't handle

```csharp
public partial class MyView : UserControl
{
    public MyView()
    {
        InitializeComponent();
    }

    // Only UI-specific handlers
    private void OnTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        // Select all text for better UX
        ((TextBox)sender).SelectAll();
    }
}
```

## Organization
Consider organizing by purpose:
- `/Documents` - Main document views
- `/Dialogs` - Dialog windows
- `/Tools` - Tool panels and windows
- `/Controls` - Reusable user controls (move to /Controls folder)