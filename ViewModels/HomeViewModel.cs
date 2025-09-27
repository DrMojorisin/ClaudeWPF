using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WPFBase.Interfaces;

namespace WPFBase.ViewModels;

/// <summary>
/// Home view ViewModel - demonstrates CommunityToolkit.Mvvm features
/// </summary>
public partial class HomeViewModel : ViewModelBase
{
    private readonly INavigationService? _navigationService;
    [ObservableProperty]
    private string welcomeMessage = "Welcome to WPF MVVM with Community Toolkit!";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddItemCommand))]
    private string userInput = string.Empty;

    [ObservableProperty]
    private int counter;

    [ObservableProperty]
    private DateTime currentTime = DateTime.Now;

    public ObservableCollection<string> Items { get; } = new();

    public HomeViewModel(INavigationService? navigationService = null)
    {
        _navigationService = navigationService;
        Title = "Home";
        InitializeData();
        
        // Start a timer to update current time
        _ = UpdateTimeAsync();
    }

    private void InitializeData()
    {
        Items.Add("Item 1 - MVVM Pattern");
        Items.Add("Item 2 - Data Binding");
        Items.Add("Item 3 - Commands");
        Items.Add("Item 4 - Observable Properties");
    }

    [RelayCommand]
    private void IncrementCounter()
    {
        Counter++;
    }

    [RelayCommand]
    private void DecrementCounter()
    {
        Counter--;
    }

    [RelayCommand]
    private void ResetCounter()
    {
        Counter = 0;
    }

    [RelayCommand(CanExecute = nameof(CanAddItem))]
    private void AddItem()
    {
        if (!string.IsNullOrWhiteSpace(UserInput))
        {
            Items.Add($"User Item: {UserInput}");
            UserInput = string.Empty;
            // No need to call NotifyCanExecuteChanged - handled by [NotifyCanExecuteChangedFor]
        }
    }

    private bool CanAddItem()
    {
        return !string.IsNullOrWhiteSpace(UserInput);
    }

    [RelayCommand]
    private void ClearItems()
    {
        Items.Clear();
    }

    [RelayCommand]
    private async Task ShowValidationExampleAsync()
    {
        if (_navigationService != null)
        {
            await _navigationService.NavigateToAsync<ModernValidationViewModel>();
        }
    }

    private async Task UpdateTimeAsync()
    {
        while (!IsBusy)
        {
            CurrentTime = DateTime.Now;
            await Task.Delay(1000);
        }
    }

    // REMOVED: No longer needed thanks to [NotifyCanExecuteChangedFor]
    // partial void OnUserInputChanged(string value)
    // {
    //     AddItemCommand.NotifyCanExecuteChanged();
    // }

    public override Task OnNavigatedToAsync(object? parameter = null)
    {
        // Called when navigated to this view
        WelcomeMessage = parameter as string ?? "Welcome to WPF MVVM with Community Toolkit!";
        return base.OnNavigatedToAsync(parameter);
    }
}