using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WPFBase.Interfaces;
using WPFBase.ViewModels;
// using WPFBase.Views.Dialogs; // Directory doesn't exist yet

namespace WPFBase.Services;

/// <summary>
/// Implementation of dialog service for WPF
/// </summary>
public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Window _owner;

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _owner = Application.Current?.MainWindow!;
    }

    public Task<MessageBoxResult> ShowMessageAsync(string message, string title = "Message", 
        MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Information)
    {
        if (string.IsNullOrEmpty(message))
            throw new ArgumentException("Message cannot be null or empty", nameof(message));

        var owner = _owner ?? Application.Current?.MainWindow;
        return Task.FromResult(MessageBox.Show(owner, message, title, buttons, icon));
    }

    public async Task<bool> ShowConfirmationAsync(string message, string title = "Confirm")
    {
        var result = await ShowMessageAsync(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    public Task ShowErrorAsync(string message, string title = "Error", Exception? exception = null)
    {
        var fullMessage = exception != null 
            ? $"{message}\n\nDetails: {exception.Message}" 
            : message;
        
        return ShowMessageAsync(fullMessage, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public Task ShowWarningAsync(string message, string title = "Warning")
    {
        return ShowMessageAsync(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public Task ShowInformationAsync(string message, string title = "Information")
    {
        return ShowMessageAsync(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public async Task<string?> ShowInputAsync(string prompt, string title = "Input", string defaultValue = "")
    {
        // Create a simple input dialog
        var inputWindow = new Window
        {
            Title = title,
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = _owner,
            ResizeMode = ResizeMode.NoResize
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.Margin = new Thickness(10);

        var promptLabel = new TextBlock
        {
            Text = prompt,
            Margin = new Thickness(0, 0, 0, 10),
            TextWrapping = TextWrapping.Wrap
        };
        Grid.SetRow(promptLabel, 0);
        grid.Children.Add(promptLabel);

        var inputTextBox = new TextBox
        {
            Text = defaultValue,
            Margin = new Thickness(0, 0, 0, 10)
        };
        Grid.SetRow(inputTextBox, 1);
        grid.Children.Add(inputTextBox);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        Grid.SetRow(buttonPanel, 2);

        var okButton = new Button
        {
            Content = "OK",
            Width = 75,
            Margin = new Thickness(0, 0, 5, 0),
            IsDefault = true
        };
        okButton.Click += (s, e) => inputWindow.DialogResult = true;
        buttonPanel.Children.Add(okButton);

        var cancelButton = new Button
        {
            Content = "Cancel",
            Width = 75,
            IsCancel = true
        };
        cancelButton.Click += (s, e) => inputWindow.DialogResult = false;
        buttonPanel.Children.Add(cancelButton);

        grid.Children.Add(buttonPanel);
        inputWindow.Content = grid;

        // Focus the input box
        inputWindow.Loaded += (s, e) =>
        {
            inputTextBox.Focus();
            inputTextBox.SelectAll();
        };

        var result = await Task.FromResult(inputWindow.ShowDialog());
        return result == true ? inputTextBox.Text : null;
    }

    public async Task<TResult?> ShowDialogAsync<TViewModel, TResult>(object? parameter = null) 
        where TViewModel : class 
        where TResult : class
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        
        // Get dialog view type by convention
        var dialogViewType = GetDialogViewType(typeof(TViewModel));
        var dialogView = CreateDialogView(dialogViewType, viewModel);

        // Show dialog
        var dialogWindow = CreateDialogWindow(dialogView, viewModel);
        
        // Initialize dialog if it implements IDialogViewModel
        if (viewModel is IDialogViewModel dialogViewModel)
        {
            await dialogViewModel.OnDialogOpenedAsync(parameter);
            
            dialogWindow.Closing += async (s, e) =>
            {
                if (!dialogViewModel.CanClose)
                {
                    e.Cancel = true;
                    return;
                }
                
                e.Cancel = !await dialogViewModel.OnClosingAsync();
            };
            
            var result = dialogWindow.ShowDialog();
            return result == true ? dialogViewModel.DialogResult as TResult : null;
        }
        
        dialogWindow.ShowDialog();
        return null;
    }

    public async Task ShowDialogAsync<TViewModel>(object? parameter = null) where TViewModel : class
    {
        await ShowDialogAsync<TViewModel, object>(parameter);
    }

    public Task<string?> ShowOpenFileDialogAsync(string filter = "All Files|*.*", string title = "Open File")
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter,
            Title = title,
            CheckFileExists = true,
            CheckPathExists = true
        };

        var result = dialog.ShowDialog(_owner);
        return Task.FromResult(result == true ? dialog.FileName : null);
    }

    public Task<string?> ShowSaveFileDialogAsync(string filter = "All Files|*.*", string title = "Save File", string defaultFileName = "")
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            Title = title,
            FileName = defaultFileName,
            CheckPathExists = true
        };

        var result = dialog.ShowDialog(_owner);
        return Task.FromResult(result == true ? dialog.FileName : null);
    }

    public Task<string?> ShowFolderBrowserAsync(string title = "Select Folder")
    {
        // Use Windows Forms FolderBrowserDialog or modern alternative
        var dialog = new OpenFileDialog
        {
            Title = title,
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = "Select Folder",
            Filter = "Folders|no.files",
            ValidateNames = false
        };

        var result = dialog.ShowDialog(_owner);
        if (result == true)
        {
            return Task.FromResult<string?>(System.IO.Path.GetDirectoryName(dialog.FileName));
        }
        
        return Task.FromResult<string?>(null);
    }

    private Type GetDialogViewType(Type viewModelType)
    {
        // Convention: DialogViewModel -> DialogView
        var viewName = viewModelType.Name.Replace("ViewModel", "View");
        var viewType = Type.GetType($"WPFBase.Views.Dialogs.{viewName}");
        
        if (viewType == null)
        {
            // Try without Dialogs namespace
            viewType = Type.GetType($"WPFBase.Views.{viewName}");
        }
        
        return viewType ?? throw new InvalidOperationException($"Dialog view for {viewModelType.Name} not found");
    }

    private UserControl CreateDialogView(Type viewType, object viewModel)
    {
        var view = (UserControl)Activator.CreateInstance(viewType)!;
        view.DataContext = viewModel;
        return view;
    }

    private Window CreateDialogWindow(UserControl content, object viewModel)
    {
        var window = new Window
        {
            Owner = _owner,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false,
            ResizeMode = ResizeMode.NoResize,
            SizeToContent = SizeToContent.WidthAndHeight,
            Content = content
        };

        // Try to get title from ViewModel
        if (viewModel is ViewModelBase vmBase)
        {
            window.Title = vmBase.Title;
        }

        // Add default styling
        window.Background = SystemColors.ControlBrush;
        window.MinWidth = 300;
        window.MinHeight = 150;

        return window;
    }
}