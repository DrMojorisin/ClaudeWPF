using System.Windows;
using System.Windows.Controls;
using WPFBase.ViewModels;

namespace WPFBase.Views;

/// <summary>
/// Interaction logic for ModernValidationView.xaml
/// </summary>
public partial class ModernValidationView : UserControl
{
    public ModernValidationView()
    {
        InitializeComponent();
    }

    private void PasswordField_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ModernValidationViewModel viewModel && sender is PasswordBox passwordBox)
        {
            viewModel.Password = passwordBox.Password;
        }
    }

    private void ConfirmPasswordField_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ModernValidationViewModel viewModel && sender is PasswordBox passwordBox)
        {
            viewModel.ConfirmPassword = passwordBox.Password;
        }
    }
}