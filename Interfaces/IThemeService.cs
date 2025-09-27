using System.Windows;

namespace WPFBase.Interfaces;

/// <summary>
/// Service for managing application themes
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets the current theme
    /// </summary>
    ApplicationTheme CurrentTheme { get; }
    
    /// <summary>
    /// Gets available themes
    /// </summary>
    IReadOnlyList<ApplicationTheme> AvailableThemes { get; }
    
    /// <summary>
    /// Apply a theme to the application
    /// </summary>
    Task ApplyThemeAsync(ApplicationTheme theme);
    
    /// <summary>
    /// Apply theme by name
    /// </summary>
    Task ApplyThemeAsync(string themeName);
    
    /// <summary>
    /// Toggle between light and dark themes
    /// </summary>
    Task ToggleThemeAsync();
    
    /// <summary>
    /// Load custom theme from file
    /// </summary>
    Task<ApplicationTheme?> LoadCustomThemeAsync(string filePath);
    
    /// <summary>
    /// Save current theme as default
    /// </summary>
    Task SaveThemePreferenceAsync();
    
    /// <summary>
    /// Event raised when theme changes
    /// </summary>
    event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
}

/// <summary>
/// Application theme definition
/// </summary>
public class ApplicationTheme
{
    public string Name { get; set; } = "Default";
    public AppThemeMode Mode { get; set; } = AppThemeMode.Light;
    public string PrimaryColor { get; set; } = "#2196F3";
    public string AccentColor { get; set; } = "#FF4081";
    public string BackgroundColor { get; set; } = "#FFFFFF";
    public string ForegroundColor { get; set; } = "#000000";
    public string BorderColor { get; set; } = "#E0E0E0";
    public string ErrorColor { get; set; } = "#F44336";
    public string WarningColor { get; set; } = "#FF9800";
    public string SuccessColor { get; set; } = "#4CAF50";
    public string InfoColor { get; set; } = "#2196F3";
    public Dictionary<string, string> CustomColors { get; set; } = new();
    public string? ResourceDictionaryPath { get; set; }
}

/// <summary>
/// Theme mode enumeration
/// </summary>
public enum AppThemeMode
{
    Light,
    Dark,
    System,
    Custom
}

/// <summary>
/// Theme changed event arguments
/// </summary>
public class ThemeChangedEventArgs : EventArgs
{
    public ApplicationTheme? OldTheme { get; }
    public ApplicationTheme NewTheme { get; }
    
    public ThemeChangedEventArgs(ApplicationTheme? oldTheme, ApplicationTheme newTheme)
    {
        OldTheme = oldTheme;
        NewTheme = newTheme;
    }
}