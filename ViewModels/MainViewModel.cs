using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Windows;
using System.Windows.Controls;
// using WPFBase.Commands; // Removed - using CommunityToolkit.Mvvm instead
using WPFBase.Interfaces;
using WPFBase.Services;
using WPFBase.Models.Messages;

namespace WPFBase.ViewModels;

/// <summary>
/// Main window ViewModel - handles the application shell and navigation
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IThemeService _themeService;
    private readonly IMessageBus _messageBus;
    private readonly IUserSettingsService _userSettingsService;
    private readonly ExceptionHandler _exceptionHandler;
    private IDisposable? _statusMessageSubscription;
    private IDisposable? _progressMessageSubscription;

    [ObservableProperty]
    private UserControl? currentView;

    [ObservableProperty]
    private string applicationTitle = "WPF MVVM Application";

    [ObservableProperty]
    private string statusText = "Ready";

    [ObservableProperty]
    private double progressValue;

    [ObservableProperty]
    private Visibility progressVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private string currentThemeName = "Light";

    [ObservableProperty]
    private bool canGoBack;

    [ObservableProperty]
    private List<RecentFile> recentFiles = new();

    public MainViewModel(
        INavigationService navigationService,
        IDialogService dialogService,
        IThemeService themeService,
        IMessageBus messageBus,
        IUserSettingsService userSettingsService,
        ExceptionHandler exceptionHandler)
    {
        _navigationService = navigationService;
        _dialogService = dialogService;
        _themeService = themeService;
        _messageBus = messageBus;
        _userSettingsService = userSettingsService;
        _exceptionHandler = exceptionHandler;
        
        _navigationService.Navigated += OnNavigated;
        _themeService.ThemeChanged += OnThemeChanged;
        
        // Subscribe to messages
        _statusMessageSubscription = _messageBus.Subscribe<StatusMessage>(OnStatusMessage);
        _progressMessageSubscription = _messageBus.Subscribe<ProgressMessage>(OnProgressMessage);
        
        Title = "Main Window";
        CurrentThemeName = _themeService.CurrentTheme.Name;
        
        // Load recent files
        RefreshRecentFiles();
        
        // Navigate to home view on startup
        _ = NavigateToHomeAsync();
    }

    private void OnNavigated(object? sender, NavigationEventArgs e)
    {
        CurrentView = _navigationService.CurrentView;
        CanGoBack = _navigationService.CanGoBack;
        GoBackCommand.NotifyCanExecuteChanged();
    }

    private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        CurrentThemeName = e.NewTheme.Name;
    }

    private void OnStatusMessage(StatusMessage message)
    {
        StatusText = message.Text;
        
        // Auto-clear status after duration
        if (message.Duration.HasValue)
        {
            Task.Delay(message.Duration.Value).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() => StatusText = "Ready");
            });
        }
    }

    private void OnProgressMessage(ProgressMessage message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (message.IsCompleted)
            {
                ProgressVisibility = Visibility.Collapsed;
                StatusText = "Ready";
            }
            else
            {
                ProgressVisibility = Visibility.Visible;
                ProgressValue = message.Progress;
                StatusText = message.StatusText ?? message.OperationName;
            }
        });
    }

    [RelayCommand]
    private async Task NavigateToHomeAsync()
    {
        await _navigationService.NavigateToAsync<HomeViewModel>();
    }

    [RelayCommand]
    private async Task NavigateToSettingsAsync()
    {
        // Example of navigation to another view
        // await _navigationService.NavigateToAsync<SettingsViewModel>();
        await _dialogService.ShowMessageAsync("Settings view not yet implemented", "Coming Soon");
    }

    [RelayCommand]
    private async Task ToggleThemeAsync()
    {
        await _themeService.ToggleThemeAsync();
        _messageBus.PublishStatus($"Theme changed to {_themeService.CurrentTheme.Name}", StatusMessageType.Success);
    }

    [RelayCommand]
    private async Task ShowAboutAsync()
    {
        var message = "WPF MVVM Application\nVersion 1.0.0\n\nBuilt with:\n• .NET 9.0\n• CommunityToolkit.Mvvm\n• Enhanced MVVM Framework";
        await _dialogService.ShowMessageAsync(message, "About", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // BEST PRACTICE: Use [IncludeCancelCommand] for cancellable long-running operations
    [RelayCommand(IncludeCancelCommand = true)]
    private async Task PerformLongOperationAsync(CancellationToken cancellationToken)
    {
        try
        {
            _messageBus.PublishStatus("Starting long operation...", StatusMessageType.Information);

            for (int i = 0; i <= 100; i += 10)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _messageBus.PublishProgress("Processing", i / 100.0, $"Processing... {i}%");
                await Task.Delay(500, cancellationToken);
            }

            _messageBus.PublishStatus("Long operation completed!", StatusMessageType.Success);
        }
        catch (OperationCanceledException)
        {
            _messageBus.PublishStatus("Operation cancelled", StatusMessageType.Warning);
        }
        finally
        {
            _messageBus.PublishProgress("", 0, ""); // Clear progress
        }
    }

    [RelayCommand]
    private async Task TestExceptionAsync()
    {
        try
        {
            throw new InvalidOperationException("This is a test exception to demonstrate error handling");
        }
        catch (Exception ex)
        {
            await _exceptionHandler.HandleExceptionAsync(ex, "Test Command", false);
        }
    }

    [RelayCommand]
    private async Task NewAsync()
    {
        await _dialogService.ShowMessageAsync("New file functionality", "New");
    }

    [RelayCommand]
    private async Task OpenAsync()
    {
        var filePath = await _dialogService.ShowOpenFileDialogAsync("All Files|*.*|Text Files|*.txt");
        if (!string.IsNullOrEmpty(filePath))
        {
            await _dialogService.ShowMessageAsync($"Selected file: {filePath}", "Open File");
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var filePath = await _dialogService.ShowSaveFileDialogAsync("Text Files|*.txt|All Files|*.*");
        if (!string.IsNullOrEmpty(filePath))
        {
            await _dialogService.ShowMessageAsync($"Save to: {filePath}", "Save File");
        }
    }

    [RelayCommand]
    private void Exit()
    {
        Application.Current.Shutdown();
    }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private async Task GoBackAsync()
    {
        await _navigationService.GoBackAsync();
    }

    [RelayCommand]
    private async Task OpenRecentFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            await _dialogService.ShowErrorAsync($"File not found: {filePath}", "Open Recent File");
            _userSettingsService.RemoveRecentFile(filePath);
            RefreshRecentFiles();
            return;
        }

        try
        {
            // Add to recent files
            _userSettingsService.AddRecentFile(filePath);
            RefreshRecentFiles();
            
            // Open the file (this would typically be handled by the docking service)
            await _dialogService.ShowMessageAsync($"Opening file: {filePath}", "Open Recent File");
        }
        catch (Exception ex)
        {
            await _exceptionHandler.HandleExceptionAsync(ex, "Open Recent File", false);
        }
    }

    [RelayCommand]
    private void ClearRecentFiles()
    {
        _userSettingsService.ClearRecentFiles();
        RefreshRecentFiles();
        _messageBus.PublishStatus("Recent files cleared", StatusMessageType.Success);
    }

    private void RefreshRecentFiles()
    {
        RecentFiles = _userSettingsService.GetRecentFiles();
        OnPropertyChanged(nameof(RecentFiles));
    }

    protected override void OnDispose()
    {
        _navigationService.Navigated -= OnNavigated;
        _themeService.ThemeChanged -= OnThemeChanged;
        _statusMessageSubscription?.Dispose();
        _progressMessageSubscription?.Dispose();
        base.Dispose();
    }
}