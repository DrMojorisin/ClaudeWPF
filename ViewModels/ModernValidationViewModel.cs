using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
using WPFBase.Interfaces;

namespace WPFBase.ViewModels;

/// <summary>
/// Modern validation example using CommunityToolkit.Mvvm's ObservableValidator (2024-2025 best practices)
/// </summary>
public partial class ModernValidationViewModel : ObservableValidator
{
    private readonly IDialogService _dialogService;
    private readonly IMessageBus _messageBus;

    // Simple properties with automatic validation using source generators
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    private string email = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    private int age = 18;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "Password must contain uppercase, lowercase, and digit")]
    private string password = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    [Required(ErrorMessage = "Please confirm your password")]
    [CustomValidation(typeof(ModernValidationViewModel), nameof(ValidatePasswordMatch))]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(FullName))]
    [Required(ErrorMessage = "First name is required")]
    [MinLength(2, ErrorMessage = "First name must be at least 2 characters")]
    private string firstName = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(FullName))]
    [Required(ErrorMessage = "Last name is required")]
    [MinLength(2, ErrorMessage = "Last name must be at least 2 characters")]
    [CustomValidation(typeof(ModernValidationViewModel), nameof(ValidateNamesNotSame))]
    private string lastName = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Please select a country")]
    private string country = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    [CustomValidation(typeof(ModernValidationViewModel), nameof(ValidateTermsAccepted))]
    private bool acceptTerms;

    // Computed properties
    public bool IsFormValid => !HasErrors;
    public string FullName => $"{FirstName} {LastName}".Trim();

    public ModernValidationViewModel(IDialogService dialogService, IMessageBus messageBus)
    {
        _dialogService = dialogService;
        _messageBus = messageBus;
    }

    // Custom validation methods (static for use with CustomValidation attribute)
    public static ValidationResult? ValidatePasswordMatch(string confirmPassword, ValidationContext context)
    {
        var instance = context.ObjectInstance as ModernValidationViewModel;
        if (instance != null && !string.IsNullOrEmpty(confirmPassword))
        {
            if (confirmPassword != instance.Password)
            {
                return new ValidationResult("Passwords do not match");
            }
        }
        return ValidationResult.Success;
    }

    public static ValidationResult? ValidateNamesNotSame(string lastName, ValidationContext context)
    {
        var instance = context.ObjectInstance as ModernValidationViewModel;
        if (instance != null && !string.IsNullOrEmpty(lastName) && !string.IsNullOrEmpty(instance.FirstName))
        {
            if (lastName.Equals(instance.FirstName, StringComparison.OrdinalIgnoreCase))
            {
                return new ValidationResult("First name and last name cannot be the same");
            }
        }
        return ValidationResult.Success;
    }

    public static ValidationResult? ValidateTermsAccepted(bool acceptTerms, ValidationContext context)
    {
        if (!acceptTerms)
        {
            return new ValidationResult("You must accept the terms and conditions");
        }
        return ValidationResult.Success;
    }

    // Commands using modern [RelayCommand] attribute
    [RelayCommand]
    private void ValidateAll()
    {
        ValidateAllProperties();

        var message = IsFormValid ? "All validation passed!" : "Please correct the validation errors.";
        var messageType = IsFormValid ? Models.Messages.StatusMessageType.Success : Models.Messages.StatusMessageType.Warning;

        _messageBus.Publish(new Models.Messages.StatusMessage
        {
            Text = message,
            Type = messageType
        });
    }

    [RelayCommand]
    private void ClearValidation()
    {
        ClearErrors();

        // Reset fields to defaults
        Email = string.Empty;
        Age = 18;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        Country = string.Empty;
        AcceptTerms = false;

        _messageBus.Publish(new Models.Messages.StatusMessage
        {
            Text = "Form cleared",
            Type = Models.Messages.StatusMessageType.Information
        });
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        // Final validation before submit
        ValidateAllProperties();

        if (!IsFormValid)
        {
            await _dialogService.ShowWarningAsync(
                "Please correct all validation errors before submitting.",
                "Validation Error");
            return;
        }

        var message = $"Form submitted successfully!\n\n" +
                     $"Name: {FirstName} {LastName}\n" +
                     $"Email: {Email}\n" +
                     $"Age: {Age}\n" +
                     $"Country: {Country}";

        await _dialogService.ShowInformationAsync(message, "Success");

        _messageBus.Publish(new Models.Messages.StatusMessage
        {
            Text = "Form submitted successfully",
            Type = Models.Messages.StatusMessageType.Success
        });
    }

    private bool CanSubmit() => IsFormValid && AcceptTerms;

    // Modern async command with cancellation token support (2024 pattern)
    [RelayCommand]
    private async Task LoadCountriesAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Simulate async loading with cancellation support
            await Task.Delay(1000, cancellationToken);

            // In real app, load from service
            var countries = new[] { "USA", "Canada", "UK", "Germany", "France" };

            _messageBus.Publish(new Models.Messages.StatusMessage
            {
                Text = $"Loaded {countries.Length} countries",
                Type = Models.Messages.StatusMessageType.Success
            });
        }
        catch (OperationCanceledException)
        {
            _messageBus.Publish(new Models.Messages.StatusMessage
            {
                Text = "Country loading cancelled",
                Type = Models.Messages.StatusMessageType.Warning
            });
        }
    }

    // Property change handlers (only keep those that do custom validation)
    partial void OnPasswordChanged(string value)
    {
        // When password changes, revalidate confirm password
        if (!string.IsNullOrEmpty(ConfirmPassword))
        {
            ValidateProperty(ConfirmPassword, nameof(ConfirmPassword));
        }
        // No need for SubmitCommand.NotifyCanExecuteChanged() - handled by [NotifyCanExecuteChangedFor]
    }

    partial void OnFirstNameChanged(string value)
    {
        // Revalidate last name when first name changes (for duplicate check)
        if (!string.IsNullOrEmpty(LastName))
        {
            ValidateProperty(LastName, nameof(LastName));
        }
        // FullName notification handled by [NotifyPropertyChangedFor]
    }

    // Removed: No longer needed thanks to [NotifyCanExecuteChangedFor]
    // partial void OnConfirmPasswordChanged(string value) - handled by attribute
    // partial void OnAcceptTermsChanged(bool value) - handled by attribute
}