# Claude Code Snippets for WPF MVVM Development

## 🚀 Quick Copy-Paste Templates for Claude Code

Use these snippets to quickly generate WPF MVVM code. Just tell Claude: "Create a [pattern name] for [your requirement]"

## 1. Complete ViewModel with Validation

```csharp
// SNIPPET: ValidatedViewModel
// Usage: "Create a user registration form with email, password, and age validation"
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
using WPFBase.Interfaces;

namespace WPFBase.ViewModels;

public partial class $NAME$ViewModel : ObservableValidator
{
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [Required(ErrorMessage = "$FIELD$ is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    private string $FIELD$ = string.Empty;

    // Computed property
    public bool CanSave => !HasErrors && !string.IsNullOrEmpty($FIELD$);

    public $NAME$ViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors)
        {
            await _dialogService.ShowWarningAsync("Please fix validation errors");
            return;
        }

        // TODO: Save logic
        await _dialogService.ShowInformationAsync("Saved successfully!");
    }
}
```

## 2. Master-Detail ViewModel Pattern

```csharp
// SNIPPET: MasterDetailViewModel
// Usage: "Create a product list with detail view"
public partial class $ENTITY$ListViewModel : ViewModelBase
{
    private readonly I$ENTITY$Service _service;
    private readonly INavigationService _navigation;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    private $ENTITY$? selectedItem;

    [ObservableProperty]
    private ObservableCollection<$ENTITY$> items = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private string searchText = string.Empty;

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var data = await _service.GetAllAsync();
            Items = new ObservableCollection<$ENTITY$>(data);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private async Task EditAsync()
    {
        await _navigation.NavigateToAsync<$ENTITY$DetailViewModel>(SelectedItem?.Id);
    }

    private bool CanEdit() => SelectedItem != null;

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task SearchAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadAsync();
            return;
        }

        IsBusy = true;
        try
        {
            var results = await _service.SearchAsync(SearchText, cancellationToken);
            Items = new ObservableCollection<$ENTITY$>(results);
        }
        finally { IsBusy = false; }
    }
}
```

## 3. Async Data Loading Pattern

```csharp
// SNIPPET: AsyncDataLoader
// Usage: "Load data from API with retry and error handling"
public partial class DataViewModel : ViewModelBase
{
    private readonly IApiService _api;
    private readonly IDialogService _dialog;

    [ObservableProperty]
    private string status = "Ready";

    public IAsyncRelayCommand<int> LoadDataCommand { get; }

    public DataViewModel(IApiService api, IDialogService dialog)
    {
        _api = api;
        _dialog = dialog;

        LoadDataCommand = new AsyncRelayCommand<int>(
            LoadDataWithRetryAsync,
            (id) => id > 0,
            AsyncRelayCommandOptions.FlowExceptionsToTaskScheduler);
    }

    private async Task LoadDataWithRetryAsync(int id, CancellationToken cancellationToken)
    {
        Status = "Loading...";
        int retries = 3;

        while (retries > 0)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var data = await _api.GetDataAsync(id, cancellationToken);
                ProcessData(data);
                Status = "Loaded successfully";
                return;
            }
            catch (HttpRequestException ex) when (retries > 1)
            {
                retries--;
                Status = $"Retry {4 - retries}/3...";
                await Task.Delay(1000 * (4 - retries), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Status = "Cancelled";
                throw;
            }
            catch (Exception ex)
            {
                Status = "Error";
                await _dialog.ShowErrorAsync($"Failed to load: {ex.Message}");
                throw;
            }
        }
    }
}
```

## 4. Reactive Search with Debounce

```csharp
// SNIPPET: ReactiveSearch
// Usage: "Create live search that waits for user to stop typing"
public partial class SearchViewModel : ViewModelBase
{
    private readonly ISearchService _searchService;
    private CancellationTokenSource? _searchCts;

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SearchResult> results = new();

    [ObservableProperty]
    private bool isSearching;

    partial void OnSearchQueryChanged(string? oldValue, string newValue)
    {
        // Cancel previous search
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();

        // Debounce: wait 300ms after user stops typing
        _ = DebounceSearchAsync(newValue, _searchCts.Token);
    }

    private async Task DebounceSearchAsync(string query, CancellationToken cancellationToken)
    {
        try
        {
            // Wait for user to stop typing
            await Task.Delay(300, cancellationToken);

            if (string.IsNullOrWhiteSpace(query))
            {
                Results.Clear();
                return;
            }

            IsSearching = true;
            var searchResults = await _searchService.SearchAsync(query, cancellationToken);

            Results.Clear();
            foreach (var result in searchResults)
                Results.Add(result);
        }
        catch (OperationCanceledException)
        {
            // Search was cancelled, ignore
        }
        finally
        {
            IsSearching = false;
        }
    }
}
```

## 5. Dialog and Modal Pattern

```csharp
// SNIPPET: DialogViewModel
// Usage: "Create a modal dialog for user input"
public partial class InputDialogViewModel : ObservableValidator, IDialogViewModel<string>
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(OkCommand))]
    [Required]
    [MinLength(3)]
    private string inputValue = string.Empty;

    [ObservableProperty]
    private string prompt = "Enter value:";

    public string? Result { get; private set; }

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private void Ok()
    {
        ValidateAllProperties();
        if (!HasErrors)
        {
            Result = InputValue;
            RequestClose?.Invoke(true);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Result = null;
        RequestClose?.Invoke(false);
    }

    private bool CanConfirm() => !HasErrors && !string.IsNullOrWhiteSpace(InputValue);

    public event Action<bool>? RequestClose;
}

// Usage in parent ViewModel:
var result = await _dialogService.ShowDialogAsync<InputDialogViewModel, string>(
    new { Prompt = "Enter product name:" });
if (result != null)
{
    // User clicked OK
}
```

## 6. Collection with Add/Edit/Delete

```csharp
// SNIPPET: CrudCollection
// Usage: "Create a list where users can add, edit, and delete items"
public partial class ItemManagerViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<ItemViewModel> items = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private ItemViewModel? selectedItem;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private string newItemName = string.Empty;

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add()
    {
        var item = new ItemViewModel { Name = NewItemName, Id = Guid.NewGuid() };
        Items.Add(item);
        NewItemName = string.Empty;
        SelectedItem = item;
    }

    private bool CanAdd() => !string.IsNullOrWhiteSpace(NewItemName);

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task EditAsync()
    {
        if (SelectedItem == null) return;

        var editedName = await _dialogService.ShowInputAsync(
            "Edit item name:", "Edit", SelectedItem.Name);

        if (!string.IsNullOrWhiteSpace(editedName))
        {
            SelectedItem.Name = editedName;
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteAsync()
    {
        if (SelectedItem == null) return;

        if (await _dialogService.ShowConfirmationAsync(
            $"Delete '{SelectedItem.Name}'?", "Confirm Delete"))
        {
            Items.Remove(SelectedItem);
            SelectedItem = null;
        }
    }

    private bool HasSelection() => SelectedItem != null;
}
```

## 7. File Operations Pattern

```csharp
// SNIPPET: FileOperations
// Usage: "Create file open/save functionality"
public partial class FileViewModel : ViewModelBase
{
    private readonly IDialogService _dialog;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFile))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string? currentFilePath;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string content = string.Empty;

    [ObservableProperty]
    private bool isDirty;

    public bool HasFile => !string.IsNullOrEmpty(CurrentFilePath);

    [RelayCommand]
    private async Task OpenAsync()
    {
        if (IsDirty && !await ConfirmDiscardChanges())
            return;

        var path = await _dialog.ShowOpenFileDialogAsync("txt", "Text files|*.txt|All files|*.*");
        if (!string.IsNullOrEmpty(path))
        {
            CurrentFilePath = path;
            Content = await File.ReadAllTextAsync(path);
            IsDirty = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (string.IsNullOrEmpty(CurrentFilePath))
        {
            await SaveAsAsync();
            return;
        }

        await File.WriteAllTextAsync(CurrentFilePath, Content);
        IsDirty = false;
        await _dialog.ShowInformationAsync("File saved successfully");
    }

    [RelayCommand]
    private async Task SaveAsAsync()
    {
        var path = await _dialog.ShowSaveFileDialogAsync("txt", "Text files|*.txt");
        if (!string.IsNullOrEmpty(path))
        {
            CurrentFilePath = path;
            await SaveAsync();
        }
    }

    private bool CanSave() => IsDirty || !string.IsNullOrEmpty(Content);

    partial void OnContentChanged(string? oldValue, string newValue)
    {
        if (oldValue != null) IsDirty = true;
    }
}
```

## 8. Progress Reporting Pattern

```csharp
// SNIPPET: ProgressReporting
// Usage: "Show progress for long-running operations"
public partial class ProgressViewModel : ViewModelBase
{
    private readonly IMessageBus _messageBus;

    [ObservableProperty]
    private double progressValue;

    [ObservableProperty]
    private string progressText = "Ready";

    [ObservableProperty]
    private bool isIndeterminate;

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task ProcessFilesAsync(CancellationToken cancellationToken)
    {
        var files = GetFiles();
        ProgressValue = 0;
        IsIndeterminate = false;

        for (int i = 0; i < files.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ProgressText = $"Processing {Path.GetFileName(files[i])}...";
            ProgressValue = (double)(i + 1) / files.Count * 100;

            _messageBus.PublishProgress("Processing files", ProgressValue / 100, ProgressText);

            await ProcessFileAsync(files[i], cancellationToken);
        }

        ProgressText = "Completed";
        _messageBus.PublishStatus("All files processed", StatusMessageType.Success);
    }
}
```

## 9. Settings/Preferences Pattern

```csharp
// SNIPPET: SettingsViewModel
// Usage: "Create application settings page"
public partial class SettingsViewModel : ViewModelBase
{
    private readonly IUserSettingsService _settings;
    private readonly IThemeService _themes;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string theme = "Light";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private int fontSize = 12;

    [ObservableProperty]
    private bool autoSave;

    [ObservableProperty]
    private bool hasChanges;

    public ObservableCollection<string> AvailableThemes { get; } = new()
    {
        "Light", "Dark", "Blue", "High Contrast"
    };

    public SettingsViewModel(IUserSettingsService settings, IThemeService themes)
    {
        _settings = settings;
        _themes = themes;
        LoadSettings();
    }

    private void LoadSettings()
    {
        Theme = _settings.GetSetting("Theme", "Light");
        FontSize = _settings.GetSetting("FontSize", 12);
        AutoSave = _settings.GetSetting("AutoSave", false);
        HasChanges = false;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        _settings.SetSetting("Theme", Theme);
        _settings.SetSetting("FontSize", FontSize);
        _settings.SetSetting("AutoSave", AutoSave);

        await _settings.SaveAsync();
        await _themes.ApplyThemeAsync(Theme);

        HasChanges = false;
    }

    [RelayCommand]
    private void Reset()
    {
        LoadSettings();
        HasChanges = false;
    }

    private bool CanSave() => HasChanges;

    partial void OnThemeChanged(string? oldValue, string newValue) => HasChanges = true;
    partial void OnFontSizeChanged(int oldValue, int newValue) => HasChanges = true;
    partial void OnAutoSaveChanged(bool oldValue, bool newValue) => HasChanges = true;
}
```

## 10. Navigation with Parameters

```csharp
// SNIPPET: NavigationWithParams
// Usage: "Navigate between views with data passing"
public partial class NavigationViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;

    [RelayCommand]
    private async Task NavigateToDetailAsync(int itemId)
    {
        // Navigate with parameter
        await _navigation.NavigateToAsync<DetailViewModel>(
            new NavigationParameter("itemId", itemId));
    }

    // In DetailViewModel:
    protected override async Task OnNavigatedToAsync(object? parameter)
    {
        if (parameter is NavigationParameter navParam &&
            navParam.TryGetValue<int>("itemId", out var id))
        {
            await LoadItemAsync(id);
        }
    }

    protected override async Task<bool> OnNavigatingFromAsync()
    {
        if (HasUnsavedChanges)
        {
            return await _dialogService.ShowConfirmationAsync(
                "You have unsaved changes. Continue?");
        }
        return true;
    }
}
```

## 🎯 Quick Claude Commands

### Generate Complete CRUD View
"Using the WPFBase framework, create a complete CRUD view for [Entity] with validation, search, and async operations using patterns from CLAUDE_CODE_SNIPPETS.md"

### Add Feature to Existing View
"Add [feature description] to [ViewModel] following the patterns in CLAUDE_CODE_SNIPPETS.md"

### Create Service Integration
"Create a service that [description] and integrate it with [ViewModel] using dependency injection"

### Generate Test Cases
"Generate unit tests for [ViewModel] using xUnit and Moq following the patterns in the framework"

## 📝 Notes for Claude Code

1. **Always use partial classes** for ViewModels with source generators
2. **Prefer [ObservableProperty]** over manual property implementation
3. **Use notification attributes** to eliminate manual updates
4. **Implement IAsyncRelayCommand** for better async control
5. **Follow constructor injection** for all dependencies
6. **Use CancellationToken** in all async methods
7. **Validate with ObservableValidator** not custom base classes
8. **Register services and ViewModels** in App.xaml.cs
9. **Use IDialogService** never MessageBox directly
10. **Follow naming convention**: *ViewModel, *Service, *View

## 🔧 Custom Snippet Creation

To create custom snippets, use this template:
```csharp
// SNIPPET: [SnippetName]
// Usage: "[When to use this pattern]"
// Dependencies: [Required services/interfaces]
// Replace: $VARIABLE$ with actual values

[Your code pattern here]
```