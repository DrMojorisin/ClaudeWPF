using Polly;
using Polly.CircuitBreaker;

namespace WPFBase.Interfaces;

/// <summary>
/// Service for implementing resilience patterns using Polly.
/// Provides retry policies, circuit breakers, and timeout handling.
/// Optimized for WPF desktop applications and Claude Code development.
/// </summary>
public interface IResilienceService
{
    /// <summary>
    /// Execute an operation with retry policy
    /// </summary>
    Task<T> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        int maxRetries = 3,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute an operation with circuit breaker
    /// </summary>
    Task<T> ExecuteWithCircuitBreakerAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute an operation with timeout
    /// </summary>
    Task<T> ExecuteWithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        TimeSpan timeout,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute an operation with combined resilience strategies
    /// </summary>
    Task<T> ExecuteWithFullResilienceAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get circuit breaker state
    /// </summary>
    CircuitState GetCircuitBreakerState();

    /// <summary>
    /// Reset circuit breaker
    /// </summary>
    void ResetCircuitBreaker();

    /// <summary>
    /// Configure custom retry policy
    /// </summary>
    void ConfigureRetryPolicy(int maxRetries, TimeSpan delay, double backoffMultiplier = 2.0);

    /// <summary>
    /// Configure circuit breaker
    /// </summary>
    void ConfigureCircuitBreaker(int handledEventsBeforeBreaking, TimeSpan durationOfBreak);
}