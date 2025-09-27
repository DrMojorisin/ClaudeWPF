# Themes

This folder contains theme definitions and styling resources for the application.

## Purpose
- Define application appearance and styling
- Support multiple themes (Light, Dark, etc.)
- Centralize style definitions for consistency

## Theme Structure
Each theme typically includes:
- Color definitions (brushes)
- Control styles
- Resource dictionaries

## Usage

### Applying Themes
Themes are managed by `IThemeService`:

```csharp
// Switch to dark theme
await _themeService.ApplyThemeAsync("Dark");

// Toggle between light/dark
await _themeService.ToggleThemeAsync();
```

### Theme Files
- `LightTheme.xaml` - Light color scheme
- `DarkTheme.xaml` - Dark color scheme
- `Generic.xaml` - Default styles for custom controls

### Resource Dictionary Structure
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Color Definitions -->
    <SolidColorBrush x:Key="PrimaryBrush" Color="#007ACC" />
    <SolidColorBrush x:Key="BackgroundBrush" Color="#FFFFFF" />

    <!-- Control Styles -->
    <Style TargetType="Button">
        <Setter Property="Background" Value="{StaticResource PrimaryBrush}" />
        <Setter Property="Foreground" Value="White" />
    </Style>

</ResourceDictionary>
```

## Creating New Themes

1. Create new `.xaml` resource dictionary
2. Define color scheme using consistent key names
3. Override control styles as needed
4. Register theme in `ThemeService`

### Color Key Conventions
- `PrimaryBrush` - Main accent color
- `SecondaryBrush` - Secondary accent
- `BackgroundBrush` - Main background
- `SurfaceBrush` - Card/panel backgrounds
- `TextBrush` - Primary text color
- `DisabledBrush` - Disabled element color

## Integration with WPF-UI
This project uses WPF-UI library which provides:
- Modern control styles
- Theme-aware controls
- Fluent design elements

Themes should complement WPF-UI styling rather than override it completely.