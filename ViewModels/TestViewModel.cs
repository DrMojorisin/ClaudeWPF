using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WPFBase.Interfaces;

namespace WPFBase.ViewModels;

/// <summary>
/// Test ViewModel to validate Claude Code integration and framework patterns
/// </summary>
public partial class TestViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IMessageBus _messageBus;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ProcessCommand))]
    private string testInput = string.Empty;

    [ObservableProperty]
    private bool isProcessing;

    public ObservableCollection<string> Results { get; } = new();

    public TestViewModel(IDialogService dialogService, IMessageBus messageBus)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        Title = "Framework Test";
    }

    [RelayCommand(CanExecute = nameof(CanProcess))]
    private async Task ProcessAsync()
    {
        IsProcessing = true;
        IsBusy = true;

        try
        {
            // Simulate some async work
            await Task.Delay(1000);

            Results.Add($"Processed: {TestInput} at {DateTime.Now:HH:mm:ss}");

            // Test message bus
            _messageBus.Publish(new Models.Messages.StatusMessage
            {
                Text = $"Successfully processed: {TestInput}",
                Type = Models.Messages.StatusMessageType.Success
            });

            TestInput = string.Empty;
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Processing failed", "Error", ex);
        }
        finally
        {
            IsProcessing = false;
            IsBusy = false;
        }
    }

    private bool CanProcess() => !string.IsNullOrWhiteSpace(TestInput) && !IsProcessing;

    [RelayCommand]
    private async Task ShowInfoAsync()
    {
        await _dialogService.ShowInformationAsync(
            "This demonstrates the working WPFBase framework patterns:\n\n" +
            "✓ CommunityToolkit.Mvvm [ObservableProperty] and [RelayCommand]\n" +
            "✓ Dependency injection (IDialogService, IMessageBus)\n" +
            "✓ Async command patterns with CanExecute\n" +
            "✓ ObservableCollection data binding\n" +
            "✓ IsBusy state management\n" +
            "✓ Message bus communication\n" +
            "✓ Error handling patterns",
            "Framework Features"
        );
    }

    [RelayCommand]
    private void ClearResults()
    {
        Results.Clear();
    }

    // Removed: No longer needed thanks to [NotifyCanExecuteChangedFor]
    // partial void OnTestInputChanged(string value)
    // {
    //     ProcessCommand.NotifyCanExecuteChanged();
    // }
}