using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;
using System.IO;
using WPFBase.Interfaces;

namespace WPFBase.Services;

/// <summary>
/// Centralized logging service using Serilog
/// </summary>
public class LoggingService : ILoggingService, IDisposable
{
    private readonly Serilog.ILogger _logger;
    private readonly IConfigurationService _configurationService;
    private readonly string _logDirectory;

    /// <summary>
    /// Get the underlying Microsoft.Extensions.Logging.ILogger instance
    /// </summary>
    public Microsoft.Extensions.Logging.ILogger Logger => CreateLogger("WPFBase.Services.LoggingService");
    
    public LoggingService(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
        
        // Get log configuration
        var logPath = _configurationService.GetValue<string>("Logging:LogFilePath", "logs/app.log");
        var logLevel = _configurationService.GetValue<string>("Logging:LogLevel:Default", "Information");
        var enableFileLogging = _configurationService.GetValue<bool>("Logging:EnableFileLogging", true);
        var maxLogFileSize = _configurationService.GetValue<long>("Logging:MaxLogFileSize", 10485760); // 10MB
        var maxLogFiles = _configurationService.GetValue<int>("Logging:MaxLogFiles", 5);
        
        // Ensure log directory exists
        _logDirectory = Path.GetDirectoryName(logPath) ?? "logs";
        if (!Path.IsPathRooted(_logDirectory))
        {
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _logDirectory);
        }
        Directory.CreateDirectory(_logDirectory);
        
        // Safely handle potential null from Path.GetFileName
        // If logPath is null/empty, fallback to default filename
        var fileName = Path.GetFileName(logPath) ?? "application.log";
        var fullLogPath = Path.Combine(_logDirectory, fileName);
        
        // Configure Serilog
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(ParseLogLevel(logLevel ?? "Information"))
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Debug();
        
        if (enableFileLogging)
        {
            loggerConfig.WriteTo.File(
                fullLogPath,
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: maxLogFileSize,
                retainedFileCountLimit: maxLogFiles,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");
        }
        
        _logger = loggerConfig.CreateLogger();
        
        // Set as global logger
        Log.Logger = _logger;
    }
    
    /// <summary>
    /// Create a logger for a specific type
    /// </summary>
    public ILogger<T> CreateLogger<T>()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog(_logger);
        });
        
        return loggerFactory.CreateLogger<T>();
    }
    
    /// <summary>
    /// Create a logger with a custom category name
    /// </summary>
    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog(_logger);
        });
        
        return loggerFactory.CreateLogger(categoryName);
    }
    
    /// <summary>
    /// Log an information message
    /// </summary>
    public void LogInformation(string message, params object[] args)
    {
        _logger.Information(message, args);
    }
    
    /// <summary>
    /// Log a debug message
    /// </summary>
    public void LogDebug(string message, params object[] args)
    {
        _logger.Debug(message, args);
    }
    
    /// <summary>
    /// Log a warning message
    /// </summary>
    public void LogWarning(string message, params object[] args)
    {
        _logger.Warning(message, args);
    }
    
    /// <summary>
    /// Log an error message
    /// </summary>
    public void LogError(Exception exception, string message, params object[] args)
    {
        _logger.Error(exception, message, args);
    }
    
    /// <summary>
    /// Log an error message without exception
    /// </summary>
    public void LogError(string message, params object[] args)
    {
        _logger.Error(message, args);
    }
    
    /// <summary>
    /// Log a critical error
    /// </summary>
    public void LogCritical(Exception exception, string message, params object[] args)
    {
        _logger.Fatal(exception, message, args);
    }
    
    /// <summary>
    /// Create a scoped logger with additional context properties
    /// </summary>
    public IDisposable BeginScope(string name, object properties)
    {
        return _logger.ForContext(name, properties, destructureObjects: true)
            as IDisposable ?? new NoOpDisposable();
    }

    /// <summary>
    /// Begin a logging scope
    /// </summary>
    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return _logger.ForContext("Scope", state, destructureObjects: true)
            as IDisposable ?? new NoOpDisposable();
    }
    
    /// <summary>
    /// Get the path to the current log file
    /// </summary>
    public string GetCurrentLogFilePath()
    {
        var files = Directory.GetFiles(_logDirectory, "*.log")
            .OrderByDescending(f => new FileInfo(f).LastWriteTime)
            .FirstOrDefault();
        
        return files ?? Path.Combine(_logDirectory, "app.log");
    }
    
    /// <summary>
    /// Clear old log files
    /// </summary>
    public void ClearOldLogs(int daysToKeep = 30)
    {
        var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
        var files = Directory.GetFiles(_logDirectory, "*.log");
        
        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            if (fileInfo.LastWriteTime < cutoffDate)
            {
                try
                {
                    File.Delete(file);
                    LogInformation("Deleted old log file: {LogFile}", file);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Failed to delete old log file: {LogFile}", file);
                }
            }
        }
    }
    
    private LogEventLevel ParseLogLevel(string level)
    {
        return level?.ToLower() switch
        {
            "verbose" => LogEventLevel.Verbose,
            "debug" => LogEventLevel.Debug,
            "information" or "info" => LogEventLevel.Information,
            "warning" or "warn" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "critical" or "fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
    
    public void Dispose()
    {
        Log.CloseAndFlush();
    }
    
    private class NoOpDisposable : IDisposable
    {
        public void Dispose() { }
    }
}

/// <summary>
/// Static logger helper for easy access throughout the application
/// </summary>
public static class Logger
{
    private static LoggingService? _loggingService;
    
    public static void Initialize(LoggingService loggingService)
    {
        _loggingService = loggingService;
    }
    
    public static void Info(string message, params object[] args)
        => _loggingService?.LogInformation(message, args);
    
    public static void Debug(string message, params object[] args)
        => _loggingService?.LogDebug(message, args);
    
    public static void Warning(string message, params object[] args)
        => _loggingService?.LogWarning(message, args);
    
    public static void Error(Exception ex, string message, params object[] args)
        => _loggingService?.LogError(ex, message, args);
    
    public static void Error(string message, params object[] args)
        => _loggingService?.LogError(message, args);
    
    public static void Critical(Exception ex, string message, params object[] args)
        => _loggingService?.LogCritical(ex, message, args);
}