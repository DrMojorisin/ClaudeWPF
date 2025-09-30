# Phase 2: Performance Monitoring & Telemetry Implementation Guide

## Overview
Complete implementation guide for integrating OpenTelemetry and Application Insights into WPFBase, providing comprehensive performance monitoring optimized for WPF desktop applications.

## Goals
- Real-time performance monitoring with < 1% overhead
- Comprehensive telemetry for debugging production issues
- AI-friendly patterns for Claude Code generation
- Cost-effective with free tier options

## Step-by-Step Implementation

### Step 1: Install NuGet Packages

```xml
<!-- Add to WPFBase.csproj -->
<ItemGroup>
  <!-- OpenTelemetry Core -->
  <PackageReference Include="OpenTelemetry" Version="1.9.0" />
  <PackageReference Include="OpenTelemetry.Exporter.Console" Version="1.9.0" />
  <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.9.0" />
  <PackageReference Include="OpenTelemetry.Instrumentation.Runtime" Version="1.9.0" />

  <!-- Application Insights -->
  <PackageReference Include="Microsoft.ApplicationInsights.WorkerService" Version="2.22.0" />
  <PackageReference Include="OpenTelemetry.Exporter.AzureMonitor" Version="1.3.0" />

  <!-- Metrics & Tracing -->
  <PackageReference Include="System.Diagnostics.DiagnosticSource" Version="9.0.0" />
  <PackageReference Include="OpenTelemetry.Api" Version="1.9.0" />
  <PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.9.0" />
</ItemGroup>
```

### Step 2: Create Telemetry Service Interface

```csharp
// File: Interfaces/ITelemetryService.cs
using System.Diagnostics;

namespace WPFBase.Interfaces;

/// <summary>
/// Telemetry service for performance monitoring and metrics collection.
/// Optimized for WPF applications with minimal overhead.
/// </summary>
public interface ITelemetryService : IDisposable
{
    // Activity (Tracing) Operations
    Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal);
    void RecordException(Exception exception, Activity? activity = null);

    // Metrics
    void RecordMetric(string name, double value, Dictionary<string, object?>? tags = null);
    void IncrementCounter(string name, Dictionary<string, object?>? tags = null);
    void RecordDuration(string name, TimeSpan duration, Dictionary<string, object?>? tags = null);

    // WPF-Specific Metrics
    void TrackViewModelInitialization(string viewModelName, TimeSpan duration);
    void TrackCommandExecution(string commandName, TimeSpan duration, bool success);
    void TrackUIThreadBlocking(TimeSpan duration);
    void TrackDataBindingPerformance(string propertyName, TimeSpan duration);
    void TrackMemoryPressure(long workingSet, long gcTotalMemory);

    // Custom Events
    void TrackEvent(string eventName, Dictionary<string, string>? properties = null);
    void TrackPageView(string pageName, TimeSpan? duration = null);

    // Configuration
    void SetUserId(string userId);
    void SetSessionId(string sessionId);
    bool IsEnabled { get; }
    TelemetryLevel Level { get; set; }
}

public enum TelemetryLevel
{
    None = 0,
    Critical = 1,
    Error = 2,
    Warning = 3,
    Information = 4,
    Verbose = 5
}
```

### Step 3: Implement Telemetry Service

```csharp
// File: Services/TelemetryService.cs
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WPFBase.Interfaces;

namespace WPFBase.Services;

/// <summary>
/// Production-ready telemetry service with OpenTelemetry and Application Insights.
/// Optimized for WPF desktop applications with minimal performance impact.
/// </summary>
public class TelemetryService : ITelemetryService
{
    private readonly TelemetryClient? _telemetryClient;
    private readonly ActivitySource _activitySource;
    private readonly Meter _meter;
    private readonly ILoggingService _logger;

    // Metrics
    private readonly Counter<long> _commandExecutionCounter;
    private readonly Histogram<double> _commandDurationHistogram;
    private readonly Counter<long> _exceptionCounter;
    private readonly Histogram<double> _viewModelInitHistogram;
    private readonly Histogram<double> _uiBlockingHistogram;

    // Configuration
    private bool _isEnabled;
    private TelemetryLevel _level = TelemetryLevel.Information;
    private readonly string _applicationName;
    private readonly string _applicationVersion;

    public TelemetryService(
        IConfigurationService configuration,
        ILoggingService logger)
    {
        _logger = logger;
        _applicationName = "WPFBase";
        _applicationVersion = Assembly.GetExecutingAssembly()
            .GetName().Version?.ToString() ?? "1.0.0";

        // Initialize activity source for tracing
        _activitySource = new ActivitySource(_applicationName, _applicationVersion);

        // Initialize meter for metrics
        _meter = new Meter(_applicationName, _applicationVersion);

        // Create metrics instruments
        _commandExecutionCounter = _meter.CreateCounter<long>(
            "wpfbase.command.executions",
            description: "Number of command executions");

        _commandDurationHistogram = _meter.CreateHistogram<double>(
            "wpfbase.command.duration",
            unit: "ms",
            description: "Command execution duration");

        _exceptionCounter = _meter.CreateCounter<long>(
            "wpfbase.exceptions",
            description: "Number of exceptions");

        _viewModelInitHistogram = _meter.CreateHistogram<double>(
            "wpfbase.viewmodel.init.duration",
            unit: "ms",
            description: "ViewModel initialization duration");

        _uiBlockingHistogram = _meter.CreateHistogram<double>(
            "wpfbase.ui.blocking.duration",
            unit: "ms",
            description: "UI thread blocking duration");

        // Initialize based on configuration
        var connectionString = configuration.GetValue<string>("Telemetry:ConnectionString");
        _isEnabled = configuration.GetValue<bool>("Telemetry:Enabled", false);
        _level = configuration.GetValue<TelemetryLevel>("Telemetry:Level", TelemetryLevel.Information);

        if (_isEnabled && !string.IsNullOrEmpty(connectionString))
        {
            InitializeTelemetry(connectionString);
        }

        _logger.LogInformation("Telemetry service initialized. Enabled: {Enabled}, Level: {Level}",
            _isEnabled, _level);
    }

    private void InitializeTelemetry(string connectionString)
    {
        try
        {
            // Configure Application Insights
            var telemetryConfig = TelemetryConfiguration.CreateDefault();
            telemetryConfig.ConnectionString = connectionString;

            // Add custom telemetry initializers
            telemetryConfig.TelemetryInitializers.Add(new WPFTelemetryInitializer());

            // Add telemetry processors for filtering
            telemetryConfig.DefaultTelemetrySink.TelemetryProcessors.Add(
                new WPFTelemetryProcessor(_level));

            _telemetryClient = new TelemetryClient(telemetryConfig);
            _telemetryClient.Context.Application.Version = _applicationVersion;
            _telemetryClient.Context.Device.Type = "Desktop";
            _telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();

            // Configure OpenTelemetry
            ConfigureOpenTelemetry(connectionString);

            _logger.LogInformation("Application Insights telemetry initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize telemetry");
            _isEnabled = false;
        }
    }

    private void ConfigureOpenTelemetry(string connectionString)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName: _applicationName, serviceVersion: _applicationVersion)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = "production",
                ["telemetry.sdk.language"] = "dotnet",
                ["service.instance.id"] = Environment.MachineName
            });

        // Configure tracing
        Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddSource(_applicationName)
            .AddHttpClientInstrumentation()
            .AddAzureMonitorTraceExporter(options =>
            {
                options.ConnectionString = connectionString;
            })
            .Build();

        // Configure metrics
        Sdk.CreateMeterProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddMeter(_applicationName)
            .AddRuntimeInstrumentation()
            .AddHttpClientInstrumentation()
            .AddAzureMonitorMetricExporter(options =>
            {
                options.ConnectionString = connectionString;
            })
            .Build();
    }

    public bool IsEnabled => _isEnabled;
    public TelemetryLevel Level
    {
        get => _level;
        set => _level = value;
    }

    public Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        if (!_isEnabled) return null;

        var activity = _activitySource.StartActivity(name, kind);
        activity?.SetTag("wpfbase.component", "application");
        return activity;
    }

    public void RecordException(Exception exception, Activity? activity = null)
    {
        if (!_isEnabled) return;

        _exceptionCounter.Add(1,
            new KeyValuePair<string, object?>("exception.type", exception.GetType().Name));

        activity?.RecordException(exception);

        _telemetryClient?.TrackException(new ExceptionTelemetry(exception)
        {
            SeverityLevel = SeverityLevel.Error,
            Properties =
            {
                ["Activity"] = activity?.DisplayName ?? "Unknown",
                ["Source"] = exception.Source ?? "Unknown"
            }
        });
    }

    public void RecordMetric(string name, double value, Dictionary<string, object?>? tags = null)
    {
        if (!_isEnabled) return;

        var metric = new MetricTelemetry(name, value);

        if (tags != null)
        {
            foreach (var tag in tags)
            {
                metric.Properties[tag.Key] = tag.Value?.ToString() ?? "";
            }
        }

        _telemetryClient?.TrackMetric(metric);
    }

    public void IncrementCounter(string name, Dictionary<string, object?>? tags = null)
    {
        if (!_isEnabled) return;

        RecordMetric(name, 1, tags);
    }

    public void RecordDuration(string name, TimeSpan duration, Dictionary<string, object?>? tags = null)
    {
        if (!_isEnabled) return;

        RecordMetric(name, duration.TotalMilliseconds, tags);
    }

    public void TrackViewModelInitialization(string viewModelName, TimeSpan duration)
    {
        if (!_isEnabled) return;

        _viewModelInitHistogram.Record(
            duration.TotalMilliseconds,
            new KeyValuePair<string, object?>("viewmodel", viewModelName));

        _telemetryClient?.TrackEvent("ViewModelInitialized",
            properties: new Dictionary<string, string>
            {
                ["ViewModelName"] = viewModelName,
                ["Duration"] = duration.TotalMilliseconds.ToString("F2")
            },
            metrics: new Dictionary<string, double>
            {
                ["DurationMs"] = duration.TotalMilliseconds
            });
    }

    public void TrackCommandExecution(string commandName, TimeSpan duration, bool success)
    {
        if (!_isEnabled) return;

        _commandExecutionCounter.Add(1,
            new KeyValuePair<string, object?>("command", commandName),
            new KeyValuePair<string, object?>("success", success));

        _commandDurationHistogram.Record(
            duration.TotalMilliseconds,
            new KeyValuePair<string, object?>("command", commandName));

        _telemetryClient?.TrackEvent("CommandExecuted",
            properties: new Dictionary<string, string>
            {
                ["CommandName"] = commandName,
                ["Success"] = success.ToString(),
                ["Duration"] = duration.TotalMilliseconds.ToString("F2")
            },
            metrics: new Dictionary<string, double>
            {
                ["DurationMs"] = duration.TotalMilliseconds
            });
    }

    public void TrackUIThreadBlocking(TimeSpan duration)
    {
        if (!_isEnabled) return;

        _uiBlockingHistogram.Record(duration.TotalMilliseconds);

        if (duration.TotalMilliseconds > 100) // Log if blocking > 100ms
        {
            _telemetryClient?.TrackEvent("UIThreadBlocked",
                properties: new Dictionary<string, string>
                {
                    ["Duration"] = duration.TotalMilliseconds.ToString("F2"),
                    ["Severity"] = duration.TotalMilliseconds > 1000 ? "High" : "Medium"
                },
                metrics: new Dictionary<string, double>
                {
                    ["DurationMs"] = duration.TotalMilliseconds
                });
        }
    }

    public void TrackDataBindingPerformance(string propertyName, TimeSpan duration)
    {
        if (!_isEnabled || _level < TelemetryLevel.Verbose) return;

        if (duration.TotalMilliseconds > 10) // Only track slow bindings
        {
            RecordMetric("wpfbase.databinding.duration",
                duration.TotalMilliseconds,
                new Dictionary<string, object?>
                {
                    ["property"] = propertyName
                });
        }
    }

    public void TrackMemoryPressure(long workingSet, long gcTotalMemory)
    {
        if (!_isEnabled) return;

        RecordMetric("wpfbase.memory.workingset", workingSet / (1024.0 * 1024.0)); // MB
        RecordMetric("wpfbase.memory.managed", gcTotalMemory / (1024.0 * 1024.0)); // MB
    }

    public void TrackEvent(string eventName, Dictionary<string, string>? properties = null)
    {
        if (!_isEnabled) return;

        _telemetryClient?.TrackEvent(eventName, properties);
    }

    public void TrackPageView(string pageName, TimeSpan? duration = null)
    {
        if (!_isEnabled) return;

        var pageView = new PageViewTelemetry(pageName);

        if (duration.HasValue)
        {
            pageView.Duration = duration.Value;
        }

        _telemetryClient?.TrackPageView(pageView);
    }

    public void SetUserId(string userId)
    {
        if (_telemetryClient != null)
        {
            _telemetryClient.Context.User.Id = userId;
        }
    }

    public void SetSessionId(string sessionId)
    {
        if (_telemetryClient != null)
        {
            _telemetryClient.Context.Session.Id = sessionId;
        }
    }

    public void Dispose()
    {
        _telemetryClient?.Flush();
        _activitySource?.Dispose();
        _meter?.Dispose();

        // Wait for telemetry to be sent
        Thread.Sleep(1000);
    }
}

/// <summary>
/// Custom telemetry initializer for WPF applications
/// </summary>
internal class WPFTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.GlobalProperties["AppType"] = "WPF";
        telemetry.Context.GlobalProperties["Framework"] = ".NET 9";
        telemetry.Context.GlobalProperties["UIThread"] =
            Thread.CurrentThread.GetApartmentState() == ApartmentState.STA ? "STA" : "MTA";
    }
}

/// <summary>
/// Telemetry processor for filtering based on level
/// </summary>
internal class WPFTelemetryProcessor : ITelemetryProcessor
{
    private readonly TelemetryLevel _level;
    private readonly ITelemetryProcessor _next;

    public WPFTelemetryProcessor(TelemetryLevel level, ITelemetryProcessor? next = null)
    {
        _level = level;
        _next = next ?? new StubTelemetryProcessor();
    }

    public void Process(ITelemetry item)
    {
        // Filter based on telemetry level
        if (ShouldProcess(item))
        {
            _next.Process(item);
        }
    }

    private bool ShouldProcess(ITelemetry item)
    {
        return item switch
        {
            ExceptionTelemetry => _level >= TelemetryLevel.Error,
            EventTelemetry evt => ShouldProcessEvent(evt),
            MetricTelemetry => _level >= TelemetryLevel.Information,
            PageViewTelemetry => _level >= TelemetryLevel.Information,
            DependencyTelemetry => _level >= TelemetryLevel.Verbose,
            RequestTelemetry => _level >= TelemetryLevel.Verbose,
            TraceTelemetry trace => ShouldProcessTrace(trace),
            _ => true
        };
    }

    private bool ShouldProcessEvent(EventTelemetry evt)
    {
        // Filter specific events based on level
        return evt.Name switch
        {
            "UIThreadBlocked" => _level >= TelemetryLevel.Warning,
            "CommandExecuted" => _level >= TelemetryLevel.Information,
            "ViewModelInitialized" => _level >= TelemetryLevel.Verbose,
            _ => _level >= TelemetryLevel.Information
        };
    }

    private bool ShouldProcessTrace(TraceTelemetry trace)
    {
        return trace.SeverityLevel switch
        {
            SeverityLevel.Critical => _level >= TelemetryLevel.Critical,
            SeverityLevel.Error => _level >= TelemetryLevel.Error,
            SeverityLevel.Warning => _level >= TelemetryLevel.Warning,
            SeverityLevel.Information => _level >= TelemetryLevel.Information,
            SeverityLevel.Verbose => _level >= TelemetryLevel.Verbose,
            _ => true
        };
    }
}

internal class StubTelemetryProcessor : ITelemetryProcessor
{
    public void Process(ITelemetry item) { }
}
```

### Step 4: Integrate with ViewModelBase

```csharp
// File: ViewModels/Base/ViewModelBase.cs (Update)
public abstract partial class ViewModelBase : ObservableObject
{
    private readonly ITelemetryService? _telemetry;
    private readonly Stopwatch _initStopwatch;
    private Activity? _viewModelActivity;

    protected ViewModelBase()
    {
        _initStopwatch = Stopwatch.StartNew();

        // Get telemetry from DI if available
        _telemetry = App.Current?.Services?.GetService<ITelemetryService>();

        // Start activity for this view model
        var viewModelName = GetType().Name;
        _viewModelActivity = _telemetry?.StartActivity($"ViewModel.{viewModelName}");

        // Track initialization when complete
        Loaded += () =>
        {
            _initStopwatch.Stop();
            _telemetry?.TrackViewModelInitialization(viewModelName, _initStopwatch.Elapsed);
            _viewModelActivity?.Stop();
        };
    }

    // Add telemetry to existing IsBusy property
    partial void OnIsBusyChanged(bool value)
    {
        if (value)
        {
            _busyStopwatch = Stopwatch.StartNew();
        }
        else if (_busyStopwatch != null)
        {
            _busyStopwatch.Stop();
            _telemetry?.RecordDuration("viewmodel.busy.duration",
                _busyStopwatch.Elapsed,
                new Dictionary<string, object?>
                {
                    ["viewmodel"] = GetType().Name
                });
        }
    }

    // Helper method for tracking command execution
    protected async Task<T> ExecuteWithTelemetryAsync<T>(
        string commandName,
        Func<Task<T>> action)
    {
        using var activity = _telemetry?.StartActivity($"Command.{commandName}");
        var stopwatch = Stopwatch.StartNew();
        var success = false;

        try
        {
            var result = await action();
            success = true;
            return result;
        }
        catch (Exception ex)
        {
            _telemetry?.RecordException(ex, activity);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _telemetry?.TrackCommandExecution(commandName, stopwatch.Elapsed, success);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _viewModelActivity?.Dispose();
        }

        base.Dispose(disposing);
    }
}
```

### Step 5: Add Performance Monitoring to Commands

```csharp
// File: Commands/TelemetryAwareRelayCommand.cs
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;

namespace WPFBase.Commands;

/// <summary>
/// Relay command with built-in telemetry tracking
/// </summary>
public class TelemetryAwareAsyncRelayCommand : AsyncRelayCommand
{
    private readonly ITelemetryService? _telemetry;
    private readonly string _commandName;

    public TelemetryAwareAsyncRelayCommand(
        Func<Task> execute,
        Func<bool>? canExecute = null,
        string? commandName = null)
        : base(execute, canExecute)
    {
        _telemetry = App.Current?.Services?.GetService<ITelemetryService>();
        _commandName = commandName ?? "UnnamedCommand";
    }

    protected override async Task ExecuteAsync(object? parameter)
    {
        using var activity = _telemetry?.StartActivity($"Command.{_commandName}");
        var stopwatch = Stopwatch.StartNew();
        var success = false;

        try
        {
            await base.ExecuteAsync(parameter);
            success = true;
        }
        catch (Exception ex)
        {
            _telemetry?.RecordException(ex, activity);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _telemetry?.TrackCommandExecution(_commandName, stopwatch.Elapsed, success);
        }
    }
}
```

### Step 6: Configure Application Settings

```json
// File: appsettings.json
{
  "Telemetry": {
    "Enabled": true,
    "Level": "Information",
    "ConnectionString": "InstrumentationKey=YOUR_KEY;IngestionEndpoint=https://YOUR_REGION.in.applicationinsights.azure.com/",
    "SamplingPercentage": 10.0,
    "EnableAdaptiveSampling": true,
    "ExcludedTypes": [
      "DependencyTelemetry",
      "RequestTelemetry"
    ],
    "PerformanceCounters": {
      "CollectCPU": true,
      "CollectMemory": true,
      "CollectGC": true,
      "CollectThreads": true
    }
  }
}
```

### Step 7: Register in Dependency Injection

```csharp
// File: App.xaml.cs (Update ConfigureServices)
private void ConfigureServices(IServiceCollection services)
{
    // ... existing services ...

    // Register telemetry service
    services.AddSingleton<ITelemetryService, TelemetryService>();

    // Configure OpenTelemetry (alternative to in-service configuration)
    services.AddOpenTelemetry()
        .WithTracing(builder =>
        {
            builder
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService("WPFBase"))
                .AddSource("WPFBase")
                .AddHttpClientInstrumentation()
                .AddAzureMonitorTraceExporter();
        })
        .WithMetrics(builder =>
        {
            builder
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService("WPFBase"))
                .AddMeter("WPFBase")
                .AddRuntimeInstrumentation()
                .AddHttpClientInstrumentation()
                .AddAzureMonitorMetricExporter();
        });
}
```

### Step 8: Create Performance Dashboard View

```csharp
// File: ViewModels/Tools/PerformanceDashboardViewModel.cs
public partial class PerformanceDashboardViewModel : ToolViewModel
{
    private readonly ITelemetryService _telemetry;
    private readonly IPerformanceOptimizationService _performanceService;
    private readonly DispatcherTimer _updateTimer;

    [ObservableProperty]
    private double cpuUsage;

    [ObservableProperty]
    private long memoryUsage;

    [ObservableProperty]
    private int gcCollections;

    [ObservableProperty]
    private double averageCommandDuration;

    [ObservableProperty]
    private ObservableCollection<PerformanceMetric> metrics = new();

    public PerformanceDashboardViewModel(
        ITelemetryService telemetry,
        IPerformanceOptimizationService performanceService)
    {
        _telemetry = telemetry;
        _performanceService = performanceService;

        Title = "Performance Dashboard";
        ContentId = "PerformanceDashboard";

        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _updateTimer.Tick += UpdateMetrics;
        _updateTimer.Start();
    }

    private void UpdateMetrics(object? sender, EventArgs e)
    {
        // Update CPU usage
        CpuUsage = _performanceService.GetCPUUsage();

        // Update memory usage
        var process = Process.GetCurrentProcess();
        MemoryUsage = process.WorkingSet64;

        // Update GC collections
        GcCollections = GC.CollectionCount(0);

        // Track memory pressure
        _telemetry.TrackMemoryPressure(MemoryUsage, GC.GetTotalMemory(false));

        // Add to metrics history
        Metrics.Add(new PerformanceMetric
        {
            Timestamp = DateTime.Now,
            CpuUsage = CpuUsage,
            MemoryUsage = MemoryUsage,
            GcCollections = GcCollections
        });

        // Keep only last 60 seconds
        while (Metrics.Count > 60)
        {
            Metrics.RemoveAt(0);
        }
    }

    [RelayCommand]
    private void ForceGarbageCollection()
    {
        _performanceService.PerformMemoryCleanup(aggressive: true);
        _telemetry.TrackEvent("ManualGarbageCollection");
    }

    [RelayCommand]
    private async Task ExportMetricsAsync()
    {
        // Export current metrics to CSV
        var csv = new StringBuilder();
        csv.AppendLine("Timestamp,CPU Usage,Memory (MB),GC Collections");

        foreach (var metric in Metrics)
        {
            csv.AppendLine($"{metric.Timestamp:yyyy-MM-dd HH:mm:ss}," +
                          $"{metric.CpuUsage:F2}," +
                          $"{metric.MemoryUsage / (1024.0 * 1024.0):F2}," +
                          $"{metric.GcCollections}");
        }

        // Save to file
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            $"WPFBase_Metrics_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

        await File.WriteAllTextAsync(path, csv.ToString());

        _telemetry.TrackEvent("MetricsExported", new Dictionary<string, string>
        {
            ["FilePath"] = path,
            ["RecordCount"] = Metrics.Count.ToString()
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _updateTimer?.Stop();
        }

        base.Dispose(disposing);
    }
}

public class PerformanceMetric
{
    public DateTime Timestamp { get; set; }
    public double CpuUsage { get; set; }
    public long MemoryUsage { get; set; }
    public int GcCollections { get; set; }
}
```

### Step 9: Create WPF Performance Monitoring Behavior

```csharp
// File: Behaviors/PerformanceMonitoringBehavior.cs
using System.Windows;
using System.Windows.Interactivity;
using System.Diagnostics;

namespace WPFBase.Behaviors;

/// <summary>
/// Behavior for monitoring WPF control rendering performance
/// </summary>
public class PerformanceMonitoringBehavior : Behavior<FrameworkElement>
{
    private readonly ITelemetryService? _telemetry;
    private Stopwatch? _renderStopwatch;

    public PerformanceMonitoringBehavior()
    {
        _telemetry = App.Current?.Services?.GetService<ITelemetryService>();
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.Loaded += OnLoaded;
        AssociatedObject.SizeChanged += OnSizeChanged;

        if (AssociatedObject is IInputElement inputElement)
        {
            CompositionTarget.Rendering += OnRendering;
        }
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= OnLoaded;
        AssociatedObject.SizeChanged -= OnSizeChanged;
        CompositionTarget.Rendering -= OnRendering;

        base.OnDetaching();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _renderStopwatch?.Stop();

        if (_renderStopwatch != null)
        {
            _telemetry?.RecordDuration(
                "wpfbase.control.load",
                _renderStopwatch.Elapsed,
                new Dictionary<string, object?>
                {
                    ["control"] = AssociatedObject.GetType().Name
                });
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        _telemetry?.RecordMetric(
            "wpfbase.control.size",
            e.NewSize.Width * e.NewSize.Height,
            new Dictionary<string, object?>
            {
                ["control"] = AssociatedObject.GetType().Name,
                ["width"] = e.NewSize.Width,
                ["height"] = e.NewSize.Height
            });
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        if (_renderStopwatch == null)
        {
            _renderStopwatch = Stopwatch.StartNew();
        }
    }
}
```

### Step 10: Usage Examples

```csharp
// Example: Using telemetry in ViewModels
public partial class CustomerViewModel : ViewModelBase
{
    private readonly ICustomerService _customerService;
    private readonly ITelemetryService _telemetry;

    [ObservableProperty]
    private ObservableCollection<Customer> customers = new();

    public CustomerViewModel(
        ICustomerService customerService,
        ITelemetryService telemetry)
    {
        _customerService = customerService;
        _telemetry = telemetry;
    }

    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        using var activity = _telemetry.StartActivity("LoadCustomers");

        try
        {
            IsBusy = true;

            var stopwatch = Stopwatch.StartNew();
            var customers = await _customerService.GetAllAsync();
            stopwatch.Stop();

            _telemetry.RecordMetric("customers.loaded", customers.Count);
            _telemetry.RecordDuration("customers.load.duration", stopwatch.Elapsed);

            Customers.Clear();
            foreach (var customer in customers)
            {
                Customers.Add(customer);
            }

            _telemetry.TrackEvent("CustomersLoaded", new Dictionary<string, string>
            {
                ["Count"] = customers.Count.ToString(),
                ["Duration"] = stopwatch.ElapsedMilliseconds.ToString()
            });
        }
        catch (Exception ex)
        {
            _telemetry.RecordException(ex, activity);
            await _dialogService.ShowErrorAsync($"Failed to load customers: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

## Testing the Implementation

```csharp
// File: WPFBase.Tests/Services/TelemetryServiceTests.cs
[TestClass]
public class TelemetryServiceTests
{
    private TelemetryService _service;
    private Mock<IConfigurationService> _mockConfig;
    private Mock<ILoggingService> _mockLogger;

    [TestInitialize]
    public void Setup()
    {
        _mockConfig = new Mock<IConfigurationService>();
        _mockLogger = new Mock<ILoggingService>();

        // Configure for testing without actual telemetry
        _mockConfig.Setup(x => x.GetValue<bool>("Telemetry:Enabled"))
            .Returns(true);
        _mockConfig.Setup(x => x.GetValue<string>("Telemetry:ConnectionString"))
            .Returns(""); // Empty to prevent actual connection

        _service = new TelemetryService(_mockConfig.Object, _mockLogger.Object);
    }

    [TestMethod]
    public void TrackViewModelInitialization_RecordsMetric()
    {
        // Act
        _service.TrackViewModelInitialization("TestViewModel", TimeSpan.FromMilliseconds(100));

        // Assert
        _mockLogger.Verify(x => x.LogInformation(
            It.IsAny<string>(),
            It.IsAny<object[]>()),
            Times.AtLeastOnce);
    }

    [TestMethod]
    public void StartActivity_WhenEnabled_ReturnsActivity()
    {
        // Act
        using var activity = _service.StartActivity("TestActivity");

        // Assert
        activity.Should().NotBeNull();
        activity.DisplayName.Should().Be("TestActivity");
    }
}
```

## Cost Analysis

### Azure Application Insights Pricing (2024)
- **Free Tier**: 5 GB/month data ingestion
- **Paid Tier**: $2.30 per GB after free tier
- **Data Retention**: 90 days free, extended retention extra

### Estimated Monthly Costs
| User Count | Daily Events | Monthly Data | Cost |
|------------|-------------|--------------|------|
| 1-10 | 10K | 300 MB | Free |
| 10-50 | 50K | 1.5 GB | Free |
| 50-200 | 200K | 6 GB | $2.30 |
| 200-1000 | 1M | 30 GB | $57.50 |

### Cost Optimization Strategies
1. **Sampling**: Reduce data by 90% with 10% sampling
2. **Level Control**: Use Information level in production
3. **Local Development**: Disable in debug builds
4. **Batch Sending**: Reduce API calls

## Dashboard Setup

### Option 1: Azure Portal
1. Navigate to Application Insights resource
2. Use built-in dashboards for:
   - Application Map
   - Performance metrics
   - Failure analysis
   - User flows

### Option 2: Grafana (Self-Hosted)
```yaml
# docker-compose.yml for Grafana
version: '3'
services:
  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    volumes:
      - grafana-storage:/var/lib/grafana
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
```

## Implementation Checklist

### Week 1
- [ ] Install NuGet packages
- [ ] Create ITelemetryService interface
- [ ] Implement TelemetryService
- [ ] Update ViewModelBase integration
- [ ] Create telemetry-aware commands
- [ ] Configure application settings

### Week 2
- [ ] Add performance dashboard
- [ ] Create WPF behaviors
- [ ] Implement custom telemetry processors
- [ ] Setup Application Insights resource
- [ ] Configure dashboards
- [ ] Test telemetry pipeline
- [ ] Document usage patterns

## Claude Code Templates

```
"Add telemetry to CustomerViewModel load operation"
"Create performance dashboard for monitoring app metrics"
"Generate telemetry tests for all services"
"Add custom metrics for data processing operations"
```

## Benefits Achieved

1. **Visibility**: Complete insight into application performance
2. **Debugging**: Detailed traces for production issues
3. **Optimization**: Data-driven performance improvements
4. **User Experience**: Track actual usage patterns
5. **Proactive**: Identify issues before users report them

---

*This implementation provides enterprise-grade telemetry with minimal performance impact while maintaining Claude Code-friendly patterns.*