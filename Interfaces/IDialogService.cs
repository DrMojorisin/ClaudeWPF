using System.Windows;

namespace WPFBase.Interfaces;

/// <summary>
/// Service for showing dialogs and popups
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Show a simple message dialog
    /// </summary>
    Task<MessageBoxResult> ShowMessageAsync(string message, string title = "Message", 
        MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Information);
    
    /// <summary>
    /// Show a confirmation dialog
    /// </summary>
    Task<bool> ShowConfirmationAsync(string message, string title = "Confirm");
    
    /// <summary>
    /// Show an error dialog
    /// </summary>
    Task ShowErrorAsync(string message, string title = "Error", Exception? exception = null);
    
    /// <summary>
    /// Show a warning dialog
    /// </summary>
    Task ShowWarningAsync(string message, string title = "Warning");
    
    /// <summary>
    /// Show an information dialog
    /// </summary>
    Task ShowInformationAsync(string message, string title = "Information");
    
    /// <summary>
    /// Show an input dialog for getting text from user
    /// </summary>
    Task<string?> ShowInputAsync(string prompt, string title = "Input", string defaultValue = "");
    
    /// <summary>
    /// Show a custom dialog with a ViewModel
    /// </summary>
    Task<TResult?> ShowDialogAsync<TViewModel, TResult>(object? parameter = null) 
        where TViewModel : class
        where TResult : class;
    
    /// <summary>
    /// Show a custom dialog without expecting a result
    /// </summary>
    Task ShowDialogAsync<TViewModel>(object? parameter = null) where TViewModel : class;
    
    /// <summary>
    /// Show a file open dialog
    /// </summary>
    Task<string?> ShowOpenFileDialogAsync(string filter = "All Files|*.*", string title = "Open File");
    
    /// <summary>
    /// Show a file save dialog
    /// </summary>
    Task<string?> ShowSaveFileDialogAsync(string filter = "All Files|*.*", string title = "Save File", string defaultFileName = "");
    
    /// <summary>
    /// Show a folder browser dialog
    /// </summary>
    Task<string?> ShowFolderBrowserAsync(string title = "Select Folder");
}

/// <summary>
/// Base class for dialog ViewModels
/// </summary>
public interface IDialogViewModel
{
    /// <summary>
    /// Gets or sets the dialog result
    /// </summary>
    object? DialogResult { get; set; }
    
    /// <summary>
    /// Gets whether the dialog can be closed
    /// </summary>
    bool CanClose { get; }
    
    /// <summary>
    /// Called when dialog is about to close
    /// </summary>
    Task<bool> OnClosingAsync();
    
    /// <summary>
    /// Called when dialog is shown
    /// </summary>
    Task OnDialogOpenedAsync(object? parameter);
}