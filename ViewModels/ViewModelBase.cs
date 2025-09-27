using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WPFBase.ViewModels;

/// <summary>
/// Base class for all ViewModels using CommunityToolkit.Mvvm
/// Provides INotifyPropertyChanged implementation and common functionality
/// </summary>
public abstract partial class ViewModelBase : ObservableObject, IDisposable
{
    /// <summary>
    /// Gets or sets whether the ViewModel is currently busy
    /// Useful for showing loading indicators
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool isBusy;

    /// <summary>
    /// Gets whether the ViewModel is not busy
    /// </summary>
    public bool IsNotBusy => !IsBusy;

    /// <summary>
    /// Gets or sets the title of the view
    /// </summary>
    [ObservableProperty]
    private string title = string.Empty;

    /// <summary>
    /// Called when the ViewModel is navigated to
    /// </summary>
    public virtual Task OnNavigatedToAsync(object? parameter = null)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when the ViewModel is navigated from
    /// </summary>
    public virtual Task OnNavigatedFromAsync()
    {
        return Task.CompletedTask;
    }

    private bool _disposed;

    /// <summary>
    /// Performs application-defined tasks associated with freeing resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                OnDispose();
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Override this method to dispose of managed resources
    /// </summary>
    protected virtual void OnDispose()
    {
        // Override in derived classes to clean up resources
    }
}