using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using WPFBase.Interfaces;

namespace WPFBase.Services;

/// <summary>
/// Implementation of docking service using AvalonDock
/// </summary>
public class DockingService : IDockingService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfigurationService _configurationService;
    private readonly LoggingService _loggingService;
    private readonly IMessageBus _messageBus;
    private DockingManager? _dockingManager;
    private readonly string _layoutsPath;
    private readonly Dictionary<string, Type> _contentRegistry = new();

    public DockingService(
        IServiceProvider serviceProvider,
        IConfigurationService configurationService,
        LoggingService loggingService,
        IMessageBus messageBus)
    {
        _serviceProvider = serviceProvider;
        _configurationService = configurationService;
        _loggingService = loggingService;
        _messageBus = messageBus;
        
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _layoutsPath = Path.Combine(appDataPath, "WPFBase", "Layouts");
        Directory.CreateDirectory(_layoutsPath);
        
        RegisterContentTypes();
    }

    public DockingManager? DockingManager => _dockingManager;

    public event EventHandler<DocumentChangedEventArgs>? ActiveDocumentChanged;

    public void Initialize(DockingManager dockingManager)
    {
        _dockingManager = dockingManager;
        
        // Subscribe to docking manager events
        _dockingManager.ActiveContentChanged += OnActiveContentChanged;
        _dockingManager.DocumentClosing += OnDocumentClosing;
        
        // Set up layout update strategy
        var serializer = new XmlLayoutSerializer(_dockingManager);
        serializer.LayoutSerializationCallback += OnLayoutDeserialization;
        
        _loggingService.LogInformation("Docking service initialized");
    }

    public async Task<LayoutDocument> ShowDocumentAsync<TViewModel>(TViewModel viewModel, string title = "") 
        where TViewModel : class
    {
        if (_dockingManager == null)
            throw new InvalidOperationException("DockingManager not initialized");

        // Check if document already exists
        var existing = FindDocument(viewModel);
        if (existing != null)
        {
            existing.IsActive = true;
            return existing;
        }

        // Create view for the view model
        var view = CreateView(viewModel);
        
        // Create layout document
        var document = new LayoutDocument
        {
            Content = view,
            Title = title
        };

        // Set content ID if view model implements IDockableViewModel
        if (viewModel is IDockableViewModel dockable)
        {
            document.ContentId = dockable.ContentId;
            document.Title = string.IsNullOrEmpty(title) ? dockable.Title : title;
            document.CanClose = dockable.CanClose;
        }

        // Add to document pane
        var documentPane = _dockingManager.Layout.Descendents()
            .OfType<LayoutDocumentPane>()
            .FirstOrDefault();

        if (documentPane == null)
        {
            documentPane = new LayoutDocumentPane();
            if (_dockingManager.Layout.RootPanel == null)
            {
                _dockingManager.Layout.RootPanel = new LayoutPanel(documentPane);
            }
            else
            {
                _dockingManager.Layout.RootPanel.Children.Add(documentPane);
            }
        }

        documentPane.Children.Add(document);
        document.IsActive = true;

        _loggingService.LogInformation("Opened document: {Title}", document.Title);
        
        return await Task.FromResult(document);
    }

    public async Task<LayoutAnchorable> ShowToolAsync<TViewModel>(TViewModel viewModel, string title = "", Interfaces.AnchorableShowStrategy strategy = Interfaces.AnchorableShowStrategy.Right) 
        where TViewModel : class
    {
        if (_dockingManager == null)
            throw new InvalidOperationException("DockingManager not initialized");

        // Check if tool already exists
        var existing = FindTool(viewModel);
        if (existing != null)
        {
            existing.IsActive = true;
            existing.IsVisible = true;
            return existing;
        }

        // Create view for the view model
        var view = CreateView(viewModel);
        
        // Create layout anchorable
        var tool = new LayoutAnchorable
        {
            Content = view,
            Title = title
        };

        // Set properties if view model implements IToolViewModel
        if (viewModel is IToolViewModel toolViewModel)
        {
            tool.ContentId = toolViewModel.ContentId;
            tool.Title = string.IsNullOrEmpty(title) ? toolViewModel.Title : title;
            tool.CanClose = toolViewModel.CanClose;
            tool.AutoHideWidth = toolViewModel.PreferredWidth;
            tool.AutoHideHeight = toolViewModel.PreferredHeight;
        }

        // Add to appropriate pane based on strategy
        AddToolToLayout(tool, strategy);
        
        tool.IsActive = true;
        tool.IsVisible = true;

        _loggingService.LogInformation("Opened tool: {Title}", tool.Title);
        
        return await Task.FromResult(tool);
    }

    public async Task<bool> CloseDocumentAsync(LayoutDocument document)
    {
        if (document.Content is UserControl view && view.DataContext is IDockableViewModel dockable)
        {
            if (!await dockable.OnClosingAsync())
            {
                return false;
            }
        }

        document.Close();
        return true;
    }

    public void CloseTool(LayoutAnchorable tool)
    {
        tool.Hide();
    }

    public LayoutDocument? FindDocument<TViewModel>(TViewModel viewModel) where TViewModel : class
    {
        return _dockingManager?.Layout.Descendents()
            .OfType<LayoutDocument>()
            .FirstOrDefault(d => d.Content is UserControl view && view.DataContext == viewModel);
    }

    public LayoutAnchorable? FindTool<TViewModel>(TViewModel viewModel) where TViewModel : class
    {
        return _dockingManager?.Layout.Descendents()
            .OfType<LayoutAnchorable>()
            .FirstOrDefault(a => a.Content is UserControl view && view.DataContext == viewModel);
    }

    public async Task SaveLayoutAsync(string fileName = "")
    {
        if (_dockingManager == null) return;

        try
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = Path.Combine(_layoutsPath, "default.layout");
            }
            else if (!Path.IsPathRooted(fileName))
            {
                fileName = Path.Combine(_layoutsPath, fileName);
            }

            var serializer = new XmlLayoutSerializer(_dockingManager);
            using (var stream = new FileStream(fileName, FileMode.Create))
            {
                serializer.Serialize(stream);
            }

            _loggingService.LogInformation("Layout saved to {FileName}", fileName);
            
            // Save to configuration
            _configurationService.SetValue("Layout:LastSaved", fileName);
            await _configurationService.SaveAsync();
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to save layout");
            throw;
        }
    }

    public async Task LoadLayoutAsync(string fileName = "")
    {
        if (_dockingManager == null) return;

        try
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = _configurationService.GetValue<string>("Layout:LastSaved") 
                    ?? Path.Combine(_layoutsPath, "default.layout");
            }
            else if (!Path.IsPathRooted(fileName))
            {
                fileName = Path.Combine(_layoutsPath, fileName);
            }

            if (!File.Exists(fileName))
            {
                _loggingService.LogWarning("Layout file not found: {FileName}", fileName);
                return;
            }

            var serializer = new XmlLayoutSerializer(_dockingManager);
            serializer.LayoutSerializationCallback += OnLayoutDeserialization;
            
            using (var stream = new FileStream(fileName, FileMode.Open))
            {
                serializer.Deserialize(stream);
            }

            _loggingService.LogInformation("Layout loaded from {FileName}", fileName);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to load layout");
            ResetLayout();
        }
        
        await Task.CompletedTask;
    }

    public void ResetLayout()
    {
        if (_dockingManager == null) return;

        // Clear existing layout
        _dockingManager.Layout.RootPanel = null;

        // Create default layout
        var rootPanel = new LayoutPanel();
        rootPanel.Orientation = Orientation.Horizontal;
        
        // Left side - tools
        var leftPane = new LayoutAnchorablePane();
        
        // Center - documents
        var documentPane = new LayoutDocumentPane();
        rootPanel.Children.Add(documentPane);
        
        // Right side - tools
        var rightPane = new LayoutAnchorablePane();
        rootPanel.Children.Add(rightPane);
        
        // Bottom - output/errors
        var bottomPane = new LayoutAnchorablePane();
        rootPanel.Children.Add(bottomPane);

        _dockingManager.Layout.RootPanel = rootPanel;
        
        _loggingService.LogInformation("Layout reset to default");
    }

    public IEnumerable<LayoutDocument> GetDocuments()
    {
        return _dockingManager?.Layout.Descendents()
            .OfType<LayoutDocument>() 
            ?? Enumerable.Empty<LayoutDocument>();
    }

    public IEnumerable<LayoutAnchorable> GetTools()
    {
        return _dockingManager?.Layout.Descendents()
            .OfType<LayoutAnchorable>() 
            ?? Enumerable.Empty<LayoutAnchorable>();
    }

    private UserControl CreateView(object viewModel)
    {
        // Try to find registered view type
        var viewModelType = viewModel.GetType();
        var viewTypeName = viewModelType.Name.Replace("ViewModel", "View");
        var viewType = Type.GetType($"WPFBase.Views.{viewTypeName}") 
            ?? Type.GetType($"WPFBase.Views.Documents.{viewTypeName}")
            ?? Type.GetType($"WPFBase.Views.Tools.{viewTypeName}");

        UserControl? view = null;
        
        if (viewType != null)
        {
            view = Activator.CreateInstance(viewType) as UserControl;
        }
        
        // Fallback to generic content presenter
        if (view == null)
        {
            view = new UserControl { Content = viewModel };
        }

        view.DataContext = viewModel;
        return view;
    }

    private void AddToolToLayout(LayoutAnchorable tool, Interfaces.AnchorableShowStrategy strategy)
    {
        if (_dockingManager == null) return;

        LayoutAnchorablePane? targetPane = null;
        
        switch (strategy)
        {
            case Interfaces.AnchorableShowStrategy.Left:
                targetPane = GetOrCreateAnchorablePane(AnchorSide.Left);
                break;
                
            case Interfaces.AnchorableShowStrategy.Right:
                targetPane = GetOrCreateAnchorablePane(AnchorSide.Right);
                break;
                
            case Interfaces.AnchorableShowStrategy.Top:
                targetPane = GetOrCreateAnchorablePane(AnchorSide.Top);
                break;
                
            case Interfaces.AnchorableShowStrategy.Bottom:
                targetPane = GetOrCreateAnchorablePane(AnchorSide.Bottom);
                break;
                
            case Interfaces.AnchorableShowStrategy.Float:
                tool.Float();
                return;
        }

        targetPane?.Children.Add(tool);
    }

    private LayoutAnchorablePane GetOrCreateAnchorablePane(AnchorSide side)
    {
        if (_dockingManager == null) 
            throw new InvalidOperationException("DockingManager not initialized");

        var existingPane = _dockingManager.Layout.Descendents()
            .OfType<LayoutAnchorablePane>()
            .FirstOrDefault();

        if (existingPane != null)
            return existingPane;

        var newPane = new LayoutAnchorablePane();
        
        if (_dockingManager.Layout.RootPanel == null)
        {
            _dockingManager.Layout.RootPanel = new LayoutPanel(newPane);
        }
        else
        {
            _dockingManager.Layout.RootPanel.Children.Add(newPane);
        }

        return newPane;
    }

    private void OnActiveContentChanged(object? sender, EventArgs e)
    {
        if (_dockingManager == null) return;

        var oldDocument = ActiveDocumentChanged != null 
            ? GetDocuments().FirstOrDefault(d => d.IsActive) 
            : null;
            
        var newDocument = _dockingManager.ActiveContent as LayoutDocument;

        if (newDocument?.Content is UserControl view && view.DataContext is IDockableViewModel dockable)
        {
            dockable.OnActivated();
        }

        ActiveDocumentChanged?.Invoke(this, new DocumentChangedEventArgs(oldDocument, newDocument));
    }

    private void OnDocumentClosing(object? sender, DocumentClosingEventArgs e)
    {
        if (e.Document.Content is UserControl view && view.DataContext is IDockableViewModel dockable)
        {
            if (!dockable.OnClosingAsync().GetAwaiter().GetResult())
            {
                e.Cancel = true;
            }
        }
    }

    private void OnLayoutDeserialization(object? sender, LayoutSerializationCallbackEventArgs e)
    {
        // Restore content based on ContentId
        if (!string.IsNullOrEmpty(e.Model.ContentId) && _contentRegistry.TryGetValue(e.Model.ContentId, out var viewModelType))
        {
            try
            {
                var viewModel = _serviceProvider.GetService(viewModelType);
                if (viewModel != null)
                {
                    e.Content = CreateView(viewModel);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Failed to restore content: {ContentId}", e.Model.ContentId);
            }
        }
    }

    private void RegisterContentTypes()
    {
        // Register known content types for layout persistence
        // This should be expanded as new tools/documents are added
        _contentRegistry["SolutionExplorer"] = typeof(ViewModels.Tools.SolutionExplorerViewModel);
        _contentRegistry["Properties"] = typeof(ViewModels.Tools.PropertiesViewModel);
        _contentRegistry["Output"] = typeof(ViewModels.Tools.OutputViewModel);
        _contentRegistry["ErrorList"] = typeof(ViewModels.Tools.ErrorListViewModel);
    }
}

public enum AnchorSide
{
    Left,
    Right,
    Top,
    Bottom
}