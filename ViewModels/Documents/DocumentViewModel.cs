using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPFBase.Interfaces;
using WPFBase.Services;

namespace WPFBase.ViewModels.Documents;

/// <summary>
/// Base class for document view models
/// </summary>
public abstract partial class DocumentViewModel : DockableViewModelBase, IDocumentViewModel
{
    private readonly IDialogService _dialogService;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayTitle))]
    private bool isDirty;
    
    [ObservableProperty]
    private string? filePath;
    
    [ObservableProperty]
    private string content = string.Empty;

    public DocumentViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public string DisplayTitle => IsDirty ? $"{Title}*" : Title;

    public override async Task<bool> OnClosingAsync()
    {
        if (!IsDirty) return true;
        
        var result = await _dialogService.ShowMessageAsync(
            $"Save changes to {Title}?",
            "Unsaved Changes",
            System.Windows.MessageBoxButton.YesNoCancel,
            System.Windows.MessageBoxImage.Question);
            
        return result switch
        {
            System.Windows.MessageBoxResult.Yes => await SaveAsync(),
            System.Windows.MessageBoxResult.No => true,
            _ => false
        };
    }

    public abstract Task<bool> SaveAsync();
    
    public abstract Task<bool> SaveAsAsync(string filePath);
    
    [RelayCommand]
    protected virtual async Task Save()
    {
        await SaveAsync();
    }
    
    [RelayCommand]
    protected virtual async Task SaveAs()
    {
        var path = await _dialogService.ShowSaveFileDialogAsync();
        if (!string.IsNullOrEmpty(path))
        {
            await SaveAsAsync(path);
        }
    }
}

/// <summary>
/// Text editor document view model
/// </summary>
public partial class TextDocumentViewModel : DocumentViewModel
{
    private static int _documentCounter = 1;
    private readonly LoggingService _loggingService;
    
    [ObservableProperty]
    private string syntax = "Text";
    
    [ObservableProperty]
    private string content = string.Empty;
    
    public TextDocumentViewModel(IDialogService dialogService, LoggingService loggingService) 
        : base(dialogService)
    {
        _loggingService = loggingService;
        Title = $"Document{_documentCounter++}";
    }


    public override async Task<bool> SaveAsync()
    {
        if (string.IsNullOrEmpty(FilePath))
        {
            return await SaveAsAsync(FilePath ?? "");
        }
        
        try
        {
            await File.WriteAllTextAsync(FilePath, Content);
            IsDirty = false;
            _loggingService.LogInformation("Saved document: {FilePath}", FilePath);
            return true;
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to save document: {FilePath}", FilePath);
            return false;
        }
    }

    public override async Task<bool> SaveAsAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            var dialogService = App.Current.Services.GetService(typeof(IDialogService)) as IDialogService;
            filePath = await dialogService!.ShowSaveFileDialogAsync() ?? "";
            if (string.IsNullOrEmpty(filePath))
                return false;
        }
        
        FilePath = filePath;
        Title = Path.GetFileName(filePath);
        UpdateSyntaxFromExtension();
        
        return await SaveAsync();
    }
    
    public async Task LoadFromFileAsync(string filePath)
    {
        try
        {
            Content = await File.ReadAllTextAsync(filePath);
            FilePath = filePath;
            Title = Path.GetFileName(filePath);
            IsDirty = false;
            UpdateSyntaxFromExtension();
            
            _loggingService.LogInformation("Loaded document: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to load document: {FilePath}", filePath);
            throw;
        }
    }
    
    private void UpdateSyntaxFromExtension()
    {
        if (string.IsNullOrEmpty(FilePath)) return;
        
        Syntax = Path.GetExtension(FilePath).ToLower() switch
        {
            ".cs" => "C#",
            ".xaml" or ".xml" => "XML",
            ".json" => "JSON",
            ".js" or ".ts" => "JavaScript",
            ".html" or ".htm" => "HTML",
            ".css" => "CSS",
            ".sql" => "SQL",
            ".py" => "Python",
            ".md" => "Markdown",
            _ => "Text"
        };
    }
    
    partial void OnContentChanged(string value)
    {
        IsDirty = true;
    }
}