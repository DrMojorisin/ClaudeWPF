# Resilience Patterns Guide

## Overview

**ResilienceService** provides production-ready resilience patterns using **Polly 8.5.0** for WPF applications. It implements retry logic, circuit breakers, timeouts, and combined resilience strategies to handle transient failures in external services, APIs, and databases.

### Why Resilience Matters

Modern WPF applications interact with unreliable resources:
- HTTP APIs with network timeouts
- Database connections that fail temporarily
- Cloud services with rate limiting
- File system operations that lock

**ResilienceService** automatically handles these failures with proven patterns, preventing cascading failures and improving user experience.

### Key Features

- **Retry with Exponential Backoff** - Automatically retry failed operations with increasing delays
- **Circuit Breaker** - Prevent repeated calls to failing services
- **Timeout** - Enforce operation time limits
- **Full Resilience** - Combined strategies for maximum protection
- **Polly 8.x Lambda Pattern** - Modern async/await integration
- **Extension Methods** - Easy HTTP and database integration

---

## Quick Start

### 1. Inject the Service

ResilienceService is registered in DI container automatically:

```csharp
public class MyViewModel : ViewModelBase
{
    private readonly IResilienceService _resilience;

    public MyViewModel(IResilienceService resilience)
    {
        _resilience = resilience;
    }
}
```

### 2. Execute with Resilience

```csharp
// Simple retry pattern
var result = await _resilience.ExecuteWithRetryAsync(async ct =>
{
    return await httpClient.GetStringAsync("https://api.example.com/data", ct);
});

// Full resilience (timeout + retry + circuit breaker)
var data = await _resilience.ExecuteWithFullResilienceAsync(async ct =>
{
    return await _database.QueryAsync<User>("SELECT * FROM Users", ct);
});
```

---

## Resilience Patterns

### 1. Retry Pattern

**Purpose:** Automatically retry transient failures with exponential backoff.

**When to Use:**
- Network requests that fail temporarily
- Database deadlocks
- Rate-limited APIs
- Intermittent service availability

**Default Configuration:**
- 3 retry attempts
- 1 second initial delay
- 2x exponential backoff (1s → 2s → 4s)

#### Basic Usage

```csharp
public async Task<WeatherData> GetWeatherAsync()
{
    return await _resilience.ExecuteWithRetryAsync(async ct =>
    {
        var response = await _httpClient.GetAsync("https://api.weather.com/forecast", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<WeatherData>(ct);
    });
}
```

#### Custom Retry Count

```csharp
// Retry up to 5 times for critical operations
var userData = await _resilience.ExecuteWithRetryAsync(
    async ct => await _database.GetUserAsync(userId, ct),
    maxRetries: 5
);
```

#### How It Works

**Polly 8.x Lambda Pattern:**
```csharp
// Polly 8 requires lambda wrapper for CancellationToken
await _retryPipeline.ExecuteAsync(async ct =>
{
    return await YourOperationAsync(ct);
}, cancellationToken);
```

**Exponential Backoff Timing:**
- Attempt 1: Execute immediately
- Attempt 2: Wait 1 second
- Attempt 3: Wait 2 seconds (1s × 2)
- Attempt 4: Wait 4 seconds (2s × 2)

**Logging Output:**
```
[Warning] Retry attempt 1 after 00:00:01
[Warning] Retry attempt 2 after 00:00:02
[Warning] Retry attempt 3 after 00:00:04
```

---

### 2. Circuit Breaker Pattern

**Purpose:** Stop calling a failing service to prevent cascading failures and allow recovery time.

**When to Use:**
- External services that go down completely
- APIs with long outages
- Preventing resource exhaustion
- Protecting downstream services

**Default Configuration:**
- 50% failure ratio triggers break
- 10-second sampling window
- Minimum 3 requests before break
- 30-second break duration

#### Circuit Breaker States

```
┌─────────┐     3+ failures in 10s     ┌──────┐
│ Closed  │─────────────────────────────▶│ Open │
│(Normal) │                              │(Fail)│
└─────────┘                              └──────┘
     ▲                                        │
     │                                        │ 30 seconds
     │                                        │
     │         Success                        ▼
     │      ┌──────────┐                ┌─────────┐
     └──────│Half-Open │◀───────────────│ Waiting │
            │(Testing) │                └─────────┘
            └──────────┘
```

- **Closed (Normal):** Requests flow through normally
- **Open (Failing):** All requests fail immediately (BrokenCircuitException)
- **Half-Open (Testing):** After break duration, allows one test request
- **Waiting:** Circuit is open, waiting for break duration to expire

#### Basic Usage

```csharp
public async Task<string> CallExternalApiAsync()
{
    try
    {
        return await _resilience.ExecuteWithCircuitBreakerAsync(async ct =>
        {
            var response = await _httpClient.GetAsync("https://api.partner.com/data", ct);
            return await response.Content.ReadAsStringAsync(ct);
        });
    }
    catch (BrokenCircuitException)
    {
        // Circuit is open - service is down
        return "Service temporarily unavailable";
    }
}
```

#### Monitoring Circuit State

```csharp
public string GetServiceHealthStatus()
{
    var state = _resilience.GetCircuitBreakerState();

    return state switch
    {
        CircuitState.Closed => "✓ Service healthy",
        CircuitState.Open => "✗ Service down (circuit open)",
        CircuitState.HalfOpen => "⚠ Service testing (half-open)",
        CircuitState.Isolated => "⊗ Service isolated",
        _ => "Unknown"
    };
}
```

#### Manual Circuit Reset

```csharp
// Force reset circuit breaker after maintenance
_resilience.ResetCircuitBreaker();
_loggingService.LogInformation("Circuit breaker manually reset after service maintenance");
```

#### Real-World Example

```csharp
public class PaymentProcessor
{
    private readonly IResilienceService _resilience;

    public async Task<PaymentResult> ProcessPaymentAsync(Payment payment)
    {
        try
        {
            return await _resilience.ExecuteWithCircuitBreakerAsync(async ct =>
            {
                return await _paymentGateway.ChargeAsync(payment, ct);
            });
        }
        catch (BrokenCircuitException)
        {
            // Payment gateway is down - queue for later processing
            await _queue.EnqueueAsync(payment);
            return PaymentResult.Queued("Payment gateway temporarily unavailable");
        }
    }
}
```

---

### 3. Timeout Pattern

**Purpose:** Prevent operations from running indefinitely.

**When to Use:**
- Long-running HTTP requests
- Database queries that might hang
- File operations that could deadlock
- Any operation with performance SLAs

**Default Configuration:**
- 30 second timeout

#### Basic Usage

```csharp
public async Task<ReportData> GenerateReportAsync()
{
    try
    {
        return await _resilience.ExecuteWithTimeoutAsync(
            async ct => await _reportGenerator.CreateReportAsync(ct),
            timeout: TimeSpan.FromSeconds(10)
        );
    }
    catch (TimeoutRejectedException)
    {
        _loggingService.LogWarning("Report generation timed out after 10 seconds");
        return ReportData.Empty("Report generation took too long");
    }
}
```

#### Variable Timeouts

```csharp
public async Task<T> ExecuteWithDynamicTimeoutAsync<T>(
    Func<CancellationToken, Task<T>> operation,
    OperationPriority priority)
{
    var timeout = priority switch
    {
        OperationPriority.Critical => TimeSpan.FromSeconds(5),
        OperationPriority.High => TimeSpan.FromSeconds(30),
        OperationPriority.Normal => TimeSpan.FromMinutes(2),
        OperationPriority.Low => TimeSpan.FromMinutes(5),
        _ => TimeSpan.FromSeconds(30)
    };

    return await _resilience.ExecuteWithTimeoutAsync(operation, timeout);
}
```

#### Timeout with Progress

```csharp
public async Task<byte[]> DownloadLargeFileAsync(string url, IProgress<int> progress)
{
    try
    {
        return await _resilience.ExecuteWithTimeoutAsync(
            async ct =>
            {
                using var response = await _httpClient.GetAsync(url, ct);
                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                var buffer = new byte[8192];
                var bytesRead = 0L;

                using var stream = await response.Content.ReadAsStreamAsync(ct);
                using var memoryStream = new MemoryStream();

                int read;
                while ((read = await stream.ReadAsync(buffer, ct)) > 0)
                {
                    await memoryStream.WriteAsync(buffer.AsMemory(0, read), ct);
                    bytesRead += read;

                    if (totalBytes > 0)
                        progress.Report((int)(bytesRead * 100 / totalBytes));
                }

                return memoryStream.ToArray();
            },
            timeout: TimeSpan.FromMinutes(5)
        );
    }
    catch (TimeoutRejectedException)
    {
        throw new OperationCanceledException("File download timed out after 5 minutes");
    }
}
```

---

### 4. Full Resilience Pattern

**Purpose:** Combine timeout, retry, and circuit breaker for maximum protection.

**When to Use:**
- Critical business operations
- Production API calls
- Database operations in high-load scenarios
- Any operation where failure is unacceptable

**Configuration:**
- 30 second timeout (applied first)
- 3 retries with exponential backoff
- Circuit breaker protection (applied last)

#### Strategy Order

```
Request
   │
   ▼
┌──────────┐
│ Timeout  │ ─────▶ TimeoutRejectedException (if > 30s)
└──────────┘
   │
   ▼
┌──────────┐
│  Retry   │ ─────▶ Retries up to 3 times
└──────────┘
   │
   ▼
┌──────────┐
│ Circuit  │ ─────▶ BrokenCircuitException (if open)
│ Breaker  │
└──────────┘
   │
   ▼
Success or Final Exception
```

#### Basic Usage

```csharp
public async Task<Customer> GetCustomerAsync(int customerId)
{
    return await _resilience.ExecuteWithFullResilienceAsync(async ct =>
    {
        var response = await _httpClient.GetAsync($"https://api.crm.com/customers/{customerId}", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Customer>(ct);
    });
}
```

#### Production API Call

```csharp
public class OrderService
{
    private readonly IResilienceService _resilience;
    private readonly HttpClient _httpClient;

    public async Task<OrderConfirmation> SubmitOrderAsync(Order order)
    {
        return await _resilience.ExecuteWithFullResilienceAsync(async ct =>
        {
            // This operation is protected by:
            // 1. 30-second timeout
            // 2. 3 retries with exponential backoff
            // 3. Circuit breaker (opens after repeated failures)

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/orders")
            {
                Content = JsonContent.Create(order)
            };

            var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<OrderConfirmation>(ct);
        });
    }
}
```

#### Error Handling

```csharp
public async Task<Result<Invoice>> CreateInvoiceAsync(InvoiceRequest request)
{
    try
    {
        var invoice = await _resilience.ExecuteWithFullResilienceAsync(async ct =>
        {
            return await _invoiceApi.CreateAsync(request, ct);
        });

        return Result<Invoice>.Success(invoice);
    }
    catch (TimeoutRejectedException ex)
    {
        _loggingService.LogError("Invoice creation timed out", ex);
        return Result<Invoice>.Failure("Operation timed out");
    }
    catch (BrokenCircuitException ex)
    {
        _loggingService.LogError("Invoice service circuit is open", ex);
        return Result<Invoice>.Failure("Service temporarily unavailable");
    }
    catch (Exception ex)
    {
        _loggingService.LogError("Invoice creation failed after retries", ex);
        return Result<Invoice>.Failure("Failed to create invoice");
    }
}
```

---

## Configuration

### Retry Policy Configuration

```csharp
// Configure global retry settings
_resilience.ConfigureRetryPolicy(
    maxRetries: 5,
    delay: TimeSpan.FromSeconds(2),
    backoffMultiplier: 1.5
);

// Timing: 2s → 3s → 4.5s → 6.75s → 10.125s
```

**Parameters:**
- `maxRetries`: Maximum retry attempts (default: 3)
- `delay`: Initial delay between retries (default: 1 second)
- `backoffMultiplier`: Exponential backoff multiplier (default: 2.0)

### Circuit Breaker Configuration

```csharp
// Configure circuit breaker thresholds
_resilience.ConfigureCircuitBreaker(
    handledEventsBeforeBreaking: 5,  // Break after 5 failures
    durationOfBreak: TimeSpan.FromMinutes(1)  // Stay open for 1 minute
);
```

**Parameters:**
- `handledEventsBeforeBreaking`: Minimum throughput before circuit can break
- `durationOfBreak`: How long circuit stays open before testing

**Default Settings:**
```csharp
FailureRatio = 0.5,                     // Break at 50% failure rate
SamplingDuration = TimeSpan.FromSeconds(10),  // Evaluate over 10 seconds
MinimumThroughput = 3,                  // Need 3+ requests to break
BreakDuration = TimeSpan.FromSeconds(30)      // Stay open for 30 seconds
```

### Configuration Example

```csharp
public class ResilientApiClient
{
    private readonly IResilienceService _resilience;

    public ResilientApiClient(IResilienceService resilience)
    {
        _resilience = resilience;
        ConfigureForProduction();
    }

    private void ConfigureForProduction()
    {
        // More aggressive retry for production
        _resilience.ConfigureRetryPolicy(
            maxRetries: 5,
            delay: TimeSpan.FromMilliseconds(500),
            backoffMultiplier: 1.5
        );

        // Faster circuit breaking for better user experience
        _resilience.ConfigureCircuitBreaker(
            handledEventsBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromSeconds(20)
        );
    }
}
```

---

## Extension Methods

### HTTP Resilience Extension

```csharp
using WPFBase.Services;

public class WeatherService
{
    private readonly IResilienceService _resilience;
    private readonly HttpClient _httpClient;

    public async Task<WeatherForecast> GetForecastAsync(string city)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/forecast?city={city}");

        // Automatic resilience for HTTP requests
        var response = await _resilience.ExecuteHttpWithResilienceAsync(
            _httpClient,
            request
        );

        return await response.Content.ReadFromJsonAsync<WeatherForecast>();
    }
}
```

**What it provides:**
- Full resilience (timeout + retry + circuit breaker)
- HTTP-specific error handling
- Automatic response disposal

### Database Resilience Extension

```csharp
public class UserRepository
{
    private readonly IResilienceService _resilience;
    private readonly IDbConnection _connection;

    public async Task<List<User>> GetActiveUsersAsync()
    {
        // Automatic resilience for database operations
        return await _resilience.ExecuteDatabaseOperationAsync(async () =>
        {
            return await _connection.QueryAsync<User>(
                "SELECT * FROM Users WHERE IsActive = 1"
            );
        });
    }
}
```

**What it provides:**
- 5 retry attempts (databases benefit from more retries)
- Exponential backoff for deadlock resolution
- No circuit breaker (databases typically recover quickly)

### Creating Custom Extensions

```csharp
public static class CustomResilienceExtensions
{
    /// <summary>
    /// Execute file operation with resilience (handles file locks)
    /// </summary>
    public static async Task<T> ExecuteFileOperationAsync<T>(
        this IResilienceService resilience,
        Func<Task<T>> fileOperation)
    {
        return await resilience.ExecuteWithRetryAsync(
            async ct => await fileOperation(),
            maxRetries: 10  // Files may be locked temporarily
        );
    }

    /// <summary>
    /// Execute with rate limiting consideration
    /// </summary>
    public static async Task<T> ExecuteWithRateLimitAsync<T>(
        this IResilienceService resilience,
        Func<CancellationToken, Task<T>> operation)
    {
        return await resilience.ExecuteWithRetryAsync(
            operation,
            maxRetries: 5  // APIs with rate limits need retries
        );
    }
}
```

**Usage:**
```csharp
// File operations
var content = await _resilience.ExecuteFileOperationAsync(async () =>
{
    return await File.ReadAllTextAsync("locked-file.txt");
});

// Rate-limited APIs
var data = await _resilience.ExecuteWithRateLimitAsync(async ct =>
{
    return await _twitterApi.GetTweetsAsync(username, ct);
});
```

---

## Best Practices

### When to Use Each Pattern

| Pattern | Use Case | Example |
|---------|----------|---------|
| **Retry Only** | Transient failures, quick recovery | Database deadlocks, temporary network issues |
| **Circuit Breaker Only** | Prevent wasted calls to failing service | External API completely down |
| **Timeout Only** | Prevent hanging operations | Long-running queries, slow APIs |
| **Full Resilience** | Production critical operations | Payment processing, order submission |

### Pattern Selection Guide

```csharp
// Quick, transient failures → Retry
public async Task<User> GetUserAsync(int id)
{
    return await _resilience.ExecuteWithRetryAsync(async ct =>
        await _cache.GetOrCreateAsync($"user:{id}", ct));
}

// Service that goes down completely → Circuit Breaker
public async Task<string> CallLegacySystemAsync()
{
    return await _resilience.ExecuteWithCircuitBreakerAsync(async ct =>
        await _legacyApi.GetDataAsync(ct));
}

// Unpredictable operation duration → Timeout
public async Task<Report> GenerateLargeReportAsync()
{
    return await _resilience.ExecuteWithTimeoutAsync(
        async ct => await _reportEngine.GenerateAsync(ct),
        timeout: TimeSpan.FromMinutes(5));
}

// Production critical operation → Full Resilience
public async Task<PaymentResult> ProcessPaymentAsync(Payment payment)
{
    return await _resilience.ExecuteWithFullResilienceAsync(async ct =>
        await _paymentGateway.ChargeAsync(payment, ct));
}
```

### Common Mistakes to Avoid

#### 1. Not Passing CancellationToken

```csharp
// ❌ BAD - Ignores cancellation
await _resilience.ExecuteWithRetryAsync(async ct =>
{
    return await _httpClient.GetStringAsync(url);  // Doesn't use ct!
});

// ✓ GOOD - Respects cancellation
await _resilience.ExecuteWithRetryAsync(async ct =>
{
    return await _httpClient.GetStringAsync(url, ct);
});
```

#### 2. Using Retry on Non-Transient Failures

```csharp
// ❌ BAD - Retrying permanent errors
await _resilience.ExecuteWithRetryAsync(async ct =>
{
    return await _api.GetAsync("/endpoint-that-doesnt-exist", ct);
});
// This will retry 3 times on 404 Not Found - waste of time!

// ✓ GOOD - Check error type before retrying
await _resilience.ExecuteWithRetryAsync(async ct =>
{
    var response = await _api.GetAsync("/users", ct);

    // Only throw on transient errors (5xx)
    if ((int)response.StatusCode >= 500)
        response.EnsureSuccessStatusCode();

    return await response.Content.ReadFromJsonAsync<User[]>(ct);
});
```

#### 3. Ignoring Circuit Breaker State

```csharp
// ❌ BAD - No state checking
public async Task ProcessBatch(List<Order> orders)
{
    foreach (var order in orders)
    {
        try
        {
            await _resilience.ExecuteWithCircuitBreakerAsync(async ct =>
                await _api.SubmitOrderAsync(order, ct));
        }
        catch (BrokenCircuitException)
        {
            // Continues trying all orders even though circuit is open!
        }
    }
}

// ✓ GOOD - Check state first
public async Task ProcessBatch(List<Order> orders)
{
    if (_resilience.GetCircuitBreakerState() == CircuitState.Open)
    {
        _logger.LogWarning("Circuit is open - skipping batch");
        return;
    }

    foreach (var order in orders)
    {
        await _resilience.ExecuteWithCircuitBreakerAsync(async ct =>
            await _api.SubmitOrderAsync(order, ct));
    }
}
```

#### 4. Timeout Too Short

```csharp
// ❌ BAD - Unrealistic timeout
await _resilience.ExecuteWithTimeoutAsync(
    async ct => await _database.ExecuteLongRunningQuery(ct),
    timeout: TimeSpan.FromMilliseconds(100)  // Too short!
);

// ✓ GOOD - Realistic timeout with buffer
await _resilience.ExecuteWithTimeoutAsync(
    async ct => await _database.ExecuteLongRunningQuery(ct),
    timeout: TimeSpan.FromSeconds(30)  // Reasonable for complex query
);
```

### Performance Considerations

#### 1. Avoid Nested Resilience

```csharp
// ❌ BAD - Double resilience
public async Task<Data> GetDataAsync()
{
    return await _resilience.ExecuteWithRetryAsync(async ct =>
    {
        // This already has full resilience!
        return await _resilience.ExecuteWithFullResilienceAsync(async ct2 =>
            await _api.GetDataAsync(ct2));
    });
}
// Result: 3 retries × 3 retries = 9 total attempts!

// ✓ GOOD - Single resilience layer
public async Task<Data> GetDataAsync()
{
    return await _resilience.ExecuteWithFullResilienceAsync(async ct =>
        await _api.GetDataAsync(ct));
}
```

#### 2. Circuit Breaker for Batches

```csharp
// ✓ GOOD - Single circuit breaker for batch
public async Task<List<Result>> ProcessBatchAsync(List<Item> items)
{
    var results = new List<Result>();

    foreach (var item in items)
    {
        try
        {
            var result = await _resilience.ExecuteWithCircuitBreakerAsync(async ct =>
                await ProcessItemAsync(item, ct));

            results.Add(result);
        }
        catch (BrokenCircuitException)
        {
            // Circuit is open - fail fast for remaining items
            results.AddRange(items.Skip(results.Count)
                .Select(i => Result.Failure("Service unavailable")));
            break;
        }
    }

    return results;
}
```

---

## Claude Code Examples

### Generate Resilient HTTP Service

**Prompt:**
```
Create a WeatherService class that calls https://api.weather.com/forecast with full resilience.
Use IResilienceService with proper error handling and logging.
```

**Expected Output:**
```csharp
public class WeatherService
{
    private readonly IResilienceService _resilience;
    private readonly HttpClient _httpClient;
    private readonly ILoggingService _logger;

    public WeatherService(
        IResilienceService resilience,
        HttpClient httpClient,
        ILoggingService logger)
    {
        _resilience = resilience;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<WeatherForecast> GetForecastAsync(string city)
    {
        try
        {
            return await _resilience.ExecuteWithFullResilienceAsync(async ct =>
            {
                var response = await _httpClient.GetAsync($"/forecast?city={city}", ct);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<WeatherForecast>(ct);
            });
        }
        catch (TimeoutRejectedException)
        {
            _logger.LogWarning($"Weather API timeout for city: {city}");
            return WeatherForecast.Default;
        }
        catch (BrokenCircuitException)
        {
            _logger.LogError("Weather API circuit breaker open");
            throw new ServiceUnavailableException("Weather service temporarily unavailable");
        }
    }
}
```

### Generate Resilient Database Repository

**Prompt:**
```
Create a UserRepository with resilient database queries using IResilienceService.
Include retry logic for deadlocks and proper cancellation token handling.
```

**Expected Output:**
```csharp
public class UserRepository
{
    private readonly IResilienceService _resilience;
    private readonly IDbConnection _connection;

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _resilience.ExecuteWithRetryAsync(async ct =>
        {
            return await _connection.QuerySingleOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Id = @Id",
                new { Id = id });
        }, maxRetries: 5, cancellationToken);
    }

    public async Task<List<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _resilience.ExecuteDatabaseOperationAsync(async () =>
        {
            var users = await _connection.QueryAsync<User>(
                "SELECT * FROM Users WHERE IsActive = 1");
            return users.ToList();
        }, cancellationToken);
    }
}
```

### Generate Circuit Breaker Monitoring

**Prompt:**
```
Create a ServiceHealthMonitor that tracks circuit breaker state and logs health status every 30 seconds.
Include methods to get current health and reset circuits manually.
```

**Expected Output:**
```csharp
public class ServiceHealthMonitor
{
    private readonly IResilienceService _resilience;
    private readonly ILoggingService _logger;
    private readonly Timer _healthCheckTimer;

    public ServiceHealthMonitor(IResilienceService resilience, ILoggingService logger)
    {
        _resilience = resilience;
        _logger = logger;
        _healthCheckTimer = new Timer(CheckHealth, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    private void CheckHealth(object? state)
    {
        var circuitState = _resilience.GetCircuitBreakerState();

        switch (circuitState)
        {
            case CircuitState.Closed:
                _logger.LogInformation("Circuit breaker: Healthy");
                break;
            case CircuitState.Open:
                _logger.LogWarning("Circuit breaker: Open (service degraded)");
                break;
            case CircuitState.HalfOpen:
                _logger.LogInformation("Circuit breaker: Half-open (testing recovery)");
                break;
        }
    }

    public ServiceHealth GetCurrentHealth()
    {
        var state = _resilience.GetCircuitBreakerState();
        return new ServiceHealth
        {
            IsHealthy = state == CircuitState.Closed,
            Status = state.ToString(),
            Timestamp = DateTime.UtcNow
        };
    }

    public void ForceReset()
    {
        _resilience.ResetCircuitBreaker();
        _logger.LogInformation("Circuit breaker manually reset");
    }
}
```

### Generate Resilient File Operations

**Prompt:**
```
Create a FileService that reads and writes files with resilience for file locks and IO errors.
Use custom retry policy with 10 attempts for locked files.
```

**Expected Output:**
```csharp
public class FileService
{
    private readonly IResilienceService _resilience;
    private readonly ILoggingService _logger;

    public async Task<string> ReadFileWithResilienceAsync(string filePath)
    {
        return await _resilience.ExecuteWithRetryAsync(async ct =>
        {
            return await File.ReadAllTextAsync(filePath, ct);
        }, maxRetries: 10);  // Files may be locked temporarily
    }

    public async Task WriteFileWithResilienceAsync(string filePath, string content)
    {
        await _resilience.ExecuteWithRetryAsync(async ct =>
        {
            await File.WriteAllTextAsync(filePath, content, ct);
            return true;  // Polly requires return value
        }, maxRetries: 10);
    }

    public async Task<T> ReadJsonFileAsync<T>(string filePath)
    {
        return await _resilience.ExecuteWithRetryAsync(async ct =>
        {
            var json = await File.ReadAllTextAsync(filePath, ct);
            return JsonSerializer.Deserialize<T>(json)
                ?? throw new InvalidDataException("Failed to deserialize JSON");
        }, maxRetries: 5);
    }
}
```

---

## Troubleshooting

### Issue: Retries Not Working

**Symptom:** Operation fails immediately without retrying.

**Causes:**
1. Exception type not handled by Polly
2. CancellationToken already cancelled
3. Operation doesn't throw exceptions

**Solutions:**
```csharp
// Ensure operation throws exceptions
await _resilience.ExecuteWithRetryAsync(async ct =>
{
    var response = await _httpClient.GetAsync(url, ct);
    response.EnsureSuccessStatusCode();  // ← Throws on error
    return await response.Content.ReadAsStringAsync(ct);
});

// Check cancellation token
var cts = new CancellationTokenSource();
await _resilience.ExecuteWithRetryAsync(
    async ct => await _operation.ExecuteAsync(ct),
    cancellationToken: cts.Token  // ← Pass valid token
);
```

### Issue: Circuit Breaker Always Open

**Symptom:** BrokenCircuitException thrown immediately.

**Causes:**
1. Too many recent failures
2. Circuit break duration not expired
3. MinimumThroughput too low

**Solutions:**
```csharp
// Check circuit state before calling
var state = _resilience.GetCircuitBreakerState();
if (state == CircuitState.Open)
{
    _logger.LogWarning("Circuit is open - waiting for recovery");
    return CachedResult;
}

// Manually reset if needed
_resilience.ResetCircuitBreaker();

// Adjust configuration
_resilience.ConfigureCircuitBreaker(
    handledEventsBeforeBreaking: 5,  // Increase threshold
    durationOfBreak: TimeSpan.FromSeconds(10)  // Reduce break time
);
```

### Issue: Timeout Too Aggressive

**Symptom:** TimeoutRejectedException on operations that should succeed.

**Causes:**
1. Timeout shorter than typical operation duration
2. Network latency not accounted for
3. Complex operations need more time

**Solutions:**
```csharp
// Increase timeout for long operations
await _resilience.ExecuteWithTimeoutAsync(
    async ct => await _complexOperation.ExecuteAsync(ct),
    timeout: TimeSpan.FromMinutes(5)  // ← More realistic
);

// Use different timeouts per environment
var timeout = _environment.IsProduction
    ? TimeSpan.FromSeconds(30)
    : TimeSpan.FromMinutes(5);  // Dev allows debugging

await _resilience.ExecuteWithTimeoutAsync(operation, timeout);
```

### Issue: Too Many Retries

**Symptom:** Operation takes too long due to excessive retries.

**Causes:**
1. Retrying non-transient errors
2. Too many retry attempts configured
3. No timeout combined with retries

**Solutions:**
```csharp
// Reduce retries for non-critical operations
await _resilience.ExecuteWithRetryAsync(
    async ct => await _api.GetDataAsync(ct),
    maxRetries: 2  // ← Faster failure
);

// Combine with timeout to prevent infinite waiting
await _resilience.ExecuteWithFullResilienceAsync(async ct =>
{
    // Max 30 seconds total (timeout)
    // Max 3 retries with exponential backoff
    return await _api.CallAsync(ct);
});

// Don't retry permanent errors
await _resilience.ExecuteWithRetryAsync(async ct =>
{
    var response = await _httpClient.GetAsync(url, ct);

    // Only retry transient errors (5xx)
    if ((int)response.StatusCode >= 500)
        response.EnsureSuccessStatusCode();

    return response;
});
```

### Issue: Memory Leaks with HttpClient

**Symptom:** Application memory grows over time.

**Cause:** Creating new HttpClient instances inside resilience operations.

**Solution:**
```csharp
// ❌ BAD - Creates new HttpClient each retry
await _resilience.ExecuteWithRetryAsync(async ct =>
{
    using var client = new HttpClient();  // ← LEAK!
    return await client.GetStringAsync(url);
});

// ✓ GOOD - Reuse HttpClient from DI
public class MyService
{
    private readonly HttpClient _httpClient;
    private readonly IResilienceService _resilience;

    public MyService(HttpClient httpClient, IResilienceService resilience)
    {
        _httpClient = httpClient;  // ← Injected, reused
        _resilience = resilience;
    }

    public async Task<string> GetDataAsync()
    {
        return await _resilience.ExecuteWithRetryAsync(async ct =>
            await _httpClient.GetStringAsync(url, ct));
    }
}
```

### Common Error Messages

| Error | Meaning | Solution |
|-------|---------|----------|
| `BrokenCircuitException` | Circuit breaker is open | Wait for break duration or reset manually |
| `TimeoutRejectedException` | Operation exceeded timeout | Increase timeout or optimize operation |
| `TaskCanceledException` | CancellationToken was cancelled | Check cancellation token source |
| `HttpRequestException` | Network/HTTP error (retryable) | Already handled by retry policy |

---

## Advanced Patterns

### Fallback with Retry

```csharp
public async Task<WeatherData> GetWeatherWithFallbackAsync(string city)
{
    try
    {
        // Try primary API with resilience
        return await _resilience.ExecuteWithFullResilienceAsync(async ct =>
            await _primaryApi.GetWeatherAsync(city, ct));
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Primary API failed, trying fallback");

        // Fallback to secondary API
        return await _resilience.ExecuteWithRetryAsync(async ct =>
            await _fallbackApi.GetWeatherAsync(city, ct));
    }
}
```

### Conditional Resilience

```csharp
public async Task<T> ExecuteWithAdaptiveResilienceAsync<T>(
    Func<CancellationToken, Task<T>> operation,
    OperationContext context)
{
    // Choose resilience strategy based on context
    if (context.IsCritical)
    {
        // Full protection for critical operations
        return await _resilience.ExecuteWithFullResilienceAsync(operation);
    }
    else if (context.IsRetryable)
    {
        // Just retry for simple transient failures
        return await _resilience.ExecuteWithRetryAsync(operation, maxRetries: 2);
    }
    else
    {
        // No resilience for non-critical, non-retryable operations
        return await operation(CancellationToken.None);
    }
}
```

### Bulk Operations with Circuit Breaker

```csharp
public async Task<BatchResult> ProcessBatchAsync(List<Order> orders)
{
    var results = new BatchResult();

    foreach (var order in orders)
    {
        // Check circuit before each operation
        if (_resilience.GetCircuitBreakerState() == CircuitState.Open)
        {
            results.Failed.AddRange(orders.Skip(results.Processed));
            _logger.LogWarning($"Circuit open - skipped {results.Failed.Count} orders");
            break;
        }

        try
        {
            var result = await _resilience.ExecuteWithCircuitBreakerAsync(async ct =>
                await _api.ProcessOrderAsync(order, ct));

            results.Succeeded.Add(result);
        }
        catch (BrokenCircuitException)
        {
            results.Failed.Add(order);
            break;  // Circuit just opened - stop processing
        }
        catch (Exception ex)
        {
            results.Failed.Add(order);
            _logger.LogError(ex, $"Failed to process order {order.Id}");
        }

        results.Processed++;
    }

    return results;
}
```

---

## Summary

**ResilienceService** provides production-ready resilience patterns for WPF applications:

- **Retry Pattern**: Automatic retry with exponential backoff (3 attempts, 1s → 2s → 4s)
- **Circuit Breaker**: Prevent cascading failures (50% threshold, 30s break)
- **Timeout Pattern**: Enforce operation limits (30s default)
- **Full Resilience**: Combined protection for critical operations

**Key Benefits:**
- Polly 8.5.0 modern async patterns
- Configurable retry and circuit breaker policies
- Extension methods for HTTP and database operations
- Production-tested error handling
- Claude Code integration ready

**Quick Reference:**
```csharp
// Retry transient failures
await _resilience.ExecuteWithRetryAsync(operation);

// Protect against failing services
await _resilience.ExecuteWithCircuitBreakerAsync(operation);

// Enforce time limits
await _resilience.ExecuteWithTimeoutAsync(operation, TimeSpan.FromSeconds(30));

// Full protection
await _resilience.ExecuteWithFullResilienceAsync(operation);
```

**Next Steps:**
- Review `Services/ResilienceService.cs` for implementation details
- Check `Interfaces/IResilienceService.cs` for API reference
- See Claude Code examples for generation prompts
- Configure policies for your specific use case