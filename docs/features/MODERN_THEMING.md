# Modern Theming with .NET 9 FluentTheme

## Overview

WPFBase provides two theme services:

1. **ModernThemeService** - Modern .NET 9 FluentTheme with Windows 11 native styling (RECOMMENDED)
2. **ThemeService** - Legacy resource dictionary-based theming (for compatibility)

This guide focuses on the modern approach using .NET 9's native FluentTheme capabilities.

### Key Advantages of .NET 9 FluentTheme

- **Native Windows 11 Integration** - Automatically matches system theme and accent colors
- **Simplified API** - Uses `Application.ThemeMode` property instead of resource dictionary manipulation
- **Better Performance** - Native theme switching without resource dictionary overhead
- **System Theme Support** - Automatically responds to Windows theme changes
- **Modern Look** - Fluent Design System styling out of the box

### Comparison: ModernThemeService vs ThemeService

| Feature | ModernThemeService (.NET 9) | ThemeService (Legacy) |
|---------|----------------------------|----------------------|
| Theme API | `Application.ThemeMode` | Resource Dictionaries |
| System Integration | Built-in | Manual |
| Performance | Fast (native) | Slower (resource manipulation) |
| Accent Colors | Auto from system | Manual |
| Dark Mode | Native support | Manual implementation |
| Windows 11 Look | Yes | Requires custom XAML |
| Recommended | YES | Only for compatibility |

## FluentTheme Basics

.NET 9 introduced native Fluent Design System support through the `ThemeMode` property.

### ThemeMode Enum

```csharp
public enum ThemeMode
{
    Light,        // Light appearance
    Dark,         // Dark appearance
    System        // Follow OS theme (auto light/dark)
}
```

### Setting Theme in XAML

```xml
<Application x:Class="WPFBase.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             ThemeMode="System">
    <!-- Your app resources -->
</Application>
```

### Setting Theme in Code

```csharp
// Apply light theme
Application.Current.ThemeMode = ThemeMode.Light;

// Apply dark theme
Application.Current.ThemeMode = ThemeMode.Dark;

// Follow system theme preference
Application.Current.ThemeMode = ThemeMode.System;
```

## Quick Start

### 1. Configure DI Container

In `App.xaml.cs`:

```csharp
private void ConfigureServices(IServiceCollection services)
{
    // Register modern theme service (recommended)
    services.AddSingleton<IThemeService, ModernThemeService>();

    // OR register legacy service (only if needed)
    // services.AddSingleton<IThemeService, ThemeService>();

    // ... other services
}
```

### 2. Initialize Theme on Startup

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    // Configure and build service provider
    var serviceCollection = new ServiceCollection();
    ConfigureServices(serviceCollection);
    _serviceProvider = serviceCollection.BuildServiceProvider();

    // Initialize theme service
    var themeService = _serviceProvider.GetRequiredService<IThemeService>();

    // Theme will default to System mode (follows Windows settings)

    // Show main window
    var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
    mainWindow.Show();
}
```

### 3. Apply Theme in ViewModel

```csharp
public class SettingsViewModel : ViewModelBase
{
    private readonly IThemeService _themeService;

    public SettingsViewModel(IThemeService themeService)
    {
        _themeService = themeService;
    }

    public async Task ApplyLightThemeAsync()
    {
        await _themeService.ApplyThemeAsync("Light (Fluent)");
    }

    public async Task ApplyDarkThemeAsync()
    {
        await _themeService.ApplyThemeAsync("Dark (Fluent)");
    }

    public async Task ApplySystemThemeAsync()
    {
        await _themeService.ApplySystemThemeAsync();
    }
}
```

## Theme Switching

### Toggle Light/Dark Mode

```csharp
public class MainViewModel : ViewModelBase
{
    private readonly IThemeService _themeService;

    [RelayCommand]
    private async Task ToggleTheme()
    {
        // Automatically switches between Light and Dark
        await _themeService.ToggleThemeAsync();
    }
}
```

In XAML:

```xml
<Button Content="Toggle Theme"
        Command="{Binding ToggleThemeCommand}"
        ToolTip="Switch between Light and Dark modes" />
```

### Apply Specific Theme

```csharp
// Apply by name
await _themeService.ApplyThemeAsync("Dark (Fluent)");

// Apply by theme object
var darkTheme = _themeService.AvailableThemes
    .First(t => t.Mode == AppThemeMode.Dark);
await _themeService.ApplyThemeAsync(darkTheme);
```

### Available Built-In Themes

ModernThemeService provides four built-in themes:

1. **Light (Fluent)** - Modern light theme with Windows 11 styling
2. **Dark (Fluent)** - Modern dark theme with Windows 11 styling
3. **System (Auto)** - Automatically follows Windows theme preference
4. **Classic WPF** - Traditional Aero2 theme for compatibility

```csharp
public class ThemePickerViewModel : ViewModelBase
{
    private readonly IThemeService _themeService;

    public IReadOnlyList<ApplicationTheme> AvailableThemes =>
        _themeService.AvailableThemes;

    [RelayCommand]
    private async Task SelectTheme(ApplicationTheme theme)
    {
        await _themeService.ApplyThemeAsync(theme);
    }
}
```

## Custom Colors

### ApplicationTheme Color Properties

Each theme includes customizable color palette:

```csharp
public class ApplicationTheme
{
    public string Name { get; set; }                    // "Light (Fluent)", "Dark (Fluent)", etc.
    public AppThemeMode Mode { get; set; }              // Light, Dark, System, Custom
    public string PrimaryColor { get; set; }            // Main brand color
    public string AccentColor { get; set; }             // Highlight color
    public string BackgroundColor { get; set; }         // Window background
    public string ForegroundColor { get; set; }         // Text color
    public string BorderColor { get; set; }             // Control borders
    public string ErrorColor { get; set; }              // Error states
    public string WarningColor { get; set; }            // Warning states
    public string SuccessColor { get; set; }            // Success states
    public string InfoColor { get; set; }               // Informational states
    public Dictionary<string, string> CustomColors { get; set; }
    public string? ResourceDictionaryPath { get; set; } // Path to custom XAML
}
```

### Default Fluent Theme Colors

**Light (Fluent):**
```csharp
PrimaryColor = "#0078D4"      // Windows blue
AccentColor = "#106EBE"       // Darker blue
BackgroundColor = "#FFFFFF"   // White
ForegroundColor = "#000000"   // Black
BorderColor = "#E1E1E1"       // Light gray
```

**Dark (Fluent):**
```csharp
PrimaryColor = "#60CDFF"      // Light blue
AccentColor = "#0078D4"       // Windows blue
BackgroundColor = "#202020"   // Near black
ForegroundColor = "#FFFFFF"   // White
BorderColor = "#404040"       // Dark gray
```

### Using Theme Colors in XAML

FluentTheme automatically provides system color resources:

```xml
<!-- Use system accent color (auto from Windows) -->
<Button Background="{DynamicResource SystemAccentBrush}"
        Foreground="{DynamicResource SystemAccentColorBrush}" />

<!-- Use theme-aware colors -->
<TextBlock Foreground="{DynamicResource SystemControlForegroundBaseHighBrush}" />

<Border BorderBrush="{DynamicResource SystemControlForegroundBaseMediumBrush}" />
```

### Creating Custom Color Schemes

While ModernThemeService uses native FluentTheme, you can still customize colors:

```csharp
public async Task ApplyCustomBrandingAsync()
{
    // Get current Fluent theme
    var currentTheme = _themeService.CurrentTheme;

    // Create custom variant
    var brandedTheme = new ApplicationTheme
    {
        Name = "Branded Light",
        Mode = AppThemeMode.Light,
        ResourceDictionaryPath = currentTheme.ResourceDictionaryPath,
        PrimaryColor = "#0066CC",      // Custom brand color
        AccentColor = "#FF6B00",       // Custom accent
        BackgroundColor = "#FFFFFF",
        ForegroundColor = "#000000",
        BorderColor = "#E1E1E1"
    };

    // Apply custom theme
    await _themeService.ApplyThemeAsync(brandedTheme);
}
```

## System Integration

### Following OS Theme Preference

The System theme mode automatically responds to Windows theme changes:

```csharp
public async Task EnableSystemThemeAsync()
{
    // Application will automatically switch between light/dark
    // when user changes Windows theme
    await _themeService.ApplySystemThemeAsync();
}
```

### Detecting System Theme Changes

.NET 9 automatically handles system theme changes when using `ThemeMode.System`. No additional code needed!

```csharp
// This is all you need for automatic system theme integration
Application.Current.ThemeMode = ThemeMode.System;
```

### Manual System Theme Detection

If you need to know the current system theme:

```csharp
public bool IsSystemInDarkMode()
{
    try
    {
        using var key = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        var value = key?.GetValue("AppsUseLightTheme");
        return value is int i && i == 0; // 0 = dark, 1 = light
    }
    catch
    {
        return false; // Default to light if detection fails
    }
}
```

## Resource Dictionaries

### Built-In Fluent Resource Path

ModernThemeService uses the .NET 9 Fluent theme:

```csharp
ResourceDictionaryPath = "/PresentationFramework.Fluent;component/Themes/Fluent.xaml"
```

This provides modern Windows 11 styling automatically.

### Loading Custom Resource Dictionaries

For additional custom styling on top of FluentTheme:

```csharp
public async Task LoadCustomStylesAsync()
{
    var customTheme = new ApplicationTheme
    {
        Name = "Custom Fluent",
        Mode = AppThemeMode.Light,
        // Start with Fluent base
        ResourceDictionaryPath = "/PresentationFramework.Fluent;component/Themes/Fluent.xaml",
        PrimaryColor = "#FF5722",
        AccentColor = "#FF6B00"
    };

    // Apply base theme
    await _themeService.ApplyThemeAsync(customTheme);

    // Add custom resource dictionary on top
    var customDict = new ResourceDictionary
    {
        Source = new Uri("pack://application:,,,/Themes/CustomStyles.xaml")
    };
    Application.Current.Resources.MergedDictionaries.Add(customDict);
}
```

### Custom Theme File Format (JSON)

While ModernThemeService focuses on native themes, you can still load custom definitions:

```json
{
  "Name": "Custom Corporate Theme",
  "Mode": "Light",
  "PrimaryColor": "#1565C0",
  "AccentColor": "#0091EA",
  "BackgroundColor": "#FAFAFA",
  "ForegroundColor": "#263238",
  "BorderColor": "#B0BEC5",
  "ErrorColor": "#D32F2F",
  "WarningColor": "#F57C00",
  "SuccessColor": "#388E3C",
  "InfoColor": "#0288D1",
  "ResourceDictionaryPath": "/PresentationFramework.Fluent;component/Themes/Fluent.xaml",
  "CustomColors": {
    "HeaderBackground": "#1976D2",
    "HeaderForeground": "#FFFFFF",
    "ToolbarBackground": "#E3F2FD"
  }
}
```

Load custom theme:

```csharp
var theme = await _themeService.LoadCustomThemeAsync("CustomTheme.json");
if (theme != null)
{
    await _themeService.ApplyThemeAsync(theme);
}
```

**Note:** `LoadCustomThemeAsync` is not yet implemented in ModernThemeService (throws `NotImplementedException`). This is a future enhancement.

## Theme Changed Events

### Subscribing to Theme Changes

```csharp
public class MyViewModel : ViewModelBase
{
    private readonly IThemeService _themeService;

    public MyViewModel(IThemeService themeService)
    {
        _themeService = themeService;

        // Subscribe to theme changes
        _themeService.ThemeChanged += OnThemeChanged;
    }

    private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        var oldTheme = e.OldTheme?.Name ?? "None";
        var newTheme = e.NewTheme.Name;

        Logger.Info($"Theme changed: {oldTheme} -> {newTheme}");

        // Update UI based on new theme
        UpdateThemeDependentUI(e.NewTheme);
    }

    private void UpdateThemeDependentUI(ApplicationTheme theme)
    {
        // Adjust UI elements based on theme
        if (theme.Mode == AppThemeMode.Dark)
        {
            // Dark mode specific adjustments
        }
        else
        {
            // Light mode specific adjustments
        }
    }
}
```

### Theme Change Event Args

```csharp
public class ThemeChangedEventArgs : EventArgs
{
    public ApplicationTheme? OldTheme { get; }  // null if no previous theme
    public ApplicationTheme NewTheme { get; }    // Newly applied theme

    public ThemeChangedEventArgs(ApplicationTheme? oldTheme, ApplicationTheme newTheme)
    {
        OldTheme = oldTheme;
        NewTheme = newTheme;
    }
}
```

## MessageBus Integration

**Note:** ModernThemeService does NOT currently publish to MessageBus. Only the legacy ThemeService supports this. This may be added in future updates.

If you need MessageBus integration, use ThemeService:

```csharp
// In ThemeService (legacy only)
public class ThemeChangedMessage : MessageBase
{
    public ApplicationTheme? OldTheme { get; set; }
    public ApplicationTheme? NewTheme { get; set; }
}

// Subscribe in ViewModel
_messageBus.Subscribe<ThemeChangedMessage>(OnThemeChangedMessage);

private void OnThemeChangedMessage(ThemeChangedMessage message)
{
    // Handle theme change via message bus
}
```

## Persistence

### Saving Theme Preference

```csharp
public class SettingsViewModel : ViewModelBase
{
    private readonly IThemeService _themeService;
    private readonly IUserSettingsService _userSettings;

    [RelayCommand]
    private async Task SaveCurrentTheme()
    {
        // Save theme preference
        await _themeService.SaveThemePreferenceAsync();

        // ModernThemeService just logs the action
        // For actual persistence, integrate with UserSettingsService:
        _userSettings.Set("Theme.PreferredMode",
            _themeService.CurrentTheme.Name);
        await _userSettings.SaveAsync();
    }
}
```

### Loading Saved Theme on Startup

```csharp
// In App.xaml.cs
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    // Build DI container
    ConfigureServices(_serviceCollection);
    _serviceProvider = _serviceCollection.BuildServiceProvider();

    // Load saved theme preference
    var themeService = _serviceProvider.GetRequiredService<IThemeService>();
    var userSettings = _serviceProvider.GetRequiredService<IUserSettingsService>();

    var savedTheme = userSettings.Get<string>("Theme.PreferredMode");
    if (!string.IsNullOrEmpty(savedTheme))
    {
        await themeService.ApplyThemeAsync(savedTheme);
    }

    // Show window
    var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
    mainWindow.Show();
}
```

### Integration with UserSettingsService

Full persistence example:

```csharp
public class ThemeManager
{
    private readonly IThemeService _themeService;
    private readonly IUserSettingsService _userSettings;
    private const string THEME_PREFERENCE_KEY = "UI.Theme";

    public ThemeManager(IThemeService themeService, IUserSettingsService userSettings)
    {
        _themeService = themeService;
        _userSettings = userSettings;

        // Load saved theme
        _ = LoadSavedThemeAsync();

        // Save theme when it changes
        _themeService.ThemeChanged += async (s, e) => await SaveThemeAsync(e.NewTheme);
    }

    private async Task LoadSavedThemeAsync()
    {
        var savedThemeName = _userSettings.Get<string>(THEME_PREFERENCE_KEY);
        if (!string.IsNullOrEmpty(savedThemeName))
        {
            try
            {
                await _themeService.ApplyThemeAsync(savedThemeName);
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to load saved theme: {ex.Message}");
                // Fall back to default System theme
                await _themeService.ApplySystemThemeAsync();
            }
        }
    }

    private async Task SaveThemeAsync(ApplicationTheme theme)
    {
        _userSettings.Set(THEME_PREFERENCE_KEY, theme.Name);
        await _userSettings.SaveAsync();
    }
}
```

## Migration Guide

### Moving from ThemeService to ModernThemeService

#### 1. Update Service Registration

**Before (Legacy):**
```csharp
services.AddSingleton<IThemeService, ThemeService>();
```

**After (Modern):**
```csharp
services.AddSingleton<IThemeService, ModernThemeService>();
```

#### 2. Update Theme Names

**Legacy Theme Names:**
- `"Light"` -> `"Light (Fluent)"`
- `"Dark"` -> `"Dark (Fluent)"`
- `"Blue"` -> Not available (use Light with custom colors)

**Migration Code:**
```csharp
public string MigrateThemeName(string legacyName)
{
    return legacyName switch
    {
        "Light" => "Light (Fluent)",
        "Dark" => "Dark (Fluent)",
        "Blue" => "Light (Fluent)", // Closest match
        _ => "System (Auto)" // Default to system
    };
}
```

#### 3. Remove MessageBus Theme Subscriptions

ModernThemeService doesn't publish to MessageBus (by design for simplicity).

**Before:**
```csharp
_messageBus.Subscribe<ThemeChangedMessage>(OnThemeChanged);
```

**After:**
```csharp
_themeService.ThemeChanged += OnThemeChanged;
```

#### 4. Update Custom Color Handling

Legacy ThemeService manually updated resource dictionaries. ModernThemeService uses native FluentTheme.

**Before:**
```csharp
Application.Current.Resources["PrimaryColor"] = newColor;
Application.Current.Resources["PrimaryColorBrush"] = new SolidColorBrush(newColor);
```

**After:**
```csharp
// Colors are handled automatically by FluentTheme
// Just apply theme with desired colors
var theme = new ApplicationTheme
{
    PrimaryColor = "#0078D4",
    Mode = AppThemeMode.Light,
    ResourceDictionaryPath = "/PresentationFramework.Fluent;component/Themes/Fluent.xaml"
};
await _themeService.ApplyThemeAsync(theme);
```

#### 5. System Theme Integration

**Before (Manual):**
```csharp
// Had to manually detect and apply system theme
var isDark = DetectSystemDarkMode();
await _themeService.ApplyThemeAsync(isDark ? "Dark" : "Light");

// Had to subscribe to system theme changes
SystemEvents.UserPreferenceChanged += OnSystemThemeChanged;
```

**After (Automatic):**
```csharp
// Just use System mode - automatic detection and updates
await _themeService.ApplySystemThemeAsync();
// That's it! No event subscriptions needed.
```

## Claude Code Examples

### Example 1: Add Theme Toggle to MainWindow

**Prompt:**
```
Add a theme toggle button to MainWindow in the top-right corner.
Use ModernThemeService to toggle between light and dark modes.
Add a moon/sun icon that changes based on current theme.
```

### Example 2: Create Theme Settings Page

**Prompt:**
```
Create a SettingsView with theme selection. Show all available themes
with preview colors. Use ModernThemeService and bind to ToggleThemeCommand.
Include save preference button that persists to UserSettingsService.
```

### Example 3: Custom Branded Theme

**Prompt:**
```
Create a custom branded theme in ModernThemeService with colors:
- Primary: #1976D2 (corporate blue)
- Accent: #FFC107 (gold)
- Background: #FAFAFA (off-white)
Use FluentTheme as base. Apply on startup if user is in "Corporate" mode.
```

### Example 4: Theme-Aware Custom Control

**Prompt:**
```
Create a custom UserControl that changes appearance based on current theme.
Use ModernThemeService.ThemeChanged event to update. Show different
icon and colors for light/dark/system modes. Use SystemAccentBrush.
```

### Example 5: High Contrast Support

**Prompt:**
```
Add high contrast theme support to ModernThemeService. Detect Windows
high contrast mode and apply appropriate theme. Use SystemParameters
to detect high contrast and adjust colors automatically.
```

### Example 6: Theme Transition Animation

**Prompt:**
```
Add smooth fade transition when switching themes in ModernThemeService.
Animate Application.Opacity from 1 to 0.8 and back when theme changes.
Duration 200ms. Apply to all windows.
```

## Best Practices

### 1. Prefer System Theme Mode

```csharp
// GOOD: Respects user's OS preference
await _themeService.ApplySystemThemeAsync();

// AVOID: Forcing a specific theme
await _themeService.ApplyThemeAsync("Light (Fluent)");
```

### 2. Use DynamicResource for Theme Colors

```xml
<!-- GOOD: Automatically updates on theme change -->
<Button Background="{DynamicResource SystemAccentBrush}" />

<!-- AVOID: Static color won't change with theme -->
<Button Background="#0078D4" />
```

### 3. Test in Both Light and Dark Modes

Always test your UI in both themes:

```csharp
[TestMethod]
public async Task TestUIInBothThemes()
{
    // Test light mode
    await _themeService.ApplyThemeAsync("Light (Fluent)");
    VerifyUIContrast();

    // Test dark mode
    await _themeService.ApplyThemeAsync("Dark (Fluent)");
    VerifyUIContrast();
}
```

### 4. Handle Theme Changes Gracefully

```csharp
// Unsubscribe from events to prevent memory leaks
public void Dispose()
{
    _themeService.ThemeChanged -= OnThemeChanged;
}
```

### 5. Use Fluent Design System Resources

Take advantage of built-in Fluent resources:

```xml
<!-- System-aware foreground color -->
<TextBlock Foreground="{DynamicResource SystemControlForegroundBaseHighBrush}" />

<!-- System accent color -->
<Border Background="{DynamicResource SystemControlHighlightAccentBrush}" />

<!-- Theme-aware disabled state -->
<Button IsEnabled="False"
        Foreground="{DynamicResource SystemControlDisabledBaseMediumLowBrush}" />
```

### 6. Minimize Custom Resource Dictionaries

FluentTheme provides comprehensive styling. Only add custom resources when necessary:

```csharp
// GOOD: Use built-in Fluent theme
ResourceDictionaryPath = "/PresentationFramework.Fluent;component/Themes/Fluent.xaml"

// AVOID: Replacing entire theme with custom dictionary
ResourceDictionaryPath = "pack://application:,,,/Themes/CompletelyCustomTheme.xaml"
```

### 7. Save User Preference

Always persist theme choice:

```csharp
// After theme change
await _themeService.SaveThemePreferenceAsync();

// And integrate with settings
_userSettings.Set("Theme.Mode", _themeService.CurrentTheme.Name);
await _userSettings.SaveAsync();
```

### 8. Use Extension Methods

ModernThemeService provides helpful extensions:

```csharp
// Check if using Fluent theme
if (_themeService.CurrentTheme.IsFluentTheme())
{
    // Apply Fluent-specific optimizations
}

// Get theme-aware accent brush
var accentBrush = _themeService.CurrentTheme.GetAccentBrush();
```

## Troubleshooting

### Theme Not Applying

**Problem:** Theme doesn't change when calling `ApplyThemeAsync`.

**Solution:**
```csharp
// Check theme service is registered
var themeService = _serviceProvider.GetService<IThemeService>();
if (themeService == null)
{
    throw new InvalidOperationException("IThemeService not registered in DI container");
}

// Check theme exists
var theme = themeService.AvailableThemes
    .FirstOrDefault(t => t.Name == "Light (Fluent)");
if (theme == null)
{
    throw new ArgumentException("Theme not found in available themes");
}

// Apply with error handling
try
{
    await themeService.ApplyThemeAsync(theme);
}
catch (Exception ex)
{
    Logger.Error($"Failed to apply theme: {ex.Message}");
}
```

### System Theme Not Following OS

**Problem:** System theme mode not responding to Windows theme changes.

**Solution:**
```csharp
// Ensure using System mode
Application.Current.ThemeMode = ThemeMode.System;

// Verify .NET 9 runtime
var version = Environment.Version;
if (version.Major < 9)
{
    throw new NotSupportedException(
        $"System theme requires .NET 9+. Current: {version}");
}
```

### Colors Not Updating

**Problem:** Custom colors not appearing after theme change.

**Solution:**
```xml
<!-- Use DynamicResource, not StaticResource -->
<Button Background="{DynamicResource SystemAccentBrush}" />

<!-- Avoid hardcoded colors -->
<!-- BAD: <Button Background="#0078D4" /> -->
```

### Resource Conflicts

**Problem:** Custom resource dictionary conflicts with FluentTheme.

**Solution:**
```csharp
// Order matters: Load Fluent first, custom overrides after
var fluent = new ResourceDictionary
{
    Source = new Uri("/PresentationFramework.Fluent;component/Themes/Fluent.xaml",
        UriKind.Relative)
};
var custom = new ResourceDictionary
{
    Source = new Uri("pack://application:,,,/Themes/CustomOverrides.xaml")
};

Application.Current.Resources.MergedDictionaries.Clear();
Application.Current.Resources.MergedDictionaries.Add(fluent);
Application.Current.Resources.MergedDictionaries.Add(custom); // Loads last = higher priority
```

### Theme Flicker on Startup

**Problem:** Brief flash of wrong theme when app starts.

**Solution:**
```csharp
// Apply theme BEFORE showing window
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    ConfigureServices(_serviceCollection);
    _serviceProvider = _serviceCollection.BuildServiceProvider();

    // Apply saved theme
    var themeService = _serviceProvider.GetRequiredService<IThemeService>();
    var savedTheme = LoadSavedThemePreference();
    await themeService.ApplyThemeAsync(savedTheme);

    // NOW show window
    var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
    mainWindow.Show();
}
```

### Dark Mode Text Invisible

**Problem:** Text disappears in dark mode.

**Solution:**
```xml
<!-- Don't hardcode white text -->
<!-- BAD: <TextBlock Foreground="White" /> -->

<!-- Use theme-aware brushes -->
<TextBlock Foreground="{DynamicResource SystemControlForegroundBaseHighBrush}" />
```

### Accent Color Not Applying

**Problem:** System accent color not showing in app.

**Solution:**
```csharp
// Fluent theme handles accent automatically
// Just ensure using Fluent theme
var fluentTheme = _themeService.AvailableThemes
    .First(t => t.Name.Contains("Fluent"));
await _themeService.ApplyThemeAsync(fluentTheme);

// Access system accent in XAML
<Button Background="{DynamicResource SystemAccentBrush}"
        Foreground="{DynamicResource SystemAccentColorBrush}" />
```

### Theme Events Not Firing

**Problem:** ThemeChanged event not raised.

**Solution:**
```csharp
// Ensure subscribed before applying theme
_themeService.ThemeChanged += OnThemeChanged;
await _themeService.ApplyThemeAsync("Dark (Fluent)");

// Check for event subscription leaks
private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
{
    Debug.WriteLine($"Theme changed to: {e.NewTheme.Name}");
}

// Unsubscribe to prevent memory leaks
public void Dispose()
{
    _themeService.ThemeChanged -= OnThemeChanged;
}
```

## Additional Resources

### Related Documentation
- `/docs/features/DEPENDENCY_INJECTION.md` - Service registration patterns
- `/docs/features/USER_SETTINGS.md` - Persisting theme preferences
- `/docs/features/MESSAGE_BUS.md` - Event-driven architecture (legacy ThemeService)

### Microsoft Documentation
- [.NET 9 Fluent Theme](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/fundamentals/styles-templates-themes)
- [WPF Styling and Theming](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/styling-and-templating)
- [Windows 11 Fluent Design](https://www.microsoft.com/design/fluent/)

### Source Code
- `C:\DEVELOPMENT\Projects\WPFBase\Services\ModernThemeService.cs` - Modern implementation
- `C:\DEVELOPMENT\Projects\WPFBase\Services\ThemeService.cs` - Legacy implementation
- `C:\DEVELOPMENT\Projects\WPFBase\Interfaces\IThemeService.cs` - Service contract
- `C:\DEVELOPMENT\Projects\WPFBase\App.xaml.cs` - Service registration and initialization

---

**Last Updated:** 2025-09-29
**Status:** Production Ready
**.NET Version:** 9.0+
**Tested With:** ModernThemeService in MainWindow