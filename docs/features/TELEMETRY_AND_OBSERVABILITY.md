# Telemetry and Observability

## Overview

WPFBase includes a modern telemetry system built on **OpenTelemetry 1.9.0** with **Aspire Dashboard integration**, providing comprehensive observability for WPF applications with minimal performance impact (less than 0.5% overhead).

**Key Features:**
- OpenTelemetry 1.9.0 distributed tracing
- Real-time metrics collection
- Aspire Dashboard visualization (free, no account required)
- WPF-specific telemetry (ViewModel, commands, navigation)
- Performance monitoring with automatic threshold detection
- Exception tracking with context
- Zero-cost abstraction when disabled

**Perfect For:**
- Performance monitoring and optimization
- Understanding user behavior patterns
- Debugging production issues
- Capacity planning and scaling decisions
- Identifying UI thread blocking
- Memory leak detection

---

## OpenTelemetry 1.9.0

### What is OpenTelemetry?

OpenTelemetry is the industry-standard framework for observability, providing:

1. **Traces (Distributed Tracing)**
   - Track operations across your application
   - Understand request flow and timing
   - Identify bottlenecks and dependencies
   - Parent-child relationship tracking

2. **Metrics (Time-Series Data)**
   - Counters: Total command executions, exceptions
   - Histograms: Duration distributions (P50, P90, P99)
   - Runtime metrics: Memory, CPU, GC stats
   - Custom business metrics

3. **Context Propagation**
   - Automatic correlation of related operations
   - Global properties (user ID, session ID)
   - Distributed context across async operations

### Why OpenTelemetry 1.9.0?

- **Vendor-neutral**: Works with any backend (Aspire, Jaeger, Prometheus, Azure Monitor)
- **Production-ready**: Used by major companies at scale
- **Minimal overhead**: ~0.3-0.5% performance impact
- **Semantic conventions**: Standardized naming and attributes
- **Future-proof**: CNCF graduated project with long-term support

---

## Aspire Dashboard

### What is Aspire Dashboard?

Aspire Dashboard is Microsoft's free, open-source observability UI designed for .NET applications. It provides real-time visualization of traces and metrics without requiring cloud services or accounts.

### Setup (One Command)

```bash
# Start Aspire Dashboard using Docker
docker run --rm -it -p 18888:18888 -p 4317:4317 mcr.microsoft.com/dotnet/aspire-dashboard:9.5
```

**What this does:**
- **Port 18888**: Dashboard web UI (http://localhost:18888)
- **Port 4317**: OTLP gRPC endpoint for telemetry data
- **No account required**: Completely free and local
- **Real-time updates**: See telemetry as it happens

### Dashboard UI Features

**1. Traces View**
- See all operations with timing breakdown
- Filter by operation name, status, duration
- Drill into individual traces to see span hierarchy
- View tags/attributes for detailed context

**2. Metrics View**
- Real-time charts of all metrics
- Histograms show P50/P90/P99 percentiles
- Counters show totals and rates
- Filter by metric name and dimensions

**3. Logs View** (if configured)
- Correlated with traces via trace ID
- Filter by log level and message
- See logs in context of operations

### Dashboard Benefits

- **Zero setup**: No account, no configuration, just run Docker
- **Real-time**: See telemetry within seconds
- **Free**: No cost, no data limits for local development
- **Production-capable**: Can export to any OTLP-compatible backend

---

## Activity Tracing

### What are Activities?

Activities (also called Spans in OpenTelemetry) represent a unit of work in your application. They track:
- Operation name and timing
- Success/failure status
- Custom tags/attributes
- Parent-child relationships
- Exception details

### StartActivity Pattern

```csharp
// In your ViewModel
public async Task LoadDataAsync()
{
    // Start an activity for this operation
    using var activity = _telemetry.StartActivity("LoadData");
    activity?.SetTag("data.source", "Database");

    try
    {
        var stopwatch = Stopwatch.StartNew();

        // Perform your operation
        var data = await _repository.GetDataAsync();

        stopwatch.Stop();
        activity?.SetTag("data.count", data.Count);
        activity?.SetTag("duration_ms", stopwatch.ElapsedMilliseconds);
        activity?.SetStatus(ActivityStatusCode.Ok);

        return data;
    }
    catch (Exception ex)
    {
        // Automatically records exception with context
        _telemetry.RecordException(ex, activity);
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        throw;
    }
}
```

### Activity Hierarchy

Activities automatically create parent-child relationships:

```csharp
// Parent operation
using var parentActivity = _telemetry.StartActivity("SaveWorkflow");

// Child operations (automatically linked to parent)
using (var childActivity = _telemetry.StartActivity("ValidateData"))
{
    // Validation logic
}

using (var childActivity = _telemetry.StartActivity("SaveToDatabase"))
{
    // Database logic
}

// Dashboard shows:
// SaveWorkflow (500ms)
//   ├─ ValidateData (50ms)
//   └─ SaveToDatabase (450ms)
```

### Activity Best Practices

1. **Meaningful names**: Use descriptive operation names
2. **Add context**: Set tags for important parameters
3. **Track success**: Always set status (Ok/Error)
4. **Use using statements**: Ensures proper disposal
5. **Don't over-instrument**: Focus on key operations (>10ms)

---

## Metrics

### Built-in Metrics

WPFBase includes comprehensive WPF-specific metrics:

#### 1. Command Metrics

```csharp
// Metric: wpfbase_command_executions_total
// Type: Counter
// Dimensions: command.name, command.success
// Tracks: Total command executions and success rate

// Metric: wpfbase_command_duration_ms
// Type: Histogram
// Dimensions: command.name, command.success
// Tracks: Command execution duration distribution
```

**Usage:**
```csharp
public async Task ExecuteAsync()
{
    var stopwatch = Stopwatch.StartNew();
    bool success = false;

    try
    {
        await DoWorkAsync();
        success = true;
    }
    finally
    {
        stopwatch.Stop();
        _telemetry.TrackCommandExecution(
            nameof(MyCommand),
            stopwatch.Elapsed,
            success
        );
    }
}
```

#### 2. ViewModel Metrics

```csharp
// Metric: wpfbase_viewmodel_init_duration_ms
// Type: Histogram
// Dimensions: viewmodel.name
// Tracks: ViewModel initialization time

// Usage in constructor:
public MyViewModel(ITelemetryService telemetry)
{
    var stopwatch = Stopwatch.StartNew();

    // Initialization code
    InitializeData();

    stopwatch.Stop();
    telemetry.TrackViewModelInitialization(
        nameof(MyViewModel),
        stopwatch.Elapsed
    );
}
```

#### 3. Navigation Metrics

```csharp
// Metric: wpfbase_navigation_duration_ms
// Type: Histogram
// Dimensions: navigation.from, navigation.to
// Tracks: Navigation timing between views

_telemetry.TrackNavigation(
    fromView: "HomeView",
    toView: "SettingsView",
    duration: navigationTime
);
```

#### 4. Exception Metrics

```csharp
// Metric: wpfbase_exceptions_total
// Type: Counter
// Dimensions: exception.type, exception.message
// Tracks: Total exceptions by type

_telemetry.RecordException(exception);
```

#### 5. UI Thread Blocking

```csharp
// Metric: wpfbase_ui_blocking_duration_ms
// Type: Histogram
// Tracks: UI thread blocks >100ms (automatic severity detection)

// Only triggers for significant blocks (>100ms)
_telemetry.TrackUIThreadBlocking(blockDuration);

// Sets severity:
// - Medium: 100-1000ms
// - High: >1000ms
```

#### 6. Memory Metrics

```csharp
// Metrics: wpfbase_memory_working_set_mb, wpfbase_memory_managed_mb
// Type: Histogram
// Tracks: Memory pressure and GC stats

var workingSet = Environment.WorkingSet;
var managedMemory = GC.GetTotalMemory(false);

_telemetry.TrackMemoryPressure(workingSet, managedMemory);
```

#### 7. User Action Metrics

```csharp
// Metric: wpfbase_user_actions_total
// Type: Counter
// Dimensions: action.name, action.target
// Tracks: User interactions for analytics

_telemetry.TrackUserAction(
    action: "Click",
    target: "SaveButton",
    properties: new Dictionary<string, string>
    {
        ["page"] = "Settings",
        ["section"] = "Profile"
    }
);
```

### Metric Types Explained

**Counter**: Monotonically increasing value (total count)
- Example: Total commands executed, total exceptions
- Use for: Counts that only go up

**Histogram**: Distribution of values over time
- Example: Command duration (shows P50, P90, P99)
- Use for: Duration, size, percentiles

**Gauge** (not shown): Current value that can go up or down
- Example: Active connections, queue length
- Use for: Current state

---

## Custom Metrics

### Creating Custom Counters

```csharp
// Increment a counter
_telemetry.IncrementCounter(
    name: "myapp_feature_usage_total",
    tags: new Dictionary<string, object?>
    {
        ["feature"] = "ExportData",
        ["format"] = "PDF"
    }
);
```

### Creating Custom Histograms

```csharp
// Record a value in a histogram
_telemetry.RecordMetric(
    name: "myapp_export_file_size_bytes",
    value: fileSize,
    tags: new Dictionary<string, object?>
    {
        ["format"] = "PDF",
        ["quality"] = "High"
    }
);
```

### Recording Duration

```csharp
// Record operation duration
var stopwatch = Stopwatch.StartNew();
await PerformOperationAsync();
stopwatch.Stop();

_telemetry.RecordDuration(
    name: "myapp_custom_operation_duration",
    duration: stopwatch.Elapsed,
    tags: new Dictionary<string, object?>
    {
        ["operation_type"] = "DataProcessing"
    }
);
```

### Naming Conventions

Follow OpenTelemetry semantic conventions:

```
{namespace}_{metric_name}_{unit}

Examples:
- wpfbase_command_duration_ms
- myapp_export_count_total
- myapp_cache_size_bytes
- myapp_queue_length_items
```

**Best Practices:**
- Use lowercase with underscores
- Include unit suffix (_ms, _bytes, _total)
- Add namespace prefix (wpfbase_, myapp_)
- Use descriptive but concise names

---

## Performance Tracking

### TrackAsync Helper

The `TrackAsync` method automatically creates activities, measures duration, and handles exceptions:

```csharp
// For operations that return a value
public async Task<List<Item>> LoadItemsAsync()
{
    return await _telemetry.TrackAsync(
        operationName: "LoadItems",
        operation: async () =>
        {
            var items = await _repository.GetItemsAsync();
            return items;
        },
        tags: new Dictionary<string, object?>
        {
            ["source"] = "Database"
        }
    );
}

// For void operations
public async Task SaveChangesAsync()
{
    await _telemetry.TrackAsync(
        operationName: "SaveChanges",
        operation: async () =>
        {
            await _repository.SaveAsync();
        },
        tags: new Dictionary<string, object?>
        {
            ["entity_count"] = _changeTracker.Count
        }
    );
}
```

**What TrackAsync does automatically:**
- Creates activity with operation name
- Measures elapsed time
- Sets success/failure tags
- Records exceptions with context
- Adds custom tags

### Scoped Telemetry

Use scopes to group related operations:

```csharp
public async Task ProcessBatchAsync(List<Item> items)
{
    using var scope = _telemetry.CreateScope(
        scopeName: "BatchProcessing",
        properties: new Dictionary<string, object?>
        {
            ["batch_size"] = items.Count
        }
    );

    try
    {
        foreach (var item in items)
        {
            await ProcessItemAsync(item);
        }

        scope.MarkSuccess();
    }
    catch (Exception ex)
    {
        scope.MarkFailure(ex);
        throw;
    }
}
```

**Scope benefits:**
- Groups related operations
- Automatic timing
- Success/failure tracking
- Proper resource disposal

---

## Configuration

### appsettings.json

```json
{
  "Telemetry": {
    "Enabled": true,
    "Level": "Information",
    "ServiceName": "WPFBase",
    "OtlpEndpoint": "http://localhost:4317",
    "AspireDashboard": "http://localhost:18888",
    "SamplingRatio": 0.1,
    "EnableRuntimeMetrics": true,
    "CustomMetrics": {
      "TrackViewModels": true,
      "TrackCommands": true,
      "TrackNavigation": true,
      "TrackUserActions": true,
      "TrackPerformance": true
    }
  }
}
```

### Configuration Options

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Enabled` | bool | false | Enable/disable all telemetry |
| `Level` | enum | Information | Telemetry detail level (None, Critical, Error, Warning, Information, Verbose) |
| `ServiceName` | string | "WPFBase" | Service identifier for dashboard |
| `OtlpEndpoint` | string | "http://localhost:4317" | OTLP gRPC endpoint |
| `AspireDashboard` | string | "http://localhost:18888" | Dashboard UI URL |
| `SamplingRatio` | double | 0.1 | Sample 10% of traces (reduces overhead) |
| `EnableRuntimeMetrics` | bool | true | Collect .NET runtime metrics |

### Telemetry Levels

```csharp
public enum TelemetryLevel
{
    None = 0,           // No telemetry
    Critical = 1,       // Only critical errors
    Error = 2,          // Errors and exceptions
    Warning = 3,        // Warnings, errors, performance issues
    Information = 4,    // General info (recommended)
    Verbose = 5         // Detailed debugging (high volume)
}
```

**Level Impact:**
- **None**: Zero overhead, completely disabled
- **Critical**: <0.1% overhead, only critical failures
- **Information**: ~0.3% overhead, balanced (recommended)
- **Verbose**: ~0.5% overhead, includes data binding (>10ms)

### Sampling

Sampling reduces overhead by only recording a percentage of traces:

```json
"SamplingRatio": 0.1  // Record 10% of traces
```

**When to sample:**
- Production: 1-10% (0.01-0.1)
- Staging: 50% (0.5)
- Development: 100% (1.0)

**Note:** Metrics are always recorded (not sampled).

---

## Dashboard Visualization

### Traces View

**What you see:**
- List of all operations with timing
- Success/failure status (color-coded)
- Operation hierarchy (parent-child)
- Drill-down to see detailed spans

**Example trace breakdown:**
```
Command.SaveData (543ms) ✓
├─ ValidateInput (23ms) ✓
├─ Database.SaveChanges (487ms) ✓
│  ├─ Transaction.Begin (5ms) ✓
│  ├─ Query.Execute (450ms) ✓
│  └─ Transaction.Commit (32ms) ✓
└─ Cache.Invalidate (12ms) ✓
```

**Filtering:**
- By operation name: "Command.*", "ViewModel.*"
- By duration: ">500ms", "<100ms"
- By status: Success, Error
- By time range: Last hour, Last 24h

### Metrics View

**What you see:**
- Real-time charts of all metrics
- Histogram percentiles (P50, P90, P99, P99.9)
- Counter rates (per second, per minute)
- Memory and runtime metrics

**Example charts:**
- Command duration histogram: P50=50ms, P90=200ms, P99=500ms
- Exceptions over time: 3 exceptions in last hour
- Memory usage: 150MB working set, 80MB managed

**Key insights:**
- **P50 (median)**: Typical user experience
- **P90**: 90% of users see this or better
- **P99**: Worst 1% of experiences (outliers)
- **Spikes**: Identify performance regressions

### Logs View

If you configure structured logging with OpenTelemetry:

```csharp
// Logs are automatically correlated with traces
_logger.LogInformation("Processing {Count} items", items.Count);
```

Dashboard shows logs alongside their traces for complete context.

---

## Integration Examples

### ViewModel Telemetry

```csharp
public class MyViewModel : ViewModelBase
{
    private readonly ITelemetryService _telemetry;

    public MyViewModel(ITelemetryService telemetry)
    {
        _telemetry = telemetry;

        // Track initialization
        var stopwatch = Stopwatch.StartNew();
        InitializeData();
        stopwatch.Stop();

        _telemetry.TrackViewModelInitialization(
            nameof(MyViewModel),
            stopwatch.Elapsed
        );
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        // Track command execution
        using var activity = _telemetry.StartActivity("Command.Save");
        activity?.SetTag("viewmodel", nameof(MyViewModel));

        var stopwatch = Stopwatch.StartNew();
        bool success = false;

        try
        {
            await SaveDataAsync();
            success = true;
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            _telemetry.RecordException(ex, activity);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _telemetry.TrackCommandExecution(
                "SaveCommand",
                stopwatch.Elapsed,
                success
            );
        }
    }
}
```

### Service Telemetry

```csharp
public class DataService : IDataService
{
    private readonly ITelemetryService _telemetry;

    public async Task<List<Item>> GetItemsAsync()
    {
        return await _telemetry.TrackAsync(
            operationName: "DataService.GetItems",
            operation: async () =>
            {
                using var activity = _telemetry.StartActivity("Database.Query");
                activity?.SetTag("query.type", "SELECT");

                var items = await _repository.GetAllAsync();

                activity?.SetTag("result.count", items.Count);
                return items;
            },
            tags: new Dictionary<string, object?>
            {
                ["data.source"] = "SQL Server"
            }
        );
    }
}
```

### Navigation Telemetry

```csharp
public class NavigationService : INavigationService
{
    private readonly ITelemetryService _telemetry;
    private string? _currentView;
    private Stopwatch? _viewStopwatch;

    public void NavigateTo(string viewName)
    {
        var fromView = _currentView ?? "None";
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Perform navigation
            DoNavigation(viewName);

            stopwatch.Stop();
            _telemetry.TrackNavigation(fromView, viewName, stopwatch.Elapsed);

            // Track page view
            _telemetry.TrackPageView(viewName);

            _currentView = viewName;
            _viewStopwatch = Stopwatch.StartNew();
        }
        catch (Exception ex)
        {
            _telemetry.RecordException(ex);
            throw;
        }
    }
}
```

### User Action Tracking

```csharp
// In your View code-behind or ViewModel
private void OnButtonClick(object sender, RoutedEventArgs e)
{
    _telemetry.TrackUserAction(
        action: "Click",
        target: "ExportButton",
        properties: new Dictionary<string, string>
        {
            ["format"] = "PDF",
            ["page_count"] = PageCount.ToString()
        }
    );

    // Handle click
}
```

---

## Claude Code Examples

### Prompt: Add Telemetry to New Feature

```
Add OpenTelemetry telemetry to the new ExportService:

1. Track export operations with TrackAsync
2. Record export duration and file size metrics
3. Track exceptions with context
4. Add activity tags for export format and destination

Use the existing ITelemetryService injected via DI.
```

### Prompt: Identify Performance Bottlenecks

```
Review the telemetry data in Aspire Dashboard and identify:

1. Commands with P99 duration >500ms
2. ViewModels with initialization >200ms
3. UI thread blocking events >1000ms

Suggest optimizations based on the telemetry findings.
```

### Prompt: Add Custom Metric

```
Add a custom histogram metric to track document processing time:

Metric name: wpfbase_document_processing_duration_ms
Dimensions: document_type, page_count_range
Record after each document is processed

Use RecordMetric with appropriate tags.
```

### Prompt: Create Telemetry Scope

```
Refactor the BatchProcessor to use telemetry scopes:

1. Create a scope for the entire batch operation
2. Track individual item processing with nested activities
3. Add batch_size and processed_count as scope properties
4. Mark success/failure appropriately

Use CreateScope and MarkSuccess/MarkFailure.
```

### Prompt: Add Dashboard Visualization

```
I need to see command execution patterns in Aspire Dashboard.

1. Ensure all commands record telemetry with TrackCommandExecution
2. Add command_category tag for grouping
3. Document how to filter in dashboard to see only failed commands
4. Add example queries for P90/P99 analysis
```

---

## Production Deployment

### Local Development

Use Aspire Dashboard (default configuration):

```json
"Telemetry": {
  "Enabled": true,
  "OtlpEndpoint": "http://localhost:4317"
}
```

### Production (Azure Monitor)

```json
"Telemetry": {
  "Enabled": true,
  "OtlpEndpoint": "https://your-workspace.monitor.azure.com/otlp",
  "Authentication": {
    "Type": "ApiKey",
    "ApiKey": "YOUR_KEY_FROM_KEYVAULT"
  }
}
```

Configure Azure Monitor with OpenTelemetry exporter:

```csharp
// In TelemetryService initialization
.AddOtlpExporter(options =>
{
    options.Endpoint = new Uri(azureMonitorEndpoint);
    options.Headers = $"x-api-key={apiKey}";
})
```

### Production (Jaeger)

For self-hosted Jaeger:

```json
"Telemetry": {
  "Enabled": true,
  "OtlpEndpoint": "http://jaeger-collector:4317"
}
```

### Production (Prometheus + Grafana)

For metrics-only with Prometheus:

```csharp
// Add Prometheus exporter instead of OTLP
.AddPrometheusExporter(options =>
{
    options.ScrapeEndpointPath = "/metrics";
})
```

### Sampling in Production

**Recommended production settings:**

```json
"Telemetry": {
  "Enabled": true,
  "Level": "Information",
  "SamplingRatio": 0.01,  // 1% of traces (reduces costs)
  "EnableRuntimeMetrics": false  // Reduce data volume
}
```

**Why sample in production:**
- Reduces telemetry costs (Azure Monitor, Datadog charge per GB)
- Lowers network overhead
- Metrics are always collected (not sampled)
- 1-10% sampling is sufficient for most production insights

### Cost Considerations

**Free tier (Aspire Dashboard):**
- Unlimited local development
- No cost, runs on your machine
- Supports 50+ users at $0

**Azure Monitor:**
- First 5GB/month free
- ~$2.30/GB after that
- Typical WPF app (10 users): ~500MB/month = FREE

**Datadog/New Relic:**
- Paid plans start at $15/month
- More features but higher cost

**Recommendation:** Start with Aspire Dashboard (free), scale to Azure Monitor if needed.

---

## Privacy and Data

### What Data is Collected?

**Automatically collected:**
- Operation names and timing
- Success/failure status
- Exception types and messages (NO sensitive data)
- Machine name and environment
- .NET runtime metrics (memory, GC)

**NOT collected:**
- User passwords or credentials
- Personal identifiable information (PII)
- File contents or sensitive data
- Database query parameters (unless you add them)

### Adding User Context Safely

```csharp
// Use anonymized/hashed IDs, never raw PII
_telemetry.SetUserId(HashUserId(email));
_telemetry.SetSessionId(Guid.NewGuid().ToString());

// Add non-sensitive global properties
_telemetry.AddGlobalProperty("department", "Engineering");
_telemetry.AddGlobalProperty("app_version", "1.2.3");
```

### GDPR Compliance

1. **Disable telemetry per user:**
   ```csharp
   if (userOptedOut)
   {
       _telemetry.IsEnabled = false;
   }
   ```

2. **Data retention:**
   - Aspire Dashboard: No persistence (RAM only)
   - Production: Configure retention (Azure: 30-90 days)

3. **Right to deletion:**
   - Delete telemetry data by user ID in backend

### Best Practices

- **Never log sensitive data** in tags or properties
- **Hash user identifiers** before adding to telemetry
- **Provide opt-out mechanism** in application settings
- **Document data collection** in privacy policy
- **Use sampling** to minimize data collection

---

## Troubleshooting

### Dashboard Not Showing Data

**Problem:** Telemetry enabled but Aspire Dashboard shows no data.

**Solution:**
1. Verify Docker container is running:
   ```bash
   docker ps | grep aspire-dashboard
   ```

2. Check OTLP endpoint is reachable:
   ```bash
   curl http://localhost:4317
   ```

3. Ensure telemetry is enabled in config:
   ```json
   "Telemetry": {
     "Enabled": true
   }
   ```

4. Check application logs for initialization errors:
   ```
   "Telemetry initialized with Aspire Dashboard integration"
   ```

### Docker Connection Refused

**Problem:** `Connection refused` when connecting to localhost:4317.

**Solution:**
- **Windows:** Docker Desktop might not expose localhost. Use `host.docker.internal`:
  ```json
  "OtlpEndpoint": "http://host.docker.internal:4317"
  ```

- **Linux:** Use host network mode:
  ```bash
  docker run --rm -it --network=host mcr.microsoft.com/dotnet/aspire-dashboard:9.5
  ```

### High Memory Usage

**Problem:** Application memory increases after enabling telemetry.

**Solution:**
1. Reduce telemetry level from Verbose to Information
2. Enable sampling (reduce from 1.0 to 0.1)
3. Disable runtime metrics if not needed
4. Check for activity disposal (always use `using`)

```json
"Telemetry": {
  "Level": "Information",
  "SamplingRatio": 0.1,
  "EnableRuntimeMetrics": false
}
```

### Missing Metrics

**Problem:** Activities show in dashboard but metrics don't appear.

**Solution:**
1. Ensure meter provider is initialized (check logs)
2. Verify metric names match exactly (case-sensitive)
3. Check sampling ratio (metrics are never sampled, but check config)
4. Restart application to reinitialize meter

### Slow Application Startup

**Problem:** Application takes longer to start with telemetry enabled.

**Solution:**
- Telemetry initialization is async and shouldn't block
- If blocking, move initialization to background:
  ```csharp
  Task.Run(() => InitializeTelemetry());
  ```

### Activities Not Nested Correctly

**Problem:** Child activities appear as separate root activities.

**Solution:**
- Ensure parent activity is active when starting child:
  ```csharp
  using var parent = _telemetry.StartActivity("Parent");
  // This automatically becomes a child:
  using var child = _telemetry.StartActivity("Child");
  ```

- Check for async context loss (use `ConfigureAwait(false)` carefully)

### Dashboard Shows Wrong Service Name

**Problem:** Activities appear under different service names.

**Solution:**
1. Ensure consistent ServiceName in config
2. Check for multiple ActivitySource instances
3. Verify resource attributes in initialization

```json
"Telemetry": {
  "ServiceName": "WPFBase"  // Must match everywhere
}
```

---

## Summary

**Getting Started (5 minutes):**
1. Start Aspire Dashboard: `docker run --rm -it -p 18888:18888 -p 4317:4317 mcr.microsoft.com/dotnet/aspire-dashboard:9.5`
2. Enable telemetry: `"Telemetry": { "Enabled": true }`
3. Open dashboard: http://localhost:18888
4. Run your application and see real-time telemetry

**Key Concepts:**
- **Activities (Traces)**: Track operation timing and hierarchy
- **Metrics**: Measure performance with counters and histograms
- **Dashboard**: Visualize telemetry in real-time (free)
- **Sampling**: Reduce overhead in production (1-10%)

**Production Ready:**
- <0.5% performance overhead
- GDPR compliant (no PII by default)
- Scales to Azure Monitor, Jaeger, Prometheus
- Free for small teams (Aspire Dashboard)

**Next Steps:**
1. Add telemetry to your ViewModels (initialization tracking)
2. Track command execution with success/failure
3. Monitor dashboard during development
4. Optimize based on P99 metrics
5. Deploy to production with sampling enabled