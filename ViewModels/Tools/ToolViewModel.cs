using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WPFBase.Interfaces;
using WPFBase.Services;
using WPFBase.Models.Messages;

namespace WPFBase.ViewModels.Tools;

/// <summary>
/// Base class for tool window view models
/// </summary>
public abstract partial class ToolViewModel : DockableViewModelBase, IToolViewModel
{
    [ObservableProperty]
    private bool isVisible = true;
    
    [ObservableProperty]
    private bool canHide = true;
    
    private bool _canClose = true;
    public new bool CanClose 
    { 
        get => _canClose;
        set => SetProperty(ref _canClose, value);
    }
    
    public virtual double PreferredWidth => 250;
    
    public virtual double PreferredHeight => 200;
    
    public virtual Interfaces.ToolDockPosition PreferredPosition => Interfaces.ToolDockPosition.Left;
}

/// <summary>
/// Solution Explorer tool window
/// </summary>
public partial class SolutionExplorerViewModel : ToolViewModel
{
    private readonly IDockingService _dockingService;
    private readonly IDialogService _dialogService;
    
    [ObservableProperty]
    private List<FileItem> files = new();
    
    [ObservableProperty]
    private FileItem? selectedFile;

    public SolutionExplorerViewModel(IDockingService dockingService, IDialogService dialogService)
    {
        _dockingService = dockingService;
        _dialogService = dialogService;
        Title = "Solution Explorer";
        ContentId = "SolutionExplorer";
        LoadSampleFiles();
    }

    
    private void LoadSampleFiles()
    {
        Files = new List<FileItem>
        {
            new FileItem { Name = "App.xaml", Icon = "📄", IsFile = true },
            new FileItem { Name = "App.xaml.cs", Icon = "📄", IsFile = true },
            new FileItem { Name = "MainWindow.xaml", Icon = "📄", IsFile = true },
            new FileItem { Name = "ViewModels", Icon = "📁", IsFile = false },
            new FileItem { Name = "Views", Icon = "📁", IsFile = false },
            new FileItem { Name = "Services", Icon = "📁", IsFile = false },
        };
    }
    
    partial void OnSelectedFileChanged(FileItem? value)
    {
        if (value?.IsFile == true)
        {
            // Open file in document
            _ = OpenFileAsync(value);
        }
    }
    
    private async Task OpenFileAsync(FileItem file)
    {
        var document = new Documents.TextDocumentViewModel(
            _dialogService,
            App.Current.Services.GetService(typeof(LoggingService)) as LoggingService);
        
        document.Title = file.Name;
        document.Content = $"// Content of {file.Name}";
        
        await _dockingService.ShowDocumentAsync(document, file.Name);
    }
}

/// <summary>
/// Properties tool window
/// </summary>
public partial class PropertiesViewModel : ToolViewModel
{
    [ObservableProperty]
    private object? selectedObject;
    
    [ObservableProperty]
    private string objectType = "None";
    
    public PropertiesViewModel()
    {
        Title = "Properties";
        ContentId = "Properties";
    }
    
    
    public override double PreferredWidth => 300;
    
    partial void OnSelectedObjectChanged(object? value)
    {
        ObjectType = value?.GetType().Name ?? "None";
    }
}

/// <summary>
/// Output tool window
/// </summary>
public partial class OutputViewModel : ToolViewModel
{
    private readonly IMessageBus _messageBus;
    
    [ObservableProperty]
    private string outputText = string.Empty;
    
    [ObservableProperty]
    private bool autoScroll = true;
    
    public OutputViewModel(IMessageBus messageBus)
    {
        _messageBus = messageBus;
        Title = "Output";
        ContentId = "Output";
        
        // Subscribe to status messages
        _messageBus.Subscribe<StatusMessage>(OnStatusMessage);
    }
    
    
    public override double PreferredHeight => 150;
    
    public void WriteLine(string text)
    {
        OutputText += $"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}";
    }
    
    public void Clear()
    {
        OutputText = string.Empty;
    }
    
    private void OnStatusMessage(StatusMessage message)
    {
        WriteLine($"[{message.Type}] {message.Text}");
    }
}

/// <summary>
/// Error List tool window
/// </summary>
public partial class ErrorListViewModel : ToolViewModel
{
    [ObservableProperty]
    private ObservableCollection<ErrorItem> errors = new();
    
    [ObservableProperty]
    private int errorCount;
    
    [ObservableProperty]
    private int warningCount;
    
    [ObservableProperty]
    private int infoCount;
    
    public ErrorListViewModel()
    {
        Title = "Error List";
        ContentId = "ErrorList";
    }
    
    
    public override double PreferredHeight => 150;
    
    public void AddError(string description, string file = "", int line = 0)
    {
        Errors.Add(new ErrorItem 
        { 
            Type = ErrorType.Error, 
            Description = description, 
            File = file, 
            Line = line 
        });
        ErrorCount++;
    }
    
    public void AddWarning(string description, string file = "", int line = 0)
    {
        Errors.Add(new ErrorItem 
        { 
            Type = ErrorType.Warning, 
            Description = description, 
            File = file, 
            Line = line 
        });
        WarningCount++;
    }
    
    public void Clear()
    {
        Errors.Clear();
        ErrorCount = 0;
        WarningCount = 0;
        InfoCount = 0;
    }
}

/// <summary>
/// File item for solution explorer
/// </summary>
public class FileItem
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsFile { get; set; }
    public List<FileItem> Children { get; set; } = new();
}

/// <summary>
/// Error item for error list
/// </summary>
public class ErrorItem
{
    public ErrorType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;
    public int Line { get; set; }
    public int Column { get; set; }
}

public enum ErrorType
{
    Error,
    Warning,
    Info
}