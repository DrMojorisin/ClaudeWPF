using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WPFBase.Interfaces;

namespace WPFBase.ViewModels;

/// <summary>
/// Demonstrates best practices for using IAsyncRelayCommand for enhanced async control
/// </summary>
public partial class AsyncCommandExampleViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private string status = "Ready";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshDataCommand))]
    private bool hasData;

    public ObservableCollection<string> DataItems { get; } = new();

    // BEST PRACTICE: Use IAsyncRelayCommand for better async command control
    // This provides IsRunning, ExecutionTask, and Cancel() capabilities
    public IAsyncRelayCommand LoadDataCommand { get; }
    public IAsyncRelayCommand RefreshDataCommand { get; }
    public IAsyncRelayCommand<string> ProcessItemCommand { get; }

    public AsyncCommandExampleViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        Title = "Async Command Examples";

        // Initialize async commands with proper error handling
        LoadDataCommand = new AsyncRelayCommand(
            LoadDataAsync,
            AsyncRelayCommandOptions.FlowExceptionsToTaskScheduler);

        RefreshDataCommand = new AsyncRelayCommand(
            RefreshDataAsync,
            CanRefreshData);

        ProcessItemCommand = new AsyncRelayCommand<string>(
            ProcessItemAsync,
            CanProcessItem);
    }

    private async Task LoadDataAsync(CancellationToken cancellationToken = default)
    {
        Status = "Loading data...";
        DataItems.Clear();

        try
        {
            // Simulate data loading
            await Task.Delay(1500, cancellationToken);

            for (int i = 1; i <= 10; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                DataItems.Add($"Item {i} - Loaded at {DateTime.Now:HH:mm:ss}");
                await Task.Delay(200, cancellationToken);
            }

            HasData = true;
            Status = $"Loaded {DataItems.Count} items";
        }
        catch (OperationCanceledException)
        {
            Status = "Load cancelled";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
            await _dialogService.ShowErrorAsync(ex.Message);
        }
    }

    private async Task RefreshDataAsync(CancellationToken cancellationToken = default)
    {
        Status = "Refreshing...";

        try
        {
            await Task.Delay(1000, cancellationToken);

            // Update existing data
            for (int i = 0; i < DataItems.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                DataItems[i] = $"Item {i + 1} - Refreshed at {DateTime.Now:HH:mm:ss}";
            }

            Status = "Data refreshed";
        }
        catch (OperationCanceledException)
        {
            Status = "Refresh cancelled";
        }
    }

    private bool CanRefreshData() => HasData && !RefreshDataCommand.IsRunning;

    private async Task ProcessItemAsync(string? item, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(item))
            return;

        Status = $"Processing: {item}";

        try
        {
            await Task.Delay(2000, cancellationToken);
            Status = $"Processed: {item}";
            await _dialogService.ShowInformationAsync($"Successfully processed: {item}");
        }
        catch (OperationCanceledException)
        {
            Status = "Processing cancelled";
        }
    }

    private bool CanProcessItem(string? item) => !string.IsNullOrWhiteSpace(item);

    // BEST PRACTICE: Check command running state
    [RelayCommand]
    private async Task ShowCommandStatesAsync()
    {
        var states = $"""
            Command States:

            LoadDataCommand:
              - IsRunning: {LoadDataCommand.IsRunning}
              - CanBeCanceled: {LoadDataCommand.CanBeCanceled}
              - ExecutionTask: {(LoadDataCommand.ExecutionTask?.Status.ToString() ?? "None")}

            RefreshDataCommand:
              - IsRunning: {RefreshDataCommand.IsRunning}
              - CanExecute: {RefreshDataCommand.CanExecute(null)}

            ProcessItemCommand:
              - IsRunning: {ProcessItemCommand.IsRunning}
            """;

        await _dialogService.ShowInformationAsync(states, "Command States");
    }

    // BEST PRACTICE: Cancel running commands programmatically
    [RelayCommand]
    private void CancelAllOperations()
    {
        // IAsyncRelayCommand provides direct Cancel() method
        LoadDataCommand.Cancel();
        RefreshDataCommand.Cancel();
        ProcessItemCommand.Cancel();

        Status = "All operations cancelled";
    }

    // BEST PRACTICE: Run multiple async operations with proper tracking
    [RelayCommand]
    private async Task RunMultipleOperationsAsync()
    {
        Status = "Running multiple operations...";

        // Start operations concurrently
        var loadTask = LoadDataCommand.ExecuteAsync(null);
        await Task.Delay(500); // Small delay to show concurrent execution

        // Check if first command is running
        if (LoadDataCommand.IsRunning)
        {
            Status = "Load is running, waiting...";
        }

        // Wait for completion
        await loadTask;

        // Now refresh if we have data
        if (HasData)
        {
            await RefreshDataCommand.ExecuteAsync(null);
        }

        Status = "All operations completed";
    }

    // BEST PRACTICE: Handle concurrent execution control
    private IAsyncRelayCommand? _exclusiveOperationCommand;
    public IAsyncRelayCommand ExclusiveOperationCommand =>
        _exclusiveOperationCommand ??= new AsyncRelayCommand(
            ExecuteExclusiveOperationAsync); // Default is no concurrent executions

    private async Task ExecuteExclusiveOperationAsync(CancellationToken cancellationToken)
    {
        Status = "Exclusive operation running (can't run again until complete)...";
        await Task.Delay(3000, cancellationToken);
        Status = "Exclusive operation complete";
    }
}