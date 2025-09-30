using Microsoft.Extensions.Logging;

namespace WPFBase.Interfaces;

/// <summary>
/// Service for structured logging operations
/// </summary>
public interface ILoggingService
{
    /// <summary>
    /// Log an information message
    /// </summary>
    void LogInformation(string message, params object[] args);

    /// <summary>
    /// Log a warning message
    /// </summary>
    void LogWarning(string message, params object[] args);

    /// <summary>
    /// Log an error message
    /// </summary>
    void LogError(string message, params object[] args);

    /// <summary>
    /// Log an error with exception
    /// </summary>
    void LogError(Exception exception, string message, params object[] args);

    /// <summary>
    /// Log a debug message
    /// </summary>
    void LogDebug(string message, params object[] args);

    /// <summary>
    /// Begin a logging scope
    /// </summary>
    IDisposable BeginScope<TState>(TState state) where TState : notnull;

    /// <summary>
    /// Get the underlying logger instance
    /// </summary>
    ILogger Logger { get; }
}