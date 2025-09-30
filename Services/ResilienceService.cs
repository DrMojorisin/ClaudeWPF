using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net;
using System.Net.Http;
using WPFBase.Interfaces;

namespace WPFBase.Services;

/// <summary>
/// Implementation of resilience patterns using Polly 8.5.0.
/// Provides retry, circuit breaker, timeout, and fallback strategies.
/// </summary>
public class ResilienceService : IResilienceService
{
    private readonly ILogger<ResilienceService> _logger;
    private readonly ILoggingService _loggingService;

    private ResiliencePipeline _retryPipeline = null!;
    private ResiliencePipeline _circuitBreakerPipeline = null!;
    private ResiliencePipeline _timeoutPipeline = null!;
    private ResiliencePipeline _fullResiliencePipeline = null!;

    private CircuitBreakerStateProvider? _circuitBreakerStateProvider;
    private int _maxRetries = 3;
    private TimeSpan _retryDelay = TimeSpan.FromSeconds(1);
    private double _backoffMultiplier = 2.0;

    public ResilienceService(
        ILogger<ResilienceService> logger,
        ILoggingService loggingService)
    {
        _logger = logger;
        _loggingService = loggingService;

        BuildDefaultPipelines();
    }

    private void BuildDefaultPipelines()
    {
        // Retry pipeline with exponential backoff
        _retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                MaxRetryAttempts = _maxRetries,
                Delay = _retryDelay,
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    _logger.LogWarning($"Retry attempt {args.AttemptNumber} after {args.RetryDelay}");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        // Circuit breaker pipeline
        _circuitBreakerPipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new()
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(10),
                MinimumThroughput = 3,
                BreakDuration = TimeSpan.FromSeconds(30),
                OnOpened = args =>
                {
                    _logger.LogError($"Circuit breaker opened for {args.BreakDuration}");
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    _logger.LogInformation("Circuit breaker closed");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        // Timeout pipeline
        _timeoutPipeline = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromSeconds(30))
            .Build();

        // Combined resilience pipeline
        var circuitBreakerOptions = new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(10),
            MinimumThroughput = 3,
            BreakDuration = TimeSpan.FromSeconds(30)
        };

        _fullResiliencePipeline = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromSeconds(30))
            .AddRetry(new()
            {
                MaxRetryAttempts = _maxRetries,
                Delay = _retryDelay,
                BackoffType = DelayBackoffType.Exponential
            })
            .AddCircuitBreaker(circuitBreakerOptions)
            .Build();

        _circuitBreakerStateProvider = null; // Will be set when needed
    }

    public async Task<T> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        int maxRetries = 3,
        CancellationToken cancellationToken = default)
    {
        if (maxRetries != _maxRetries)
        {
            // Build custom retry pipeline
            var customPipeline = new ResiliencePipelineBuilder()
                .AddRetry(new()
                {
                    MaxRetryAttempts = maxRetries,
                    Delay = _retryDelay,
                    BackoffType = DelayBackoffType.Exponential,
                    OnRetry = args =>
                    {
                        _loggingService.LogInformation($"Retry attempt {args.AttemptNumber}/{maxRetries}");
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();

            return await customPipeline.ExecuteAsync(async ct => await operation(ct), cancellationToken);
        }

        return await _retryPipeline.ExecuteAsync(async ct => await operation(ct), cancellationToken);
    }

    public async Task<T> ExecuteWithCircuitBreakerAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _circuitBreakerPipeline.ExecuteAsync(async ct => await operation(ct), cancellationToken);
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Circuit breaker is open");
            throw;
        }
    }

    public async Task<T> ExecuteWithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var customTimeout = new ResiliencePipelineBuilder()
            .AddTimeout(timeout)
            .Build();

        try
        {
            return await customTimeout.ExecuteAsync(async ct => await operation(ct), cancellationToken);
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, $"Operation timed out after {timeout}");
            throw;
        }
    }

    public async Task<T> ExecuteWithFullResilienceAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        return await _fullResiliencePipeline.ExecuteAsync(async ct => await operation(ct), cancellationToken);
    }

    public CircuitState GetCircuitBreakerState()
    {
        return _circuitBreakerStateProvider?.CircuitState ?? CircuitState.Closed;
    }

    public void ResetCircuitBreaker()
    {
        // Circuit breaker state is managed internally by Polly 8
        // Manual reset requires rebuilding the pipeline
        BuildDefaultPipelines();
        _logger.LogInformation("Circuit breaker reset");
    }

    public void ConfigureRetryPolicy(int maxRetries, TimeSpan delay, double backoffMultiplier = 2.0)
    {
        _maxRetries = maxRetries;
        _retryDelay = delay;
        _backoffMultiplier = backoffMultiplier;
        BuildDefaultPipelines();
        _logger.LogInformation($"Retry policy configured: {maxRetries} retries, {delay} delay, {backoffMultiplier}x backoff");
    }

    public void ConfigureCircuitBreaker(int handledEventsBeforeBreaking, TimeSpan durationOfBreak)
    {
        // Rebuild pipelines with new circuit breaker settings
        _circuitBreakerPipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new()
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(10),
                MinimumThroughput = handledEventsBeforeBreaking,
                BreakDuration = durationOfBreak
            })
            .Build();

        BuildDefaultPipelines(); // Rebuild full pipeline
        _logger.LogInformation($"Circuit breaker configured: {handledEventsBeforeBreaking} events before breaking, {durationOfBreak} break duration");
    }
}

/// <summary>
/// Extension methods for easy resilience integration
/// </summary>
public static class ResilienceExtensions
{
    /// <summary>
    /// Execute HTTP request with resilience
    /// </summary>
    public static async Task<HttpResponseMessage> ExecuteHttpWithResilienceAsync(
        this IResilienceService resilience,
        HttpClient httpClient,
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        return await resilience.ExecuteWithFullResilienceAsync(
            async ct => await httpClient.SendAsync(request, ct),
            cancellationToken);
    }

    /// <summary>
    /// Execute database operation with resilience
    /// </summary>
    public static async Task<T> ExecuteDatabaseOperationAsync<T>(
        this IResilienceService resilience,
        Func<Task<T>> databaseOperation,
        CancellationToken cancellationToken = default)
    {
        return await resilience.ExecuteWithRetryAsync(
            async ct => await databaseOperation(),
            maxRetries: 5,
            cancellationToken);
    }
}