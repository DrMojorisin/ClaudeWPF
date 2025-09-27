using System.IO;
using System.Windows;

namespace WPFBase.Interfaces;

/// <summary>
/// Service for managing user-specific settings and preferences
/// </summary>
public interface IUserSettingsService
{
    /// <summary>
    /// Event raised when settings change
    /// </summary>
    event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    /// <summary>
    /// Get a setting value with a default fallback
    /// </summary>
    T? GetSetting<T>(string key, T? defaultValue = default);

    /// <summary>
    /// Set a setting value
    /// </summary>
    void SetSetting<T>(string key, T value);

    /// <summary>
    /// Check if a setting exists
    /// </summary>
    bool HasSetting(string key);

    /// <summary>
    /// Remove a setting
    /// </summary>
    void RemoveSetting(string key);

    /// <summary>
    /// Get all recent files
    /// </summary>
    List<RecentFile> GetRecentFiles();

    /// <summary>
    /// Add a file to recent files list
    /// </summary>
    void AddRecentFile(string filePath, string? displayName = null);

    /// <summary>
    /// Remove a file from recent files list
    /// </summary>
    void RemoveRecentFile(string filePath);

    /// <summary>
    /// Clear all recent files
    /// </summary>
    void ClearRecentFiles();

    /// <summary>
    /// Save window state
    /// </summary>
    void SaveWindowState(string windowKey, WindowState windowState, double left, double top, double width, double height);

    /// <summary>
    /// Restore window state
    /// </summary>
    WindowSettings? RestoreWindowState(string windowKey);

    /// <summary>
    /// Save all settings to storage
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// Reload settings from storage
    /// </summary>
    Task ReloadAsync();

    /// <summary>
    /// Get maximum number of recent files to keep
    /// </summary>
    int MaxRecentFiles { get; }
}

/// <summary>
/// Represents a recent file entry
/// </summary>
public class RecentFile
{
    public string FilePath { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime LastAccessed { get; set; }
    public long FileSize { get; set; }
    public bool FileExists => File.Exists(FilePath);
}

/// <summary>
/// Window settings for state persistence
/// </summary>
public class WindowSettings
{
    public WindowState WindowState { get; set; }
    public double Left { get; set; }
    public double Top { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}

/// <summary>
/// Event arguments for settings change notification
/// </summary>
public class SettingsChangedEventArgs : EventArgs
{
    public string Key { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }

    public SettingsChangedEventArgs(string key, object? oldValue, object? newValue)
    {
        Key = key;
        OldValue = oldValue;
        NewValue = newValue;
    }
}