using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using WPFBase.Interfaces;

namespace WPFBase.Services;

/// <summary>
/// Service for managing application configuration using appsettings.json
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly string _configFilePath;
    private readonly string _userConfigFilePath;
    private JsonNode? _configuration;
    private JsonNode? _userConfiguration;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions;

    public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    public ConfigurationService()
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _configFilePath = Path.Combine(appDirectory, "appsettings.json");
        
        var userDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appName = "WPFBase";
        _userConfigFilePath = Path.Combine(userDataPath, appName, "usersettings.json");
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
        
        LoadConfiguration();
    }

    public T? GetValue<T>(string key, T? defaultValue = default)
    {
        try
        {
            // Try user configuration first
            var userValue = GetValueFromJson(_userConfiguration, key);
            if (userValue != null)
            {
                return JsonSerializer.Deserialize<T>(userValue.ToJsonString(), _jsonOptions);
            }
            
            // Fall back to default configuration
            var value = GetValueFromJson(_configuration, key);
            if (value != null)
            {
                return JsonSerializer.Deserialize<T>(value.ToJsonString(), _jsonOptions);
            }
            
            return defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    public T? GetSection<T>(string sectionName) where T : class, new()
    {
        try
        {
            // Merge user and default sections
            var defaultSection = GetValueFromJson(_configuration, sectionName);
            var userSection = GetValueFromJson(_userConfiguration, sectionName);
            
            if (userSection != null)
            {
                // If user section exists, use it (could merge with default if needed)
                return JsonSerializer.Deserialize<T>(userSection.ToJsonString(), _jsonOptions);
            }
            
            if (defaultSection != null)
            {
                return JsonSerializer.Deserialize<T>(defaultSection.ToJsonString(), _jsonOptions);
            }
            
            return new T();
        }
        catch
        {
            return new T();
        }
    }

    public void SetValue<T>(string key, T value)
    {
        _semaphore.Wait();
        try
        {
            var oldValue = GetValue<T>(key);
            
            // Create or update user configuration
            if (_userConfiguration == null)
            {
                _userConfiguration = new JsonObject();
            }
            
            SetValueInJson(_userConfiguration, key, JsonSerializer.SerializeToNode(value, _jsonOptions));
            
            ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(key, oldValue, value));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SaveAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_userConfiguration != null)
            {
                // Ensure directory exists
                var directory = Path.GetDirectoryName(_userConfigFilePath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                var json = _userConfiguration.ToJsonString(_jsonOptions);
                await File.WriteAllTextAsync(_userConfigFilePath, json);
            }
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
            LoadConfiguration();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public string? GetConnectionString(string name)
    {
        return GetValue<string>($"ConnectionStrings:{name}");
    }

    public bool Contains(string key)
    {
        return GetValueFromJson(_userConfiguration, key) != null || 
               GetValueFromJson(_configuration, key) != null;
    }

    private void LoadConfiguration()
    {
        try
        {
            // Load default configuration
            if (File.Exists(_configFilePath))
            {
                var json = File.ReadAllText(_configFilePath);
                _configuration = JsonNode.Parse(json);
            }
            else
            {
                // Create default configuration
                _configuration = CreateDefaultConfiguration();
                SaveDefaultConfiguration();
            }
            
            // Load user configuration
            if (File.Exists(_userConfigFilePath))
            {
                var userJson = File.ReadAllText(_userConfigFilePath);
                _userConfiguration = JsonNode.Parse(userJson);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading configuration: {ex}");
            _configuration = CreateDefaultConfiguration();
        }
    }

    private JsonNode CreateDefaultConfiguration()
    {
        var config = new JsonObject
        {
            ["ApplicationSettings"] = new JsonObject
            {
                ["ApplicationName"] = "WPF MVVM Application",
                ["Version"] = "1.0.0",
                ["Theme"] = "Light",
                ["Language"] = "en-US",
                ["EnableAutoSave"] = true,
                ["AutoSaveIntervalMinutes"] = 5,
                ["MaxRecentFiles"] = 10
            },
            ["WindowSettings"] = new JsonObject
            {
                ["RememberWindowPosition"] = true,
                ["StartupWindowState"] = "Normal",
                ["DefaultWidth"] = 1024,
                ["DefaultHeight"] = 768
            },
            ["Logging"] = new JsonObject
            {
                ["LogLevel"] = new JsonObject
                {
                    ["Default"] = "Information",
                    ["System"] = "Warning",
                    ["Microsoft"] = "Warning"
                },
                ["EnableFileLogging"] = true,
                ["LogFilePath"] = "logs/app.log",
                ["MaxLogFileSize"] = 10485760,
                ["MaxLogFiles"] = 5
            },
            ["ConnectionStrings"] = new JsonObject
            {
                ["DefaultConnection"] = "Data Source=app.db;Version=3;"
            },
            ["Features"] = new JsonObject
            {
                ["EnableExperimentalFeatures"] = false,
                ["EnableDeveloperMode"] = false,
                ["EnableTelemetry"] = false
            }
        };
        
        return config;
    }

    private void SaveDefaultConfiguration()
    {
        try
        {
            if (_configuration != null)
            {
                var json = _configuration.ToJsonString(_jsonOptions);
                File.WriteAllText(_configFilePath, json);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving default configuration: {ex}");
        }
    }

    private JsonNode? GetValueFromJson(JsonNode? node, string key)
    {
        if (node == null) return null;
        
        var keys = key.Split(':');
        var current = node;
        
        foreach (var k in keys)
        {
            if (current is JsonObject obj && obj.ContainsKey(k))
            {
                current = obj[k];
            }
            else
            {
                return null;
            }
        }
        
        return current;
    }

    private void SetValueInJson(JsonNode node, string key, JsonNode? value)
    {
        var keys = key.Split(':');
        var current = node;
        
        for (int i = 0; i < keys.Length - 1; i++)
        {
            var k = keys[i];
            if (current is JsonObject obj)
            {
                if (!obj.ContainsKey(k))
                {
                    obj[k] = new JsonObject();
                }
                current = obj[k];
            }
        }
        
        if (current is JsonObject finalObj)
        {
            finalObj[keys[^1]] = value;
        }
    }
}