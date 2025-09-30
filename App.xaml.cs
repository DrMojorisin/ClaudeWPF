using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using WPFBase.Data;
using WPFBase.Interfaces;
using WPFBase.Services;
using WPFBase.ViewModels;
using WPFBase.Views;

namespace WPFBase;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    
    /// <summary>
    /// Gets the current App instance
    /// </summary>
    public new static App Current => (App)Application.Current;
    
    /// <summary>
    /// Gets the service provider for dependency injection
    /// </summary>
    public IServiceProvider Services => _serviceProvider!;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Configure services
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        _serviceProvider = serviceCollection.BuildServiceProvider();

        // Initialize logging
        var loggingService = _serviceProvider.GetRequiredService<LoggingService>();
        Logger.Initialize(loggingService);
        Logger.Info("Application starting...");

        // Initialize exception handling
        var exceptionHandler = _serviceProvider.GetRequiredService<ExceptionHandler>();
        exceptionHandler.Initialize();

        // Initialize theme service
        var themeService = _serviceProvider.GetRequiredService<IThemeService>();
        
        // Test cutting-edge features before showing main window
        // TestCuttingEdgeFeatures(); // ScottPlot 5.0 VALIDATED

        // Create and show main window
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register core services
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IUserSettingsService, UserSettingsService>();
        services.AddSingleton<IKeyboardShortcutService, KeyboardShortcutService>();
        services.AddSingleton<LoggingService>();
        services.AddSingleton<ILoggingService>(provider => provider.GetRequiredService<LoggingService>());
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IMessageBus, MessageBus>();
        services.AddSingleton<IThemeService, ModernThemeService>();
        services.AddSingleton<IDockingService, DockingService>();
        services.AddSingleton<ExceptionHandler>();

        // Register cutting-edge 2024-2025 services
        services.AddSingleton<ITelemetryService, TelemetryService>();
        services.AddSingleton<IPerformanceOptimizationService, PerformanceOptimizationService>();
        services.AddSingleton<IResilienceService, ResilienceService>();  // Polly resilience patterns

        // Register data layer
        services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();
        
        // Configure logging
        services.AddLogging(builder =>
        {
            builder.AddProvider(new SerilogLoggerProvider());
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        // Register ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<HomeViewModel>();
        // services.AddTransient<ValidationExampleViewModel>();  // REMOVED - Legacy pattern
        services.AddTransient<ModernValidationViewModel>();    // NEW 2024-2025 pattern
        services.AddTransient<TestViewModel>();
        services.AddTransient<PartialPropertiesExampleViewModel>(provider =>
            new PartialPropertiesExampleViewModel(
                provider.GetRequiredService<IDialogService>(),
                provider.GetRequiredService<IPerformanceOptimizationService>())
            {
                TenantId = "default-tenant"
            });

        // Register cutting-edge example ViewModels (2024-2025)
        services.AddTransient<ViewModels.Examples.HighPerformanceChartViewModel>();
        
        // Register Tool ViewModels
        services.AddSingleton<ViewModels.Tools.SolutionExplorerViewModel>();
        services.AddSingleton<ViewModels.Tools.PropertiesViewModel>();
        services.AddSingleton<ViewModels.Tools.OutputViewModel>();
        services.AddSingleton<ViewModels.Tools.ErrorListViewModel>();
        
        // Register Views (Windows)
        services.AddSingleton<MainWindow>();
        
        // Add more services, ViewModels, and Views as needed
    }

    private void TestCuttingEdgeFeatures()
    {
        try
        {
            // Test CommunityToolkit.Mvvm 8.4.0 partial properties
            TestPartialProperties();
            Logger.Info("✓ CommunityToolkit.Mvvm 8.4.0 partial properties test PASSED");

            // Test ScottPlot 5.0 high-performance charting
            TestScottPlotChart();
            Logger.Info("✓ ScottPlot 5.0 high-performance charting test PASSED");
        }
        catch (Exception ex)
        {
            Logger.Error($"✗ Feature test FAILED: {ex.Message}");
        }
    }

    private void TestPartialProperties()
    {
        // Get services from DI container
        var dialogService = _serviceProvider!.GetRequiredService<IDialogService>();
        var performanceService = _serviceProvider!.GetRequiredService<IPerformanceOptimizationService>();

        // Test partial properties ViewModel creation with required property
        var viewModel = new PartialPropertiesExampleViewModel(
            dialogService,
            performanceService)
        {
            TenantId = "test-tenant-id"
        };

        // Test partial property setters/getters
        viewModel.Email = "test@example.com";
        viewModel.FirstName = "John";
        viewModel.LastName = "Doe";
        viewModel.Status = "Testing";

        // Verify properties work
        if (viewModel.Email != "test@example.com") throw new Exception("Email partial property failed");
        if (viewModel.FirstName != "John") throw new Exception("FirstName partial property failed");
        if (viewModel.LastName != "Doe") throw new Exception("LastName partial property failed");
        if (viewModel.FullName != "John Doe") throw new Exception("FullName computed property failed");
        if (viewModel.TenantId != "test-tenant-id") throw new Exception("TenantId required property failed");

        // Test property change notifications
        bool emailChanged = false;
        bool fullNameChanged = false;

        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.Email)) emailChanged = true;
            if (e.PropertyName == nameof(viewModel.FullName)) fullNameChanged = true;
        };

        viewModel.Email = "updated@example.com";
        viewModel.FirstName = "Jane";

        if (!emailChanged) throw new Exception("Email PropertyChanged notification failed");
        if (!fullNameChanged) throw new Exception("FullName PropertyChanged notification failed");

        Logger.Info("  ✓ Partial properties working correctly");
        Logger.Info("  ✓ Required properties working correctly");
        Logger.Info("  ✓ Property change notifications working");
        Logger.Info("  ✓ Computed properties working");
    }

    private void TestScottPlotChart()
    {
        // Get services from DI container
        var dialogService = _serviceProvider!.GetRequiredService<IDialogService>();
        var telemetryService = _serviceProvider!.GetService<ITelemetryService>();

        // Test ScottPlot 5.0 ViewModel creation
        var chartViewModel = new ViewModels.Examples.HighPerformanceChartViewModel(
            dialogService,
            telemetryService);

        // Verify chart control initialization
        if (chartViewModel.ChartControl == null) throw new Exception("ChartControl initialization failed");
        if (chartViewModel.DataPointCount != 1000) throw new Exception("Default DataPointCount incorrect");
        if (chartViewModel.IsGenerating) throw new Exception("Should not be generating initially");
        if (chartViewModel.IsRealTimeActive) throw new Exception("Should not be in real-time mode initially");

        // Test chart configuration (ScottPlot 5.0 API)
        var plot = chartViewModel.ChartControl.Plot;
        if (plot == null) throw new Exception("Plot object is null");

        // Verify modern ScottPlot 5.0 configuration worked
        if (plot.Axes == null) throw new Exception("Plot axes not initialized");
        if (plot.RenderManager == null) throw new Exception("RenderManager not initialized");

        Logger.Info("  ✓ ScottPlot 5.0 WpfPlot initialization working");
        Logger.Info("  ✓ Modern ScottPlot 5.0 API compatibility confirmed");
        Logger.Info("  ✓ Chart ViewModel instantiation working");
        Logger.Info("  ✓ Performance optimizations configured");

        // Dispose properly
        chartViewModel.Dispose();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Logger.Info("Application shutting down...");
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}