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
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IMessageBus, MessageBus>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IDockingService, DockingService>();
        services.AddSingleton<ExceptionHandler>();

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
        
        // Register Tool ViewModels
        services.AddSingleton<ViewModels.Tools.SolutionExplorerViewModel>();
        services.AddSingleton<ViewModels.Tools.PropertiesViewModel>();
        services.AddSingleton<ViewModels.Tools.OutputViewModel>();
        services.AddSingleton<ViewModels.Tools.ErrorListViewModel>();
        
        // Register Views (Windows)
        services.AddSingleton<MainWindow>();
        
        // Add more services, ViewModels, and Views as needed
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Logger.Info("Application shutting down...");
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}