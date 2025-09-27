using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using WPFBase.Interfaces;

namespace WPFBase.ViewModels;

/// <summary>
/// Demonstrates BEST practices for using CommunityToolkit.Mvvm (2024-2025)
/// This shows ALL the advanced features properly implemented
/// </summary>
public partial class BestPracticesViewModel : ObservableValidator
{
    private readonly IDialogService _dialogService;

    // BEST PRACTICE 1: Use [NotifyCanExecuteChangedFor] to auto-update commands
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    [NotifyDataErrorInfo]
    [Required]
    [MinLength(3)]
    private string name = string.Empty;

    // BEST PRACTICE 2: Use [NotifyPropertyChangedFor] for computed properties
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    private string firstName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    private string lastName = string.Empty;

    // Computed property that updates when firstName or lastName changes
    public string FullName => $"{FirstName} {LastName}".Trim();

    // BEST PRACTICE 3: Use [NotifyCanExecuteChangedFor] for multiple commands
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ProcessCommand))]
    [NotifyCanExecuteChangedFor(nameof(ValidateCommand))]
    private bool isReady;

    // BEST PRACTICE 4: Collections don't need [ObservableProperty]
    public ObservableCollection<string> Items { get; } = new();

    // BEST PRACTICE 5: Observable property with dependent property
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusDisplay))]
    private string status = "Ready";

    public string StatusDisplay => $"Status: {Status}";

    public BestPracticesViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    // BEST PRACTICE 6: Simple commands without parameters
    [RelayCommand]
    private void Clear()
    {
        Name = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        Items.Clear();
    }

    // BEST PRACTICE 7: Commands with CanExecute that auto-update
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        await _dialogService.ShowInformationAsync($"Saved: {Name}");
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(Name) && !HasErrors;

    // BEST PRACTICE 8: Commands with parameters
    [RelayCommand]
    private void RemoveItem(string? item)
    {
        if (item != null && Items.Contains(item))
        {
            Items.Remove(item);
        }
    }

    // BEST PRACTICE 9: Async commands with CancellationToken
    [RelayCommand(IncludeCancelCommand = true)] // Generates ProcessCommand AND CancelProcessCommand
    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        try
        {
            Status = "Processing...";
            await Task.Delay(2000, cancellationToken);
            Status = "Complete!";
        }
        catch (OperationCanceledException)
        {
            Status = "Cancelled";
        }
    }

    // BEST PRACTICE 10: Command with both parameter and CancellationToken
    [RelayCommand]
    private async Task LoadDataAsync(string? source, CancellationToken cancellationToken)
    {
        Status = $"Loading from {source ?? "default"}...";
        await Task.Delay(1000, cancellationToken);
        Items.Add($"Data from {source}");
    }

    // BEST PRACTICE 11: Validation commands
    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        ValidateAllProperties();

        if (HasErrors)
        {
            await _dialogService.ShowWarningAsync("Please fix validation errors");
            return;
        }

        await _dialogService.ShowInformationAsync("Submitted successfully!");
    }

    private bool CanSubmit() => IsReady && !HasErrors;

    [RelayCommand(CanExecute = nameof(CanValidate))]
    private void Validate()
    {
        ValidateAllProperties();
        Status = HasErrors ? "Validation failed" : "Validation passed";
    }

    private bool CanValidate() => IsReady;

    // BEST PRACTICE 12: Use partial methods for property change handlers
    partial void OnNameChanged(string? oldValue, string newValue)
    {
        // This is called automatically when Name changes
        // Both old and new values are provided
        if (!string.IsNullOrWhiteSpace(newValue))
        {
            Status = $"Name changed from '{oldValue}' to '{newValue}'";
        }
    }

    partial void OnNameChanging(string? oldValue, string newValue)
    {
        // Called BEFORE the property changes - can be used for validation
        // or to cancel the change by throwing an exception
        if (newValue?.Contains("test", StringComparison.OrdinalIgnoreCase) == true)
        {
            // Could throw exception to prevent change
            // throw new ArgumentException("Test names not allowed");
        }
    }

    // BEST PRACTICE 13: Custom property implementation when needed
    private int _customValue;

    public int CustomValue
    {
        get => _customValue;
        set
        {
            // Custom logic before setting
            if (value < 0)
                value = 0;

            if (SetProperty(ref _customValue, value))
            {
                // Custom logic after setting
                OnPropertyChanged(nameof(CustomValueDisplay));
            }
        }
    }

    public string CustomValueDisplay => $"Value: {CustomValue}";

    // BEST PRACTICE 14: IAsyncRelayCommand for better async command handling
    private IAsyncRelayCommand? _refreshCommand;
    public IAsyncRelayCommand RefreshCommand => _refreshCommand ??= new AsyncRelayCommand(RefreshAsync);

    private async Task RefreshAsync()
    {
        await Task.Delay(1000);
        Items.Clear();
        Items.Add("Refreshed at " + DateTime.Now);
    }

    // BEST PRACTICE 15: WeakReferenceMessenger for decoupled communication
    public void SendMessage()
    {
        // Send a message to other parts of the application
        WeakReferenceMessenger.Default.Send(new CustomStatusMessage(Status));
    }

    // BEST PRACTICE 16: Cleanup when view model is no longer needed
    public void Cleanup()
    {
        // Unregister from messenger if registered
        WeakReferenceMessenger.Default.UnregisterAll(this);
        // Cancel any running commands
        ProcessCommand?.Cancel();
    }
}

// Message class for WeakReferenceMessenger
public record CustomStatusMessage(string Status);