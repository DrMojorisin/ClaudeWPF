using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WPFBase.Interfaces;

namespace WPFBase.Services;

/// <summary>
/// Modern telemetry service implementing OpenTelemetry 1.9.0 with Aspire Dashboard integration.
/// Provides comprehensive observability for WPF applications with minimal performance impact.
///
/// Configuration for Aspire Dashboard:
/// 1. Start dashboard: docker run --rm -it -p 18888:18888 -p 4317:4317 mcr.microsoft.com/dotnet/aspire-dashboard:9.5
/// 2. Dashboard UI: http://localhost:18888
/// 3. OTLP Endpoint: http://localhost:4317
/// </summary>
public class TelemetryService : ITelemetryService
{
    private readonly ActivitySource _activitySource;
    private readonly Meter _meter;
    private readonly ILoggingService _logger;
    private readonly IConfigurationService _config;

    // Metrics instruments
    private readonly Counter<long> _commandExecutionCounter;
    private readonly Histogram<double> _commandDurationHistogram;
    private readonly Counter<long> _exceptionCounter;
    private readonly Histogram<double> _viewModelInitHistogram;
    private readonly Histogram<double> _uiBlockingHistogram;
    private readonly Histogram<double> _navigationHistogram;
    private readonly Counter<long> _userActionCounter;

    // Configuration
    private readonly string _serviceName;
    private readonly string _serviceVersion;
    private readonly Dictionary<string, string> _globalProperties;

    public bool IsEnabled { get; private set; }
    public TelemetryLevel Level { get; set; }

    public TelemetryService(
        IConfigurationService config,
        ILoggingService logger)
    {
        _config = config;
        _logger = logger;
        _serviceName = config.GetValue("Telemetry:ServiceName", "WPFBase");
        _serviceVersion = GetAssemblyVersion();
        _globalProperties = new Dictionary<string, string>();

        // Initialize OpenTelemetry components
        _activitySource = new ActivitySource(_serviceName, _serviceVersion);
        _meter = new Meter(_serviceName, _serviceVersion);

        // Create metric instruments
        _commandExecutionCounter = _meter.CreateCounter<long>(
            "wpfbase_command_executions_total",
            description: "Total number of command executions");

        _commandDurationHistogram = _meter.CreateHistogram<double>(
            "wpfbase_command_duration_ms",
            unit: "ms",
            description: "Command execution duration in milliseconds");

        _exceptionCounter = _meter.CreateCounter<long>(
            "wpfbase_exceptions_total",
            description: "Total number of exceptions");

        _viewModelInitHistogram = _meter.CreateHistogram<double>(
            "wpfbase_viewmodel_init_duration_ms",
            unit: "ms",
            description: "ViewModel initialization duration in milliseconds");

        _uiBlockingHistogram = _meter.CreateHistogram<double>(
            "wpfbase_ui_blocking_duration_ms",
            unit: "ms",
            description: "UI thread blocking duration in milliseconds");

        _navigationHistogram = _meter.CreateHistogram<double>(
            "wpfbase_navigation_duration_ms",
            unit: "ms",
            description: "Navigation duration in milliseconds");

        _userActionCounter = _meter.CreateCounter<long>(
            "wpfbase_user_actions_total",
            description: "Total number of user actions");

        // Initialize telemetry
        InitializeTelemetry();
    }

    private void InitializeTelemetry()
    {
        try
        {
            IsEnabled = _config.GetValue("Telemetry:Enabled", false);
            Level = _config.GetValue("Telemetry:Level", TelemetryLevel.Information);

            if (!IsEnabled)
            {
                _logger.LogInformation("Telemetry disabled by configuration");
                return;
            }

            var otlpEndpoint = _config.GetValue("Telemetry:OtlpEndpoint", "http://localhost:4317");

            // Configure OpenTelemetry with Aspire Dashboard
            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(_serviceName, _serviceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = _config.GetValue("Environment", "Development"),
                    ["telemetry.sdk.language"] = "dotnet",
                    ["telemetry.sdk.version"] = "1.9.0",
                    ["service.instance.id"] = Environment.MachineName,
                    ["application.type"] = "WPF Desktop"
                });

            // Initialize tracing
            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddSource(_serviceName)
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                })
                .Build();

            // Initialize metrics
            using var meterProvider = Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddMeter(_serviceName)
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                })
                .Build();

            _logger.LogInformation("Telemetry initialized with Aspire Dashboard integration");
            _logger.LogInformation("Dashboard available at: http://localhost:18888");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize telemetry");
            IsEnabled = false;
        }
    }

    #region Activity Tracking

    public Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        if (!IsEnabled) return null;

        var activity = _activitySource.StartActivity(name, kind);
        activity?.SetTag("service.name", _serviceName);
        activity?.SetTag("service.version", _serviceVersion);

        // Add global properties
        foreach (var prop in _globalProperties)
        {
            activity?.SetTag(prop.Key, prop.Value);
        }

        return activity;
    }

    public void RecordException(Exception exception, Activity? activity = null)
    {
        if (!IsEnabled) return;

        _exceptionCounter.Add(1,
            new KeyValuePair<string, object?>("exception.type", exception.GetType().Name),
            new KeyValuePair<string, object?>("exception.message", exception.Message));

        activity?.RecordException(exception);
        activity?.SetStatus(ActivityStatusCode.Error, exception.Message);

        _logger.LogError(exception, "Exception recorded in telemetry");
    }

    #endregion

    #region Metrics Collection

    public void RecordMetric(string name, double value, Dictionary<string, object?>? tags = null)
    {
        if (!IsEnabled) return;

        // For custom metrics, we can use the generic meter or create specific instruments
        var histogram = _meter.CreateHistogram<double>(name);
        var tagList = ConvertToTagList(tags);
        histogram.Record(value, tagList);
    }

    public void IncrementCounter(string name, Dictionary<string, object?>? tags = null)
    {
        if (!IsEnabled) return;

        var counter = _meter.CreateCounter<long>(name);
        var tagList = ConvertToTagList(tags);
        counter.Add(1, tagList);
    }

    public void RecordDuration(string name, TimeSpan duration, Dictionary<string, object?>? tags = null)
    {
        RecordMetric(name, duration.TotalMilliseconds, tags);
    }

    #endregion

    #region WPF-Specific Telemetry

    public void TrackViewModelInitialization(string viewModelName, TimeSpan duration)
    {
        if (!IsEnabled) return;

        _viewModelInitHistogram.Record(duration.TotalMilliseconds,
            new KeyValuePair<string, object?>("viewmodel.name", viewModelName));

        using var activity = StartActivity($"ViewModel.Initialize.{viewModelName}");
        activity?.SetTag("viewmodel.name", viewModelName);
        activity?.SetTag("duration_ms", duration.TotalMilliseconds);
    }

    public void TrackCommandExecution(string commandName, TimeSpan duration, bool success)
    {
        if (!IsEnabled) return;

        _commandExecutionCounter.Add(1,
            new KeyValuePair<string, object?>("command.name", commandName),
            new KeyValuePair<string, object?>("command.success", success));

        _commandDurationHistogram.Record(duration.TotalMilliseconds,
            new KeyValuePair<string, object?>("command.name", commandName),
            new KeyValuePair<string, object?>("command.success", success));

        using var activity = StartActivity($"Command.{commandName}");
        activity?.SetTag("command.name", commandName);
        activity?.SetTag("command.success", success);
        activity?.SetTag("duration_ms", duration.TotalMilliseconds);

        if (!success)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Command execution failed");
        }
    }

    public void TrackUIThreadBlocking(TimeSpan duration)
    {
        if (!IsEnabled || duration.TotalMilliseconds <= 100) return;

        _uiBlockingHistogram.Record(duration.TotalMilliseconds);

        using var activity = StartActivity("UI.ThreadBlocking");
        activity?.SetTag("duration_ms", duration.TotalMilliseconds);
        activity?.SetTag("severity", duration.TotalMilliseconds > 1000 ? "High" : "Medium");

        if (duration.TotalMilliseconds > 1000)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "UI thread blocked for over 1 second");
        }
    }

    public void TrackDataBindingPerformance(string propertyName, TimeSpan duration)
    {
        if (!IsEnabled || Level < TelemetryLevel.Verbose || duration.TotalMilliseconds <= 10) return;

        using var activity = StartActivity("DataBinding.PropertyChanged");
        activity?.SetTag("property.name", propertyName);
        activity?.SetTag("duration_ms", duration.TotalMilliseconds);

        if (duration.TotalMilliseconds > 50)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Slow data binding detected");
        }
    }

    public void TrackMemoryPressure(long workingSet, long gcTotalMemory)
    {
        if (!IsEnabled) return;

        var workingSetMB = workingSet / (1024.0 * 1024.0);
        var managedMemoryMB = gcTotalMemory / (1024.0 * 1024.0);

        RecordMetric("wpfbase_memory_working_set_mb", workingSetMB);
        RecordMetric("wpfbase_memory_managed_mb", managedMemoryMB);

        using var activity = StartActivity("Memory.Pressure");
        activity?.SetTag("memory.working_set_mb", workingSetMB);
        activity?.SetTag("memory.managed_mb", managedMemoryMB);
    }

    public void TrackNavigation(string fromView, string toView, TimeSpan duration)
    {
        if (!IsEnabled) return;

        _navigationHistogram.Record(duration.TotalMilliseconds,
            new KeyValuePair<string, object?>("navigation.from", fromView),
            new KeyValuePair<string, object?>("navigation.to", toView));

        using var activity = StartActivity("Navigation");
        activity?.SetTag("navigation.from", fromView);
        activity?.SetTag("navigation.to", toView);
        activity?.SetTag("duration_ms", duration.TotalMilliseconds);
    }

    #endregion

    #region Custom Events

    public void TrackEvent(string eventName, Dictionary<string, string>? properties = null)
    {
        if (!IsEnabled) return;

        using var activity = StartActivity($"Event.{eventName}");
        activity?.SetTag("event.name", eventName);

        if (properties != null)
        {
            foreach (var prop in properties)
            {
                activity?.SetTag($"event.{prop.Key}", prop.Value);
            }
        }
    }

    public void TrackPageView(string pageName, TimeSpan? duration = null)
    {
        if (!IsEnabled) return;

        using var activity = StartActivity("PageView");
        activity?.SetTag("page.name", pageName);

        if (duration.HasValue)
        {
            activity?.SetTag("duration_ms", duration.Value.TotalMilliseconds);
        }
    }

    public void TrackUserAction(string action, string target, Dictionary<string, string>? properties = null)
    {
        if (!IsEnabled) return;

        _userActionCounter.Add(1,
            new KeyValuePair<string, object?>("action.name", action),
            new KeyValuePair<string, object?>("action.target", target));

        using var activity = StartActivity("UserAction");
        activity?.SetTag("action.name", action);
        activity?.SetTag("action.target", target);

        if (properties != null)
        {
            foreach (var prop in properties)
            {
                activity?.SetTag($"action.{prop.Key}", prop.Value);
            }
        }
    }

    #endregion

    #region Session Management

    public void SetUserId(string userId)
    {
        AddGlobalProperty("user.id", userId);
    }

    public void SetSessionId(string sessionId)
    {
        AddGlobalProperty("session.id", sessionId);
    }

    public void AddGlobalProperty(string key, string value)
    {
        _globalProperties[key] = value;
    }

    #endregion

    #region Performance Helpers

    public async Task<T> TrackAsync<T>(string operationName, Func<Task<T>> operation, Dictionary<string, object?>? tags = null)
    {
        if (!IsEnabled)
        {
            return await operation();
        }

        using var activity = StartActivity(operationName);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    activity?.SetTag(tag.Key, tag.Value?.ToString());
                }
            }

            var result = await operation();
            stopwatch.Stop();

            activity?.SetTag("duration_ms", stopwatch.ElapsedMilliseconds);
            activity?.SetTag("operation.success", true);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetTag("duration_ms", stopwatch.ElapsedMilliseconds);
            activity?.SetTag("operation.success", false);
            RecordException(ex, activity);
            throw;
        }
    }

    public async Task TrackAsync(string operationName, Func<Task> operation, Dictionary<string, object?>? tags = null)
    {
        await TrackAsync(operationName, async () =>
        {
            await operation();
            return true;
        }, tags);
    }

    public IDisposable CreateScope(string scopeName, Dictionary<string, object?>? properties = null)
    {
        return new TelemetryScope(this, scopeName, properties);
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        _activitySource?.Dispose();
        _meter?.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Private Helpers

    private static string GetAssemblyVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly()
            .GetName().Version?.ToString() ?? "1.0.0";
    }

    private static TagList ConvertToTagList(Dictionary<string, object?>? tags)
    {
        var tagList = new TagList();
        if (tags != null)
        {
            foreach (var tag in tags)
            {
                tagList.Add(tag.Key, tag.Value);
            }
        }
        return tagList;
    }

    #endregion
}

/// <summary>
/// Telemetry scope implementation for grouping related operations
/// </summary>
internal class TelemetryScope : ITelemetryScope
{
    private readonly TelemetryService _telemetryService;
    private readonly Activity? _activity;
    private readonly Stopwatch _stopwatch;
    private bool _disposed;

    public TelemetryScope(TelemetryService telemetryService, string scopeName, Dictionary<string, object?>? properties)
    {
        _telemetryService = telemetryService;
        _activity = telemetryService.StartActivity($"Scope.{scopeName}");
        _stopwatch = Stopwatch.StartNew();

        if (properties != null)
        {
            foreach (var prop in properties)
            {
                _activity?.SetTag(prop.Key, prop.Value?.ToString());
            }
        }
    }

    public void AddProperty(string key, object? value)
    {
        _activity?.SetTag(key, value?.ToString());
    }

    public void MarkSuccess()
    {
        _activity?.SetTag("scope.success", true);
        _activity?.SetStatus(ActivityStatusCode.Ok);
    }

    public void MarkFailure(Exception? exception = null)
    {
        _activity?.SetTag("scope.success", false);
        _activity?.SetStatus(ActivityStatusCode.Error, exception?.Message ?? "Operation failed");

        if (exception != null)
        {
            _telemetryService.RecordException(exception, _activity);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _stopwatch.Stop();
        _activity?.SetTag("scope.duration_ms", _stopwatch.ElapsedMilliseconds);
        _activity?.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}