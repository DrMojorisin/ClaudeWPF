using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPFBase.Services;

namespace WPFBase.ViewModels
{
    /// <summary>
    /// Base class for document ViewModels in the docking system
    /// </summary>
    public abstract partial class DocumentViewModelBase : DockableViewModelBase
    {
        [ObservableProperty]
        private string _filePath = string.Empty;

        [ObservableProperty]
        private bool _isDirty;

        [ObservableProperty]
        private string _contentType = "Document";

        /// <summary>
        /// Initializes a new instance of DocumentViewModelBase
        /// </summary>
        protected DocumentViewModelBase()
        {
            Title = "Untitled";
            ContentId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Saves the document
        /// </summary>
        [RelayCommand]
        public virtual async Task<bool> SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                return await SaveAsAsync();
            }

            try
            {
                await SaveDocumentAsync(FilePath);
                IsDirty = false;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to save document");
                return false;
            }
        }

        /// <summary>
        /// Saves the document with a new filename
        /// </summary>
        [RelayCommand]
        public virtual async Task<bool> SaveAsAsync()
        {
            // This would typically show a save dialog
            // Override in derived classes to implement
            return await Task.FromResult(false);
        }

        /// <summary>
        /// Loads the document from a file
        /// </summary>
        public virtual async Task<bool> LoadAsync(string filePath)
        {
            try
            {
                FilePath = filePath;
                Title = System.IO.Path.GetFileName(filePath);
                await LoadDocumentAsync(filePath);
                IsDirty = false;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load document");
                return false;
            }
        }

        /// <summary>
        /// Override to implement document saving logic
        /// </summary>
        protected abstract Task SaveDocumentAsync(string filePath);

        /// <summary>
        /// Override to implement document loading logic
        /// </summary>
        protected abstract Task LoadDocumentAsync(string filePath);

        /// <summary>
        /// Called when closing the document
        /// </summary>
        public override bool OnClosing()
        {
            if (IsDirty)
            {
                // In a real application, this would show a dialog
                // asking the user if they want to save changes
                return true; // For now, allow closing
            }
            return base.OnClosing();
        }

        /// <summary>
        /// Marks the document as modified
        /// </summary>
        protected void MarkAsDirty()
        {
            IsDirty = true;
        }
    }
}