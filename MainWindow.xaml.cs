using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WPFBase.Interfaces;
using WPFBase.Services;
using WPFBase.ViewModels;
using WPFBase.ViewModels.Documents;

namespace WPFBase;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private IDockingService? _dockingService;
    private IUserSettingsService? _userSettingsService;
    private IKeyboardShortcutService? _keyboardShortcutService;
    private MainViewModel? _viewModel;
    private const string WindowKey = "MainWindow";
    
    public MainWindow(MainViewModel viewModel, IUserSettingsService userSettingsService, IKeyboardShortcutService keyboardShortcutService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _userSettingsService = userSettingsService;
        _keyboardShortcutService = keyboardShortcutService;
        DataContext = viewModel;
        
        // Set up keyboard shortcuts
        SetupKeyboardShortcuts();
        
        // Restore window state
        RestoreWindowState();
        
        // Handle key events for shortcuts
        KeyDown += OnKeyDown;
    }
    
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        
        // Initialize docking service with the DockManager
        _dockingService = App.Current.Services.GetService(typeof(IDockingService)) as IDockingService;
        if (_dockingService is DockingService dockingService)
        {
            dockingService.Initialize(DockManager);
        }
        
        // Load last layout
        _ = _dockingService?.LoadLayoutAsync();
        
        // Show default tools
        ShowDefaultTools();
    }
    
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Save window state
        SaveWindowState();
        
        // Save layout on exit
        _dockingService?.SaveLayoutAsync().Wait();
        
        // Save keyboard shortcuts
        _keyboardShortcutService?.SaveShortcutsAsync().Wait();
        
        // Save user settings
        _userSettingsService?.SaveAsync().Wait();
        
        base.OnClosing(e);
    }
    
    private void SetupKeyboardShortcuts()
    {
        if (_keyboardShortcutService == null || _viewModel == null) return;

        try
        {
            // Register commands with the keyboard shortcut service
            _keyboardShortcutService.RegisterShortcut("New", Key.N, ModifierKeys.Control, _viewModel.NewCommand, "Create new document");
            _keyboardShortcutService.RegisterShortcut("Open", Key.O, ModifierKeys.Control, _viewModel.OpenCommand, "Open file");
            _keyboardShortcutService.RegisterShortcut("Save", Key.S, ModifierKeys.Control, _viewModel.SaveCommand, "Save current document");
            _keyboardShortcutService.RegisterShortcut("ToggleTheme", Key.T, ModifierKeys.Control | ModifierKeys.Shift, _viewModel.ToggleThemeCommand, "Toggle application theme");
            _keyboardShortcutService.RegisterShortcut("Exit", Key.F4, ModifierKeys.Alt, _viewModel.ExitCommand, "Exit application");

            // Traditional command bindings as fallback
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, (s, e) => _viewModel.NewCommand.Execute(null)));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, (s, e) => _viewModel.OpenCommand.Execute(null)));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, (s, e) => _viewModel.SaveCommand.Execute(null)));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting up keyboard shortcuts: {ex}");
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (_keyboardShortcutService == null) return;

        var modifiers = ModifierKeys.None;
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            modifiers |= ModifierKeys.Control;
        if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            modifiers |= ModifierKeys.Alt;
        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            modifiers |= ModifierKeys.Shift;
        if (Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin))
            modifiers |= ModifierKeys.Windows;

        if (_keyboardShortcutService.ExecuteShortcut(e.Key, modifiers))
        {
            e.Handled = true;
        }
    }
    
    private async void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            
            foreach (var file in files)
            {
                await OpenFileAsync(file);
            }
        }
    }
    
    private void Window_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        
        e.Handled = true;
    }
    
    private async Task OpenFileAsync(string filePath)
    {
        if (_dockingService == null) return;
        
        var dialogService = App.Current.Services.GetService(typeof(IDialogService)) as IDialogService;
        var loggingService = App.Current.Services.GetService(typeof(LoggingService)) as LoggingService;
        
        if (dialogService == null || loggingService == null) return;
        
        try
        {
            var document = new TextDocumentViewModel(dialogService, loggingService);
            await document.LoadFromFileAsync(filePath);
            
            await _dockingService.ShowDocumentAsync(document, document.Title);
        }
        catch (Exception ex)
        {
            await dialogService.ShowErrorAsync($"Failed to open file: {filePath}", "Open Error", ex);
        }
    }
    
    private async void ShowDefaultTools()
    {
        if (_dockingService == null) return;
        
        try
        {
            var services = App.Current.Services;
            
            // Show Solution Explorer
            var solutionExplorer = services.GetService(typeof(ViewModels.Tools.SolutionExplorerViewModel));
            if (solutionExplorer != null)
            {
                await _dockingService.ShowToolAsync(solutionExplorer, "Solution Explorer", WPFBase.Interfaces.AnchorableShowStrategy.Left);
            }
            
            // Show Output
            var output = services.GetService(typeof(ViewModels.Tools.OutputViewModel));
            if (output != null)
            {
                await _dockingService.ShowToolAsync(output, "Output", WPFBase.Interfaces.AnchorableShowStrategy.Bottom);
            }
        }
        catch (Exception ex)
        {
            var loggingService = App.Current.Services.GetService(typeof(LoggingService)) as LoggingService;
            loggingService?.LogError(ex, "Failed to show default tools");
        }
    }
    
    private void RestoreWindowState()
    {
        if (_userSettingsService == null) return;
        
        var windowSettings = _userSettingsService.RestoreWindowState(WindowKey);
        if (windowSettings != null)
        {
            // Ensure the window fits on the screen
            var screenBounds = SystemParameters.WorkArea;
            
            WindowState = WindowState.Normal; // Set to normal first to position correctly
            
            Left = Math.Max(screenBounds.Left, Math.Min(windowSettings.Left, screenBounds.Right - windowSettings.Width));
            Top = Math.Max(screenBounds.Top, Math.Min(windowSettings.Top, screenBounds.Bottom - windowSettings.Height));
            Width = Math.Min(windowSettings.Width, screenBounds.Width);
            Height = Math.Min(windowSettings.Height, screenBounds.Height);
            
            // Apply window state after positioning
            if (windowSettings.WindowState != WindowState.Minimized)
            {
                WindowState = windowSettings.WindowState;
            }
        }
    }
    
    private void SaveWindowState()
    {
        if (_userSettingsService == null) return;
        
        _userSettingsService.SaveWindowState(
            WindowKey,
            WindowState,
            RestoreBounds.Left,
            RestoreBounds.Top,
            RestoreBounds.Width,
            RestoreBounds.Height);
    }
}

/// <summary>
/// Layout initializer for AvalonDock
/// </summary>
public class LayoutInitializer : ILayoutUpdateStrategy
{
    public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
    {
        // Determine where to place tools based on their type
        if (anchorableToShow.Content is FrameworkElement fe && fe.DataContext != null)
        {
            var viewModelType = fe.DataContext.GetType().Name;
            
            LayoutAnchorablePane? targetPane = null;
            
            // Place in appropriate pane based on tool type
            if (viewModelType.Contains("SolutionExplorer") || viewModelType.Contains("Explorer"))
            {
                targetPane = layout.Descendents().OfType<LayoutAnchorablePane>()
                    .FirstOrDefault(p => p.Name == "LeftTools");
            }
            else if (viewModelType.Contains("Properties"))
            {
                targetPane = layout.Descendents().OfType<LayoutAnchorablePane>()
                    .FirstOrDefault(p => p.Name == "RightTools");
            }
            else if (viewModelType.Contains("Output") || viewModelType.Contains("Error"))
            {
                targetPane = layout.Descendents().OfType<LayoutAnchorablePane>()
                    .FirstOrDefault(p => p.Name == "BottomTools");
            }
            
            if (targetPane != null)
            {
                targetPane.Children.Add(anchorableToShow);
                return true;
            }
        }
        
        return false;
    }
    
    public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
    {
    }
    
    public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument documentToShow, ILayoutContainer destinationContainer)
    {
        return false;
    }
    
    public void AfterInsertDocument(LayoutRoot layout, LayoutDocument document)
    {
    }
}