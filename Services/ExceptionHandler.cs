using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using WPFBase.Interfaces;

namespace WPFBase.Services;

/// <summary>
/// Global exception handler service
/// </summary>
public class ExceptionHandler
{
    private readonly LoggingService _loggingService;
    private readonly IDialogService _dialogService;
    private readonly IConfigurationService _configurationService;
    private readonly string _crashReportsPath;
    private bool _isHandlingException;

    public ExceptionHandler(
        LoggingService loggingService,
        IDialogService dialogService,
        IConfigurationService configurationService)
    {
        _loggingService = loggingService;
        _dialogService = dialogService;
        _configurationService = configurationService;
        
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _crashReportsPath = Path.Combine(appDataPath, "WPFBase", "CrashReports");
        Directory.CreateDirectory(_crashReportsPath);
    }

    /// <summary>
    /// Initialize global exception handlers
    /// </summary>
    public void Initialize()
    {
        // Handle UI thread exceptions
        Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
        
        // Handle non-UI thread exceptions
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        
        // Handle task exceptions
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        
        _loggingService.LogInformation("Exception handlers initialized");
    }

    /// <summary>
    /// Handle exception manually
    /// </summary>
    public async Task HandleExceptionAsync(Exception exception, string context = "", bool isCritical = false)
    {
        if (_isHandlingException) return;
        
        try
        {
            _isHandlingException = true;
            
            // Log the exception
            LogException(exception, context, isCritical);
            
            // Create crash report
            var reportPath = await CreateCrashReportAsync(exception, context, isCritical);
            
            // Show error dialog if critical or in developer mode
            if (isCritical || _configurationService.GetValue<bool>("Features:EnableDeveloperMode"))
            {
                await ShowErrorDialogAsync(exception, reportPath, isCritical);
            }
            
            // Send telemetry if enabled
            if (_configurationService.GetValue<bool>("Features:EnableTelemetry"))
            {
                await SendTelemetryAsync(exception, context);
            }
        }
        finally
        {
            _isHandlingException = false;
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _loggingService.LogError(e.Exception, "Unhandled UI thread exception");
        
        _ = HandleExceptionAsync(e.Exception, "UI Thread", IsCriticalException(e.Exception));
        
        // Prevent application crash for non-critical exceptions
        e.Handled = !IsCriticalException(e.Exception);
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            _loggingService.LogCritical(exception, "Unhandled application exception");
            _ = HandleExceptionAsync(exception, "AppDomain", e.IsTerminating);
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _loggingService.LogError(e.Exception, "Unobserved task exception");
        
        _ = HandleExceptionAsync(e.Exception, "Task", false);
        
        // Prevent process termination
        e.SetObserved();
    }

    private void LogException(Exception exception, string context, bool isCritical)
    {
        var message = $"Exception in {context}: {exception.Message}";
        
        if (isCritical)
        {
            _loggingService.LogCritical(exception, message);
        }
        else
        {
            _loggingService.LogError(exception, message);
        }
    }

    private async Task<string> CreateCrashReportAsync(Exception exception, string context, bool isCritical)
    {
        var reportId = Guid.NewGuid().ToString("N");
        var timestamp = DateTime.UtcNow;
        var fileName = $"crash_{timestamp:yyyyMMdd_HHmmss}_{reportId}.json";
        var filePath = Path.Combine(_crashReportsPath, fileName);
        
        var report = new CrashReport
        {
            Id = reportId,
            Timestamp = timestamp,
            Context = context,
            IsCritical = isCritical,
            ExceptionType = exception.GetType().FullName ?? "Unknown",
            Message = exception.Message,
            StackTrace = exception.StackTrace,
            InnerException = exception.InnerException?.ToString(),
            Source = exception.Source,
            TargetSite = exception.TargetSite?.ToString(),
            SystemInfo = new SystemInfo
            {
                OSVersion = Environment.OSVersion.ToString(),
                CLRVersion = Environment.Version.ToString(),
                MachineName = Environment.MachineName,
                ProcessorCount = Environment.ProcessorCount,
                Is64BitOS = Environment.Is64BitOperatingSystem,
                Is64BitProcess = Environment.Is64BitProcess,
                WorkingSet = Environment.WorkingSet,
                UserName = Environment.UserName,
                // Safe null handling for configuration service
                // GetValue may return null, so we provide a fallback value
                AppVersion = _configurationService?.GetValue<string>("ApplicationSettings:Version", "Unknown") ?? "Unknown"
            }
        };
        
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        await File.WriteAllTextAsync(filePath, json);
        
        return filePath;
    }

    private async Task ShowErrorDialogAsync(Exception exception, string reportPath, bool isCritical)
    {
        var title = isCritical ? "Critical Error" : "Application Error";
        var message = new StringBuilder();
        
        message.AppendLine(isCritical 
            ? "A critical error has occurred and the application may need to restart." 
            : "An error has occurred in the application.");
        
        message.AppendLine();
        message.AppendLine($"Error: {exception.Message}");
        
        if (_configurationService.GetValue<bool>("Features:EnableDeveloperMode"))
        {
            message.AppendLine();
            message.AppendLine($"Type: {exception.GetType().Name}");
            message.AppendLine($"Report saved to: {reportPath}");
        }
        
        await _dialogService.ShowErrorAsync(message.ToString(), title, exception);
        
        if (isCritical)
        {
            var restart = await _dialogService.ShowConfirmationAsync(
                "Would you like to restart the application?",
                "Restart Application");
            
            if (restart)
            {
                RestartApplication();
            }
        }
    }

    private async Task SendTelemetryAsync(Exception exception, string context)
    {
        try
        {
            // Implement telemetry sending logic here
            // This could send to Application Insights, Sentry, etc.
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to send telemetry");
        }
    }

    private bool IsCriticalException(Exception exception)
    {
        return exception switch
        {
            OutOfMemoryException => true,
            StackOverflowException => true,
            AccessViolationException => true,
            AppDomainUnloadedException => true,
            BadImageFormatException => true,
            _ => false
        };
    }

    private void RestartApplication()
    {
        try
        {
            var currentProcess = Process.GetCurrentProcess();
            var fileName = currentProcess.MainModule?.FileName;
            
            if (!string.IsNullOrEmpty(fileName))
            {
                Process.Start(fileName);
            }
            
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to restart application");
        }
    }

    /// <summary>
    /// Clean up old crash reports
    /// </summary>
    public void CleanupOldReports(int daysToKeep = 30)
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
            var files = Directory.GetFiles(_crashReportsPath, "*.json");
            
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    File.Delete(file);
                    _loggingService.LogInformation("Deleted old crash report: {File}", file);
                }
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to cleanup crash reports");
        }
    }
}

/// <summary>
/// Crash report model
/// </summary>
public class CrashReport
{
    public string Id { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Context { get; set; } = string.Empty;
    public bool IsCritical { get; set; }
    public string ExceptionType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? InnerException { get; set; }
    public string? Source { get; set; }
    public string? TargetSite { get; set; }
    public SystemInfo? SystemInfo { get; set; }
}

/// <summary>
/// System information for crash reports
/// </summary>
public class SystemInfo
{
    public string OSVersion { get; set; } = string.Empty;
    public string CLRVersion { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public int ProcessorCount { get; set; }
    public bool Is64BitOS { get; set; }
    public bool Is64BitProcess { get; set; }
    public long WorkingSet { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
}