using System.Diagnostics;

namespace WPFBase.Interfaces;

/// <summary>
/// Modern telemetry service interface for performance monitoring and observability.
/// Integrates with OpenTelemetry 1.9.0 and Aspire Dashboard for cutting-edge monitoring.
/// Optimized for WPF applications with minimal overhead (less than 0.5%).
/// </summary>
public interface ITelemetryService : IDisposable
{
    /// <summary>
    /// Whether telemetry collection is enabled
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Current telemetry collection level
    /// </summary>
    TelemetryLevel Level { get; set; }

    #region Activity Tracking (OpenTelemetry Tracing)

    /// <summary>
    /// Start a new activity for distributed tracing
    /// </summary>
    Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal);

    /// <summary>
    /// Record an exception with context
    /// </summary>
    void RecordException(Exception exception, Activity? activity = null);

    #endregion

    #region Metrics Collection

    /// <summary>
    /// Record a custom metric value
    /// </summary>
    void RecordMetric(string name, double value, Dictionary<string, object?>? tags = null);

    /// <summary>
    /// Increment a counter metric
    /// </summary>
    void IncrementCounter(string name, Dictionary<string, object?>? tags = null);

    /// <summary>
    /// Record operation duration
    /// </summary>
    void RecordDuration(string name, TimeSpan duration, Dictionary<string, object?>? tags = null);

    #endregion

    #region WPF-Specific Telemetry

    /// <summary>
    /// Track ViewModel initialization performance
    /// </summary>
    void TrackViewModelInitialization(string viewModelName, TimeSpan duration);

    /// <summary>
    /// Track command execution with success/failure
    /// </summary>
    void TrackCommandExecution(string commandName, TimeSpan duration, bool success);

    /// <summary>
    /// Track UI thread blocking events (>100ms)
    /// </summary>
    void TrackUIThreadBlocking(TimeSpan duration);

    /// <summary>
    /// Track data binding performance (slow bindings >10ms)
    /// </summary>
    void TrackDataBindingPerformance(string propertyName, TimeSpan duration);

    /// <summary>
    /// Track memory pressure and GC events
    /// </summary>
    void TrackMemoryPressure(long workingSet, long gcTotalMemory);

    /// <summary>
    /// Track navigation between views
    /// </summary>
    void TrackNavigation(string fromView, string toView, TimeSpan duration);

    #endregion

    #region Custom Events

    /// <summary>
    /// Track custom application events
    /// </summary>
    void TrackEvent(string eventName, Dictionary<string, string>? properties = null);

    /// <summary>
    /// Track page/view visits
    /// </summary>
    void TrackPageView(string pageName, TimeSpan? duration = null);

    /// <summary>
    /// Track user actions for analytics
    /// </summary>
    void TrackUserAction(string action, string target, Dictionary<string, string>? properties = null);

    #endregion

    #region Session Management

    /// <summary>
    /// Set the current user identifier
    /// </summary>
    void SetUserId(string userId);

    /// <summary>
    /// Set the current session identifier
    /// </summary>
    void SetSessionId(string sessionId);

    /// <summary>
    /// Add custom properties to all telemetry
    /// </summary>
    void AddGlobalProperty(string key, string value);

    #endregion

    #region Performance Helpers

    /// <summary>
    /// Execute an action with automatic timing and telemetry
    /// </summary>
    Task<T> TrackAsync<T>(string operationName, Func<Task<T>> operation, Dictionary<string, object?>? tags = null);

    /// <summary>
    /// Execute an action with automatic timing and telemetry (void return)
    /// </summary>
    Task TrackAsync(string operationName, Func<Task> operation, Dictionary<string, object?>? tags = null);

    /// <summary>
    /// Create a scoped telemetry context for related operations
    /// </summary>
    IDisposable CreateScope(string scopeName, Dictionary<string, object?>? properties = null);

    #endregion
}

/// <summary>
/// Telemetry collection levels for controlling data volume
/// </summary>
public enum TelemetryLevel
{
    /// <summary>
    /// No telemetry collection
    /// </summary>
    None = 0,

    /// <summary>
    /// Only critical errors and exceptions
    /// </summary>
    Critical = 1,

    /// <summary>
    /// Errors and exceptions
    /// </summary>
    Error = 2,

    /// <summary>
    /// Warnings, errors, and performance issues
    /// </summary>
    Warning = 3,

    /// <summary>
    /// General information, warnings, and errors
    /// </summary>
    Information = 4,

    /// <summary>
    /// Detailed debugging information (high volume)
    /// </summary>
    Verbose = 5
}

/// <summary>
/// Telemetry scope for grouping related operations
/// </summary>
public interface ITelemetryScope : IDisposable
{
    /// <summary>
    /// Add a property to this scope
    /// </summary>
    void AddProperty(string key, object? value);

    /// <summary>
    /// Mark the scope as successful
    /// </summary>
    void MarkSuccess();

    /// <summary>
    /// Mark the scope as failed with optional exception
    /// </summary>
    void MarkFailure(Exception? exception = null);
}