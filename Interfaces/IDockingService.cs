using AvalonDock.Layout;
using System.Windows.Controls;

namespace WPFBase.Interfaces;

/// <summary>
/// Service for managing docking layouts and windows
/// </summary>
public interface IDockingService
{
    /// <summary>
    /// Gets the docking manager
    /// </summary>
    AvalonDock.DockingManager? DockingManager { get; }
    
    /// <summary>
    /// Show a document
    /// </summary>
    Task<LayoutDocument> ShowDocumentAsync<TViewModel>(TViewModel viewModel, string title = "") where TViewModel : class;
    
    /// <summary>
    /// Show a tool window
    /// </summary>
    Task<LayoutAnchorable> ShowToolAsync<TViewModel>(TViewModel viewModel, string title = "", AnchorableShowStrategy strategy = AnchorableShowStrategy.Right) where TViewModel : class;
    
    /// <summary>
    /// Close a document
    /// </summary>
    Task<bool> CloseDocumentAsync(LayoutDocument document);
    
    /// <summary>
    /// Close a tool window
    /// </summary>
    void CloseTool(LayoutAnchorable tool);
    
    /// <summary>
    /// Find document by view model
    /// </summary>
    LayoutDocument? FindDocument<TViewModel>(TViewModel viewModel) where TViewModel : class;
    
    /// <summary>
    /// Find tool by view model
    /// </summary>
    LayoutAnchorable? FindTool<TViewModel>(TViewModel viewModel) where TViewModel : class;
    
    /// <summary>
    /// Save layout to file
    /// </summary>
    Task SaveLayoutAsync(string fileName = "");
    
    /// <summary>
    /// Load layout from file
    /// </summary>
    Task LoadLayoutAsync(string fileName = "");
    
    /// <summary>
    /// Reset to default layout
    /// </summary>
    void ResetLayout();
    
    /// <summary>
    /// Get all open documents
    /// </summary>
    IEnumerable<LayoutDocument> GetDocuments();
    
    /// <summary>
    /// Get all tools
    /// </summary>
    IEnumerable<LayoutAnchorable> GetTools();
    
    /// <summary>
    /// Event raised when active document changes
    /// </summary>
    event EventHandler<DocumentChangedEventArgs>? ActiveDocumentChanged;
}

/// <summary>
/// Document changed event arguments
/// </summary>
public class DocumentChangedEventArgs : EventArgs
{
    public LayoutDocument? OldDocument { get; }
    public LayoutDocument? NewDocument { get; }
    
    public DocumentChangedEventArgs(LayoutDocument? oldDocument, LayoutDocument? newDocument)
    {
        OldDocument = oldDocument;
        NewDocument = newDocument;
    }
}

/// <summary>
/// Strategy for showing anchorable windows
/// </summary>
public enum AnchorableShowStrategy
{
    Left,
    Right,
    Top,
    Bottom,
    Float
}

/// <summary>
/// Interface for dockable content view models
/// </summary>
public interface IDockableViewModel
{
    /// <summary>
    /// Gets or sets the content ID for layout persistence
    /// </summary>
    string ContentId { get; }
    
    /// <summary>
    /// Gets the title
    /// </summary>
    string Title { get; }
    
    /// <summary>
    /// Gets whether the content can be closed
    /// </summary>
    bool CanClose { get; }
    
    /// <summary>
    /// Called when closing
    /// </summary>
    Task<bool> OnClosingAsync();
    
    /// <summary>
    /// Called when activated
    /// </summary>
    void OnActivated();
    
    /// <summary>
    /// Called when deactivated
    /// </summary>
    void OnDeactivated();
}

/// <summary>
/// Interface for document view models
/// </summary>
public interface IDocumentViewModel : IDockableViewModel
{
    /// <summary>
    /// Gets whether the document has been modified
    /// </summary>
    bool IsDirty { get; }
    
    /// <summary>
    /// Gets the file path if applicable
    /// </summary>
    string? FilePath { get; }
    
    /// <summary>
    /// Save the document
    /// </summary>
    Task<bool> SaveAsync();
    
    /// <summary>
    /// Save as new file
    /// </summary>
    Task<bool> SaveAsAsync(string filePath);
}

/// <summary>
/// Interface for tool window view models
/// </summary>
public interface IToolViewModel : IDockableViewModel
{
    /// <summary>
    /// Gets whether the tool is visible
    /// </summary>
    bool IsVisible { get; set; }
    
    /// <summary>
    /// Gets the preferred width
    /// </summary>
    double PreferredWidth { get; }
    
    /// <summary>
    /// Gets the preferred height
    /// </summary>
    double PreferredHeight { get; }
}

/// <summary>
/// Defines the preferred docking position for tools
/// </summary>
public enum ToolDockPosition
{
    Left,
    Right,
    Top,
    Bottom,
    Document,
    Float
}

// Note: Moved to ViewModels\DockableViewModelBase.cs