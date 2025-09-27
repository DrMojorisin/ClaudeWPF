namespace WPFBase.Interfaces;

/// <summary>
/// Service for managing application configuration
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Get a configuration value by key
    /// </summary>
    T? GetValue<T>(string key, T? defaultValue = default);
    
    /// <summary>
    /// Get a configuration section
    /// </summary>
    T? GetSection<T>(string sectionName) where T : class, new();
    
    /// <summary>
    /// Set a configuration value
    /// </summary>
    void SetValue<T>(string key, T value);
    
    /// <summary>
    /// Save configuration changes to file
    /// </summary>
    Task SaveAsync();
    
    /// <summary>
    /// Reload configuration from file
    /// </summary>
    Task ReloadAsync();
    
    /// <summary>
    /// Get connection string by name
    /// </summary>
    string? GetConnectionString(string name);
    
    /// <summary>
    /// Check if a configuration key exists
    /// </summary>
    bool Contains(string key);
    
    /// <summary>
    /// Event raised when configuration changes
    /// </summary>
    event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;
}

/// <summary>
/// Configuration changed event arguments
/// </summary>
public class ConfigurationChangedEventArgs : EventArgs
{
    public string Key { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }
    
    public ConfigurationChangedEventArgs(string key, object? oldValue, object? newValue)
    {
        Key = key;
        OldValue = oldValue;
        NewValue = newValue;
    }
}

/// <summary>
/// Application settings configuration
/// </summary>
public class ApplicationSettings
{
    public string ApplicationName { get; set; } = "WPF Application";
    public string Version { get; set; } = "1.0.0";
    public string Theme { get; set; } = "Light";
    public string Language { get; set; } = "en-US";
    public bool EnableAutoSave { get; set; }
    public int AutoSaveIntervalMinutes { get; set; } = 5;
    public int MaxRecentFiles { get; set; } = 10;
}

/// <summary>
/// Window settings configuration
/// </summary>
public class WindowConfigSettings
{
    public bool RememberWindowPosition { get; set; } = true;
    public string StartupWindowState { get; set; } = "Normal";
    public int DefaultWidth { get; set; } = 1024;
    public int DefaultHeight { get; set; } = 768;
}

/// <summary>
/// Feature flags configuration
/// </summary>
public class FeatureSettings
{
    public bool EnableExperimentalFeatures { get; set; }
    public bool EnableDeveloperMode { get; set; }
    public bool EnableTelemetry { get; set; }
}