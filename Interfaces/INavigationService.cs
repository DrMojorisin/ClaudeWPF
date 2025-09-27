using System.Windows.Controls;

namespace WPFBase.Interfaces;

/// <summary>
/// Navigation service interface for navigating between views
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Gets the current view
    /// </summary>
    UserControl? CurrentView { get; }

    /// <summary>
    /// Navigate to a view by ViewModel type
    /// </summary>
    Task NavigateToAsync<TViewModel>(object? parameter = null) where TViewModel : class;

    /// <summary>
    /// Navigate to a view by ViewModel type name
    /// </summary>
    Task NavigateToAsync(string viewModelTypeName, object? parameter = null);

    /// <summary>
    /// Navigate back to the previous view
    /// </summary>
    Task GoBackAsync();

    /// <summary>
    /// Check if navigation can go back
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Event raised when navigation occurs
    /// </summary>
    event EventHandler<NavigationEventArgs>? Navigated;

    /// <summary>
    /// Clear navigation history (keep only current view)
    /// </summary>
    void ClearHistory();

    /// <summary>
    /// Check if navigation can go forward
    /// </summary>
    bool CanGoForward { get; }
}

/// <summary>
/// Navigation event arguments
/// </summary>
public class NavigationEventArgs : EventArgs
{
    public Type? ViewModelType { get; set; }
    public object? Parameter { get; set; }
    public bool IsFromCache { get; set; }
}