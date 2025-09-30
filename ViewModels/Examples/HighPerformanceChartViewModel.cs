using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScottPlot;
using ScottPlot.WPF;
using System.Diagnostics;
using System.Windows;
using WPFBase.Interfaces;

namespace WPFBase.ViewModels.Examples;

/// <summary>
/// Demonstrates ScottPlot 5.0 high-performance charting capabilities.
/// Showcases real-time data visualization with millions of data points.
/// Optimized for Claude Code AI-assisted development with clear patterns.
/// </summary>
public partial class HighPerformanceChartViewModel : ObservableObject, IDisposable
{
    private readonly ITelemetryService? _telemetry;
    private readonly IDialogService _dialogService;
    private readonly Random _random = new();
    private readonly Timer? _realTimeTimer;

    #region Properties

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateDataCommand))]
    [NotifyCanExecuteChangedFor(nameof(StartRealTimeCommand))]
    public partial bool IsGenerating { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartRealTimeCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopRealTimeCommand))]
    public partial bool IsRealTimeActive { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DataPointsDisplay))]
    public partial int DataPointCount { get; set; } = 1000;

    [ObservableProperty]
    public partial double FrameRate { get; set; }

    [ObservableProperty]
    public partial TimeSpan LastGenerationTime { get; set; }

    [ObservableProperty]
    public partial long MemoryUsage { get; set; }

    /// <summary>
    /// The main chart control for high-performance plotting
    /// </summary>
    [ObservableProperty]
    public partial WpfPlot? ChartControl { get; set; }

    /// <summary>
    /// Real-time data arrays for maximum performance
    /// </summary>
    [ObservableProperty]
    public partial double[]? RealTimeData { get; set; }

    /// <summary>
    /// Signal plot for ultra-high performance with large datasets
    /// </summary>
    private ScottPlot.Plottables.Signal? _signalPlot;

    // Computed properties
    public string DataPointsDisplay => $"{DataPointCount:N0} points";

    #endregion

    #region Constructor

    public HighPerformanceChartViewModel(
        IDialogService dialogService,
        ITelemetryService? telemetry = null)
    {
        _dialogService = dialogService;
        _telemetry = telemetry;

        // Initialize chart control
        InitializeChart();

        // Setup real-time timer (60 FPS target)
        if (_telemetry != null)
        {
            _realTimeTimer = new Timer(UpdateRealTimeData, null, Timeout.Infinite, Timeout.Infinite);
        }
    }

    #endregion

    #region Chart Initialization

    private void InitializeChart()
    {
        ChartControl = new WpfPlot();

        // Configure for high performance with ScottPlot 5.0 API
        ChartControl.Plot.Axes.Color(Color.FromHex("#1e1e1e"));
        ChartControl.Plot.Grid.MajorLineColor = Color.FromHex("#404040");
        ChartControl.Plot.FigureBackground.Color = Color.FromHex("#1e1e1e");
        ChartControl.Plot.DataBackground.Color = Color.FromHex("#2d2d2d");

        // Set performance optimizations
        ChartControl.Plot.RenderManager.EnableRendering = true;
        // Note: ScottPlot 5.0 anti-aliasing is handled automatically for optimal performance

        // Configure axes
        ChartControl.Plot.Axes.Title.Label.Text = "High-Performance Real-Time Data (ScottPlot 5.0)";
        ChartControl.Plot.Axes.Bottom.Label.Text = "Time (samples)";
        ChartControl.Plot.Axes.Left.Label.Text = "Value";

        _telemetry?.TrackEvent("Chart.Initialized", new Dictionary<string, string>
        {
            ["ChartType"] = "ScottPlot",
            ["Version"] = "5.0",
            ["Performance"] = "Optimized"
        });
    }

    #endregion

    #region Commands

    /// <summary>
    /// Generate large dataset for performance testing
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGenerateData))]
    private async Task GenerateDataAsync()
    {
        IsGenerating = true;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var scope = _telemetry?.CreateScope("Chart.GenerateData", new Dictionary<string, object?>
            {
                ["DataPoints"] = DataPointCount
            });

            await Task.Run(() =>
            {
                // Generate large dataset
                var data = new double[DataPointCount];
                var amplitude = 50.0;
                var frequency = 0.01;
                var noise = 10.0;

                for (int i = 0; i < DataPointCount; i++)
                {
                    // Generate sine wave with noise
                    data[i] = amplitude * Math.Sin(frequency * i) +
                             noise * (_random.NextDouble() - 0.5);
                }

                // Update UI on main thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ChartControl?.Plot.Clear();

                    if (DataPointCount > 100_000)
                    {
                        // Use SignalConst for ultra-large datasets (immutable, faster)
                        ChartControl?.Plot.Add.SignalConst(data);
                    }
                    else
                    {
                        // Use regular Signal for smaller datasets
                        _signalPlot = ChartControl?.Plot.Add.Signal(data);
                        if (_signalPlot != null)
                        {
                            _signalPlot.Color = Color.FromHex("#00d4ff");
                            _signalPlot.LineWidth = 1;
                        }
                    }

                    ChartControl?.Plot.Axes.AutoScale();
                    ChartControl?.Refresh();
                });
            });

            stopwatch.Stop();
            LastGenerationTime = stopwatch.Elapsed;

            // Update memory usage
            GC.Collect();
            MemoryUsage = GC.GetTotalMemory(false);

            // Track performance metrics
            _telemetry?.TrackCommandExecution("GenerateData", stopwatch.Elapsed, true);
            _telemetry?.RecordMetric("chart_generation_time_ms", stopwatch.Elapsed.TotalMilliseconds,
                new Dictionary<string, object?> { ["data_points"] = DataPointCount });

            await _dialogService.ShowInformationAsync(
                $"Generated {DataPointCount:N0} data points in {stopwatch.ElapsedMilliseconds:N0} ms\n" +
                $"Memory usage: {MemoryUsage / 1024.0 / 1024.0:F1} MB");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _telemetry?.RecordException(ex);
            _telemetry?.TrackCommandExecution("GenerateData", stopwatch.Elapsed, false);

            await _dialogService.ShowErrorAsync($"Failed to generate data: {ex.Message}");
        }
        finally
        {
            IsGenerating = false;
        }
    }

    private bool CanGenerateData() => !IsGenerating && !IsRealTimeActive;

    /// <summary>
    /// Start real-time data streaming at 60 FPS
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStartRealTime))]
    private async Task StartRealTimeAsync()
    {
        try
        {
            IsRealTimeActive = true;

            // Initialize real-time data array
            RealTimeData = new double[1000];
            for (int i = 0; i < RealTimeData.Length; i++)
            {
                RealTimeData[i] = 0;
            }

            // Clear existing plots and add signal for real-time updates
            ChartControl?.Plot.Clear();
            _signalPlot = ChartControl?.Plot.Add.Signal(RealTimeData);
            if (_signalPlot != null)
            {
                _signalPlot.Color = Color.FromHex("#ff6b35");
                _signalPlot.LineWidth = 2;
            }

            ChartControl?.Plot.Axes.SetLimits(0, RealTimeData.Length, -100, 100);

            // Start high-frequency timer (targeting 60 FPS)
            _realTimeTimer?.Change(0, 16); // ~60 FPS (16ms interval)

            _telemetry?.TrackEvent("RealTime.Started", new Dictionary<string, string>
            {
                ["TargetFPS"] = "60",
                ["DataPoints"] = RealTimeData.Length.ToString()
            });

            await _dialogService.ShowInformationAsync(
                "Real-time data streaming started!\n" +
                "Target: 60 FPS with 1,000 data points\n" +
                "Watch the frame rate counter.");
        }
        catch (Exception ex)
        {
            IsRealTimeActive = false;
            _telemetry?.RecordException(ex);
            await _dialogService.ShowErrorAsync($"Failed to start real-time mode: {ex.Message}");
        }
    }

    private bool CanStartRealTime() => !IsGenerating && !IsRealTimeActive;

    /// <summary>
    /// Stop real-time data streaming
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStopRealTime))]
    private async Task StopRealTimeAsync()
    {
        IsRealTimeActive = false;
        _realTimeTimer?.Change(Timeout.Infinite, Timeout.Infinite);

        _telemetry?.TrackEvent("RealTime.Stopped", new Dictionary<string, string>
        {
            ["FinalFPS"] = FrameRate.ToString("F1")
        });

        await _dialogService.ShowInformationAsync($"Real-time streaming stopped.\nFinal frame rate: {FrameRate:F1} FPS");
    }

    private bool CanStopRealTime() => IsRealTimeActive;

    /// <summary>
    /// Test extreme performance with 1 million data points
    /// </summary>
    [RelayCommand]
    private async Task TestMillionPointsAsync()
    {
        const int millionPoints = 1_000_000;

        var result = await _dialogService.ShowConfirmationAsync(
            $"This will generate {millionPoints:N0} data points.\n" +
            "This may take several seconds and use significant memory.\n\n" +
            "Continue?");

        if (result)
        {
            var oldCount = DataPointCount;
            DataPointCount = millionPoints;
            await GenerateDataAsync();
            DataPointCount = oldCount;
        }
    }

    /// <summary>
    /// Clear all data and reset chart
    /// </summary>
    [RelayCommand]
    private async Task ClearDataAsync()
    {
        if (IsRealTimeActive)
        {
            await StopRealTimeAsync();
        }

        ChartControl?.Plot.Clear();
        ChartControl?.Refresh();

        RealTimeData = null;
        _signalPlot = null;
        LastGenerationTime = TimeSpan.Zero;
        FrameRate = 0;

        // Force garbage collection
        GC.Collect();
        MemoryUsage = GC.GetTotalMemory(true);

        _telemetry?.TrackEvent("Chart.Cleared");
    }

    #endregion

    #region Real-Time Updates

    private void UpdateRealTimeData(object? state)
    {
        if (!IsRealTimeActive || RealTimeData == null) return;

        try
        {
            var frameStart = Stopwatch.GetTimestamp();

            // Shift data left and add new point
            Array.Copy(RealTimeData, 1, RealTimeData, 0, RealTimeData.Length - 1);

            // Generate new data point (sine wave with noise)
            var time = DateTime.Now.Ticks / 10_000_000.0; // Convert to seconds
            RealTimeData[^1] = 50 * Math.Sin(time * 2) + 30 * (_random.NextDouble() - 0.5);

            // Update chart on UI thread (non-blocking)
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    ChartControl?.Refresh();
                }
                catch
                {
                    // Ignore refresh errors during shutdown
                }
            }, System.Windows.Threading.DispatcherPriority.Render);

            // Calculate frame rate
            var frameTime = Stopwatch.GetTimestamp() - frameStart;
            var frameTimeMs = frameTime * 1000.0 / Stopwatch.Frequency;
            FrameRate = frameTimeMs > 0 ? 1000.0 / frameTimeMs : 0;

            // Track performance if frame rate drops below 30 FPS
            if (FrameRate < 30)
            {
                _telemetry?.TrackEvent("Performance.LowFrameRate", new Dictionary<string, string>
                {
                    ["FrameRate"] = FrameRate.ToString("F1"),
                    ["FrameTimeMs"] = frameTimeMs.ToString("F2")
                });
            }
        }
        catch (Exception ex)
        {
            _telemetry?.RecordException(ex);
        }
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        _realTimeTimer?.Dispose();
        // WpfPlot does not implement IDisposable in ScottPlot 5.0
        GC.SuppressFinalize(this);
    }

    #endregion
}

/// <summary>
/// Performance statistics for chart operations
/// </summary>
public class ChartPerformanceStats
{
    public int DataPoints { get; set; }
    public TimeSpan GenerationTime { get; set; }
    public double FrameRate { get; set; }
    public long MemoryUsage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public double PointsPerSecond => DataPoints / GenerationTime.TotalSeconds;
    public string MemoryUsageMB => $"{MemoryUsage / 1024.0 / 1024.0:F1} MB";
}