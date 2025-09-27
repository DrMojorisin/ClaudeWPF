using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using WPFBase.Interfaces;

namespace WPFBase.Services;

/// <summary>
/// Implementation of theme service for managing application themes
/// </summary>
public class ThemeService : IThemeService
{
    private readonly IConfigurationService _configurationService;
    private readonly IMessageBus _messageBus;
    private readonly LoggingService _loggingService;
    private ApplicationTheme _currentTheme;
    private readonly List<ApplicationTheme> _availableThemes;
    private readonly string _customThemesPath;

    public ThemeService(
        IConfigurationService configurationService,
        IMessageBus messageBus,
        LoggingService loggingService)
    {
        _configurationService = configurationService;
        _messageBus = messageBus;
        _loggingService = loggingService;
        
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _customThemesPath = Path.Combine(appDataPath, "WPFBase", "Themes");
        Directory.CreateDirectory(_customThemesPath);
        
        _availableThemes = InitializeDefaultThemes();
        _currentTheme = _availableThemes[0];
        
        LoadSavedThemePreference();
    }

    public ApplicationTheme CurrentTheme => _currentTheme;

    public IReadOnlyList<ApplicationTheme> AvailableThemes => _availableThemes.AsReadOnly();

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    public async Task ApplyThemeAsync(ApplicationTheme theme)
    {
        if (theme == null)
            throw new ArgumentNullException(nameof(theme));

        var oldTheme = _currentTheme;
        _currentTheme = theme;

        try
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var app = Application.Current;
                
                // Clear existing theme resources
                var themeDictionaries = app.Resources.MergedDictionaries
                    .Where(d => d.Source?.ToString().Contains("Themes") == true)
                    .ToList();
                
                foreach (var dict in themeDictionaries)
                {
                    app.Resources.MergedDictionaries.Remove(dict);
                }
                
                // Apply new theme colors
                UpdateResourceColor(app.Resources, "PrimaryColor", theme.PrimaryColor);
                UpdateResourceColor(app.Resources, "AccentColor", theme.AccentColor);
                UpdateResourceColor(app.Resources, "BackgroundColor", theme.BackgroundColor);
                UpdateResourceColor(app.Resources, "ForegroundColor", theme.ForegroundColor);
                UpdateResourceColor(app.Resources, "BorderColor", theme.BorderColor);
                UpdateResourceColor(app.Resources, "ErrorColor", theme.ErrorColor);
                UpdateResourceColor(app.Resources, "WarningColor", theme.WarningColor);
                UpdateResourceColor(app.Resources, "SuccessColor", theme.SuccessColor);
                UpdateResourceColor(app.Resources, "InfoColor", theme.InfoColor);
                
                // Apply custom colors
                foreach (var (key, value) in theme.CustomColors)
                {
                    UpdateResourceColor(app.Resources, key, value);
                }
                
                // Load custom resource dictionary if specified
                if (!string.IsNullOrEmpty(theme.ResourceDictionaryPath))
                {
                    try
                    {
                        var resourceDict = new ResourceDictionary
                        {
                            Source = new Uri(theme.ResourceDictionaryPath, UriKind.RelativeOrAbsolute)
                        };
                        app.Resources.MergedDictionaries.Add(resourceDict);
                    }
                    catch (Exception ex)
                    {
                        _loggingService.LogError(ex, "Failed to load theme resource dictionary: {Path}", 
                            theme.ResourceDictionaryPath);
                    }
                }
                
                // Update system colors for dark/light mode
                UpdateSystemColors(theme.Mode);
            });

            _loggingService.LogInformation("Theme changed from {OldTheme} to {NewTheme}", 
                oldTheme?.Name, theme.Name);
            
            // Raise event
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(oldTheme, theme));
            
            // Publish message
            _messageBus.Publish(new ThemeChangedMessage 
            { 
                OldTheme = oldTheme, 
                NewTheme = theme 
            });
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to apply theme: {ThemeName}", theme.Name);
            _currentTheme = oldTheme;
            throw;
        }
    }

    public async Task ApplyThemeAsync(string themeName)
    {
        var theme = _availableThemes.FirstOrDefault(t => 
            t.Name.Equals(themeName, StringComparison.OrdinalIgnoreCase));
        
        if (theme == null)
        {
            throw new ArgumentException($"Theme '{themeName}' not found", nameof(themeName));
        }
        
        await ApplyThemeAsync(theme);
    }

    public async Task ToggleThemeAsync()
    {
        var newTheme = _currentTheme.Mode == AppThemeMode.Light
            ? _availableThemes.FirstOrDefault(t => t.Mode == AppThemeMode.Dark)
            : _availableThemes.FirstOrDefault(t => t.Mode == AppThemeMode.Light);
        
        if (newTheme != null)
        {
            await ApplyThemeAsync(newTheme);
        }
    }

    public async Task<ApplicationTheme?> LoadCustomThemeAsync(string filePath)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var theme = JsonSerializer.Deserialize<ApplicationTheme>(json);
            
            if (theme != null)
            {
                theme.Mode = AppThemeMode.Custom;
                _availableThemes.Add(theme);
                _loggingService.LogInformation("Loaded custom theme: {ThemeName}", theme.Name);
            }
            
            return theme;
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to load custom theme from: {FilePath}", filePath);
            return null;
        }
    }

    public async Task SaveThemePreferenceAsync()
    {
        _configurationService.SetValue("ApplicationSettings:Theme", _currentTheme.Name);
        await _configurationService.SaveAsync();
    }

    private void LoadSavedThemePreference()
    {
        var savedThemeName = _configurationService.GetValue<string>("ApplicationSettings:Theme");
        if (!string.IsNullOrEmpty(savedThemeName))
        {
            var theme = _availableThemes.FirstOrDefault(t => 
                t.Name.Equals(savedThemeName, StringComparison.OrdinalIgnoreCase));
            
            if (theme != null)
            {
                _ = ApplyThemeAsync(theme);
            }
        }
    }

    private List<ApplicationTheme> InitializeDefaultThemes()
    {
        return new List<ApplicationTheme>
        {
            new ApplicationTheme
            {
                Name = "Light",
                Mode = AppThemeMode.Light,
                PrimaryColor = "#2196F3",
                AccentColor = "#FF4081",
                BackgroundColor = "#FFFFFF",
                ForegroundColor = "#212121",
                BorderColor = "#E0E0E0",
                ErrorColor = "#F44336",
                WarningColor = "#FF9800",
                SuccessColor = "#4CAF50",
                InfoColor = "#2196F3"
            },
            new ApplicationTheme
            {
                Name = "Dark",
                Mode = AppThemeMode.Dark,
                PrimaryColor = "#1976D2",
                AccentColor = "#FF4081",
                BackgroundColor = "#121212",
                ForegroundColor = "#E0E0E0",
                BorderColor = "#373737",
                ErrorColor = "#CF6679",
                WarningColor = "#FFB74D",
                SuccessColor = "#81C784",
                InfoColor = "#64B5F6"
            },
            new ApplicationTheme
            {
                Name = "Blue",
                Mode = AppThemeMode.Light,
                PrimaryColor = "#1565C0",
                AccentColor = "#0091EA",
                BackgroundColor = "#FAFAFA",
                ForegroundColor = "#263238",
                BorderColor = "#B0BEC5",
                ErrorColor = "#D32F2F",
                WarningColor = "#F57C00",
                SuccessColor = "#388E3C",
                InfoColor = "#0288D1"
            }
        };
    }

    private void UpdateResourceColor(ResourceDictionary resources, string key, string colorHex)
    {
        try
        {
            var color = (Color)ColorConverter.ConvertFromString(colorHex);
            resources[key] = color;
            resources[$"{key}Brush"] = new SolidColorBrush(color);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to update resource color: {Key} = {Color}", 
                key, colorHex);
        }
    }

    private void UpdateSystemColors(Interfaces.AppThemeMode mode)
    {
        var resources = Application.Current.Resources;
        
        if (mode == Interfaces.AppThemeMode.Dark)
        {
            resources[SystemColors.WindowColorKey] = Color.FromRgb(32, 32, 32);
            resources[SystemColors.WindowTextColorKey] = Colors.White;
            resources[SystemColors.ControlColorKey] = Color.FromRgb(45, 45, 45);
            resources[SystemColors.ControlTextColorKey] = Colors.White;
            resources[SystemColors.GrayTextColorKey] = Color.FromRgb(155, 155, 155);
            resources[SystemColors.HighlightColorKey] = Color.FromRgb(0, 120, 212);
            resources[SystemColors.HighlightTextColorKey] = Colors.White;
        }
        else
        {
            resources[SystemColors.WindowColorKey] = Colors.White;
            resources[SystemColors.WindowTextColorKey] = Colors.Black;
            resources[SystemColors.ControlColorKey] = Color.FromRgb(240, 240, 240);
            resources[SystemColors.ControlTextColorKey] = Colors.Black;
            resources[SystemColors.GrayTextColorKey] = Color.FromRgb(109, 109, 109);
            resources[SystemColors.HighlightColorKey] = Color.FromRgb(0, 120, 212);
            resources[SystemColors.HighlightTextColorKey] = Colors.White;
        }
    }
}

/// <summary>
/// Message for theme change notifications
/// </summary>
public class ThemeChangedMessage : MessageBase
{
    public ApplicationTheme? OldTheme { get; set; }
    public ApplicationTheme? NewTheme { get; set; }
}