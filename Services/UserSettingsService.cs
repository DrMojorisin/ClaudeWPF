using System.IO;
using System.Text.Json;
using System.Windows;
using WPFBase.Interfaces;

namespace WPFBase.Services;

/// <summary>
/// Service for managing user-specific settings and preferences with JSON persistence
/// </summary>
public class UserSettingsService : IUserSettingsService
{
    private readonly string _settingsFilePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private UserSettingsData _settings;

    public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    public int MaxRecentFiles { get; private set; } = 10;

    public UserSettingsService()
    {
        var userDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appName = "WPFBase";
        var settingsDirectory = Path.Combine(userDataPath, appName);
        _settingsFilePath = Path.Combine(settingsDirectory, "user-settings.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        _settings = new UserSettingsData();
        LoadSettings();
    }

    public T? GetSetting<T>(string key, T? defaultValue = default)
    {
        if (_settings.Settings.TryGetValue(key, out var value))
        {
            try
            {
                if (value is JsonElement jsonElement)
                {
                    return JsonSerializer.Deserialize<T>(jsonElement.GetRawText(), _jsonOptions);
                }
                return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, _jsonOptions), _jsonOptions);
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    public void SetSetting<T>(string key, T value)
    {
        var oldValue = GetSetting<T>(key);
        _settings.Settings[key] = value;
        SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(key, oldValue, value));
    }

    public bool HasSetting(string key)
    {
        return _settings.Settings.ContainsKey(key);
    }

    public void RemoveSetting(string key)
    {
        if (_settings.Settings.ContainsKey(key))
        {
            var oldValue = GetSetting<object>(key);
            _settings.Settings.Remove(key);
            SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(key, oldValue, null));
        }
    }

    public List<RecentFile> GetRecentFiles()
    {
        return _settings.RecentFiles
            .OrderByDescending(f => f.LastAccessed)
            .Take(MaxRecentFiles)
            .ToList();
    }

    public void AddRecentFile(string filePath, string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        // Remove existing entry if it exists
        _settings.RecentFiles.RemoveAll(f => string.Equals(f.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

        var recentFile = new RecentFile
        {
            FilePath = filePath,
            DisplayName = displayName ?? Path.GetFileName(filePath),
            LastAccessed = DateTime.Now,
            FileSize = File.Exists(filePath) ? new FileInfo(filePath).Length : 0
        };

        _settings.RecentFiles.Insert(0, recentFile);

        // Keep only the maximum number of recent files
        if (_settings.RecentFiles.Count > MaxRecentFiles)
        {
            _settings.RecentFiles.RemoveRange(MaxRecentFiles, _settings.RecentFiles.Count - MaxRecentFiles);
        }
    }

    public void RemoveRecentFile(string filePath)
    {
        _settings.RecentFiles.RemoveAll(f => string.Equals(f.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
    }

    public void ClearRecentFiles()
    {
        _settings.RecentFiles.Clear();
    }

    public void SaveWindowState(string windowKey, WindowState windowState, double left, double top, double width, double height)
    {
        _settings.WindowStates[windowKey] = new WindowSettings
        {
            WindowState = windowState,
            Left = left,
            Top = top,
            Width = width,
            Height = height
        };
    }

    public WindowSettings? RestoreWindowState(string windowKey)
    {
        return _settings.WindowStates.TryGetValue(windowKey, out var settings) ? settings : null;
    }

    public async Task SaveAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Clean up recent files (remove non-existent files older than 30 days)
            var cutoffDate = DateTime.Now.AddDays(-30);
            _settings.RecentFiles.RemoveAll(f => !f.FileExists && f.LastAccessed < cutoffDate);

            var json = JsonSerializer.Serialize(_settings, _jsonOptions);
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving user settings: {ex}");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ReloadAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            LoadSettings();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                var loadedSettings = JsonSerializer.Deserialize<UserSettingsData>(json, _jsonOptions);
                if (loadedSettings != null)
                {
                    _settings = loadedSettings;
                    
                    // Update MaxRecentFiles if it's set in settings
                    if (_settings.Settings.TryGetValue("MaxRecentFiles", out var maxFiles))
                    {
                        if (maxFiles is JsonElement element && element.TryGetInt32(out var value))
                        {
                            MaxRecentFiles = Math.Max(1, Math.Min(value, 20)); // Limit between 1 and 20
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading user settings: {ex}");
            _settings = new UserSettingsData();
        }
    }
}

/// <summary>
/// Internal data structure for persisting user settings
/// </summary>
internal class UserSettingsData
{
    public Dictionary<string, object> Settings { get; set; } = new();
    public List<RecentFile> RecentFiles { get; set; } = new();
    public Dictionary<string, WindowSettings> WindowStates { get; set; } = new();
}