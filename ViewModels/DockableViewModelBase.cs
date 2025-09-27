using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using WPFBase.Interfaces;

namespace WPFBase.ViewModels
{
    /// <summary>
    /// Base class for all dockable ViewModels (both documents and tools)
    /// </summary>
    public abstract partial class DockableViewModelBase : ViewModelBase, IDockableViewModel
    {
        private string _contentId = Guid.NewGuid().ToString();
        
        public virtual string ContentId 
        {
            get => _contentId;
            set => SetProperty(ref _contentId, value);
        }

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private bool _isActive;

        /// <summary>
        /// Gets whether the dockable can be closed
        /// </summary>
        public virtual bool CanClose => true;

        /// <summary>
        /// Called when the dockable is closing
        /// </summary>
        /// <returns>True to allow closing, false to cancel</returns>
        public virtual Task<bool> OnClosingAsync()
        {
            return Task.FromResult(OnClosing());
        }

        /// <summary>
        /// Called when the dockable is closed
        /// </summary>
        /// <returns>True to allow closing, false to cancel</returns>
        public virtual bool OnClosing()
        {
            return true;
        }

        /// <summary>
        /// Called after the dockable has been closed
        /// </summary>
        public virtual void OnClosed()
        {
            Dispose();
        }

        /// <summary>
        /// Called when activated
        /// </summary>
        public virtual void OnActivated()
        {
            IsActive = true;
            IsSelected = true;
        }

        /// <summary>
        /// Called when deactivated
        /// </summary>
        public virtual void OnDeactivated()
        {
            IsActive = false;
        }

        /// <summary>
        /// Activates this dockable
        /// </summary>
        public virtual void Activate()
        {
            OnActivated();
        }

        /// <summary>
        /// Deactivates this dockable
        /// </summary>
        public virtual void Deactivate()
        {
            OnDeactivated();
        }
    }
}