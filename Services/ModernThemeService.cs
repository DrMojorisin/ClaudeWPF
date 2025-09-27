using System.Windows;
using WPFBase.Interfaces;
using Microsoft.Extensions.Logging;

namespace WPFBase.Services;

/// <summary>
/// Modern theme service leveraging .NET 9 Fluent theming capabilities
/// Provides Windows 11 native theming with system integration
/// </summary>
public class ModernThemeService : IThemeService
{
    private readonly ILoggingService _logger;
    private ApplicationTheme _currentTheme;
    private readonly List<ApplicationTheme> _availableThemes;

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    public ModernThemeService(ILoggingService logger)
    {
        _logger = logger;
        _availableThemes = InitializeNativeThemes();
        _currentTheme = GetDefaultTheme();
    }

    public ApplicationTheme CurrentTheme => _currentTheme;
    public IReadOnlyList<ApplicationTheme> AvailableThemes => _availableThemes.AsReadOnly();

    /// <summary>
    /// Apply .NET 9 native Fluent theme using ThemeMode property
    /// </summary>
    public async Task ApplyThemeAsync(string themeName)
    {
        _logger.LogInformation("Applying .NET 9 Fluent theme: {ThemeName}", themeName);

        var theme = _availableThemes.FirstOrDefault(t => t.Name == themeName);
        if (theme == null)
        {
            _logger.LogWarning("Theme not found: {ThemeName}", themeName);
            return;
        }

        try
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Use .NET 9 ThemeMode property for native Fluent theming
                    Application.Current.ThemeMode = GetNativeThemeMode(theme);

                    // Apply accent color if supported
                    ApplyAccentColor(theme);

                    _currentTheme = theme;
                });
            });

            _logger.LogInformation("Successfully applied theme: {ThemeName}", themeName);
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(_currentTheme));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply theme: {ThemeName}", themeName);
            throw;
        }
    }

    /// <summary>
    /// Toggle between light and dark modes using native .NET 9 theming
    /// </summary>
    public async Task ToggleThemeAsync()
    {
        var targetTheme = _currentTheme.Mode switch
        {
            AppThemeMode.Light => _availableThemes.First(t => t.Mode == AppThemeMode.Dark),
            AppThemeMode.Dark => _availableThemes.First(t => t.Mode == AppThemeMode.Light),
            AppThemeMode.System => _availableThemes.First(t => t.Mode == AppThemeMode.Light),
            _ => _availableThemes.First(t => t.Mode == AppThemeMode.Light)
        };

        await ApplyThemeAsync(targetTheme.Name);
    }

    /// <summary>
    /// Apply system theme that responds to Windows 11 theme changes
    /// </summary>
    public async Task ApplySystemThemeAsync()
    {
        _logger.LogInformation("Applying system theme integration");

        try
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // Use .NET 9 System theme mode for automatic light/dark switching
                Application.Current.ThemeMode = ThemeMode.System;

                var systemTheme = _availableThemes.First(t => t.Mode == AppThemeMode.System);
                _currentTheme = systemTheme;
            });

            _logger.LogInformation("Successfully applied system theme");
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(_currentTheme));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply system theme");
            throw;
        }
    }

    public Task<ApplicationTheme> LoadCustomThemeAsync(string filePath)
    {
        // For custom themes, we still use resource dictionaries
        // but integrate with the Fluent base theme
        throw new NotImplementedException("Custom theme loading with Fluent integration - to be implemented");
    }

    /// <summary>
    /// Convert our theme modes to .NET 9 native ThemeMode
    /// </summary>
    private static ThemeMode GetNativeThemeMode(ApplicationTheme theme)
    {
        return theme.Mode switch
        {
            AppThemeMode.Light => ThemeMode.Light,
            AppThemeMode.Dark => ThemeMode.Dark,
            AppThemeMode.System => ThemeMode.System,
            _ => ThemeMode.Light
        };
    }

    /// <summary>
    /// Apply system accent color for Windows 11 integration
    /// </summary>
    private void ApplyAccentColor(ApplicationTheme theme)
    {
        try
        {
            // .NET 9 provides automatic accent color support
            // The Fluent theme will automatically pick up system accent colors
            var accentColorResource = Application.Current.Resources["SystemAccentColor"];
            if (accentColorResource != null)
            {
                _logger.LogDebug("System accent color applied automatically by Fluent theme");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not apply accent color");
        }
    }

    /// <summary>
    /// Initialize themes using .NET 9 Fluent capabilities
    /// </summary>
    private List<ApplicationTheme> InitializeNativeThemes()
    {
        return new List<ApplicationTheme>
        {
            new()
            {
                Name = "Fluent Light",
                Mode = AppThemeMode.Light,
                DisplayName = "Light (Fluent)",
                Description = ".NET 9 Fluent light theme with Windows 11 styling",
                ResourceDictionaries = new[] { "/PresentationFramework.Fluent;component/Themes/Fluent.xaml" }
            },
            new()
            {
                Name = "Fluent Dark",
                Mode = AppThemeMode.Dark,
                DisplayName = "Dark (Fluent)",
                Description = ".NET 9 Fluent dark theme with Windows 11 styling",
                ResourceDictionaries = new[] { "/PresentationFramework.Fluent;component/Themes/Fluent.xaml" }
            },
            new()
            {
                Name = "System",
                Mode = AppThemeMode.System,
                DisplayName = "System (Auto)",
                Description = "Automatically follows Windows 11 system theme",
                ResourceDictionaries = new[] { "/PresentationFramework.Fluent;component/Themes/Fluent.xaml" }
            },
            new()
            {
                Name = "Classic",
                Mode = AppThemeMode.Light,
                DisplayName = "Classic WPF",
                Description = "Traditional WPF Aero2 theme",
                ResourceDictionaries = Array.Empty<string>() // Uses default Aero2
            }
        };
    }

    private ApplicationTheme GetDefaultTheme()
    {
        // Default to system theme for best Windows 11 integration
        return _availableThemes.First(t => t.Mode == AppThemeMode.System);
    }
}

/// <summary>
/// Extension methods for enhanced theme functionality
/// </summary>
public static class ModernThemeExtensions
{
    /// <summary>
    /// Check if the current theme is using Fluent design
    /// </summary>
    public static bool IsFluentTheme(this ApplicationTheme theme)
    {
        return theme.Name.Contains("Fluent", StringComparison.OrdinalIgnoreCase) ||
               theme.Mode == AppThemeMode.System;
    }

    /// <summary>
    /// Get theme-appropriate accent brush
    /// </summary>
    public static object GetAccentBrush(this ApplicationTheme theme)
    {
        return Application.Current.TryFindResource("SystemAccentBrush") ??
               Application.Current.TryFindResource("AccentColorBrush") ??
               System.Windows.Media.Brushes.Blue;
    }
}