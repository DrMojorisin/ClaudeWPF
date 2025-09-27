using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WPFBase.Controls;

/// <summary>
/// Custom control for displaying validation errors in a summary view
/// </summary>
public partial class ValidationSummary : UserControl
{
    public static readonly DependencyProperty ShowErrorsProperty =
        DependencyProperty.Register("ShowErrors", typeof(bool), typeof(ValidationSummary),
            new PropertyMetadata(true, OnShowErrorsChanged));

    public static readonly DependencyProperty ShowWarningsProperty =
        DependencyProperty.Register("ShowWarnings", typeof(bool), typeof(ValidationSummary),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasErrorsProperty =
        DependencyProperty.Register("HasErrors", typeof(bool), typeof(ValidationSummary),
            new PropertyMetadata(false));

    public static new readonly DependencyProperty DataContextProperty =
        DependencyProperty.Register("DataContext", typeof(object), typeof(ValidationSummary),
            new PropertyMetadata(null, OnDataContextChanged));

    public bool ShowErrors
    {
        get => (bool)GetValue(ShowErrorsProperty);
        set => SetValue(ShowErrorsProperty, value);
    }

    public bool ShowWarnings
    {
        get => (bool)GetValue(ShowWarningsProperty);
        set => SetValue(ShowWarningsProperty, value);
    }

    public bool HasErrors
    {
        get => (bool)GetValue(HasErrorsProperty);
        private set => SetValue(HasErrorsProperty, value);
    }

    public ValidationSummary()
    {
        InitializeComponent();
    }

    private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ValidationSummary control)
        {
            control.UpdateValidationDisplay(e.NewValue);
        }
    }

    private static void OnShowErrorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ValidationSummary control)
        {
            control.UpdateValidationDisplay(control.DataContext);
        }
    }

    private void UpdateValidationDisplay(object? dataContext)
    {
        if (!ShowErrors)
        {
            ValidationBorder.Visibility = Visibility.Collapsed;
            return;
        }

        if (dataContext is ObservableValidator viewModel)
        {
            // Listen for error changes
            viewModel.ErrorsChanged -= OnErrorsChanged;
            viewModel.ErrorsChanged += OnErrorsChanged;

            UpdateErrorsList(viewModel);
        }
        else
        {
            ValidationBorder.Visibility = Visibility.Collapsed;
        }
    }

    private void OnErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        if (sender is ObservableValidator viewModel)
        {
            Dispatcher.Invoke(() => UpdateErrorsList(viewModel));
        }
    }

    private void UpdateErrorsList(ObservableValidator viewModel)
    {
        var allErrors = new List<string>();

        // Get all property errors
        var properties = viewModel.GetType().GetProperties();
        foreach (var property in properties)
        {
            var errors = viewModel.GetErrors(property.Name);
            if (errors != null)
            {
                foreach (var error in errors)
                {
                    if (error != null)
                    {
                        // Convert any error type to string
                        allErrors.Add(error.ToString() ?? "Validation error");
                    }
                }
            }
        }

        // Get object-level errors
        var objectErrors = viewModel.GetErrors(string.Empty);
        if (objectErrors != null)
        {
            foreach (var error in objectErrors)
            {
                if (error != null)
                {
                    // Convert any error type to string
                    allErrors.Add(error.ToString() ?? "Validation error");
                }
            }
        }

        // Update UI
        if (allErrors.Any())
        {
            ErrorsList.ItemsSource = allErrors.Distinct();
            ValidationBorder.Visibility = Visibility.Visible;
        }
        else
        {
            ValidationBorder.Visibility = Visibility.Collapsed;
        }
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == DataContextProperty)
        {
            UpdateValidationDisplay(e.NewValue);
        }
    }
}