# Validators

This folder contains FluentValidation validators for business rule validation.

## Purpose
- Define complex validation rules
- Separate validation logic from ViewModels
- Provide detailed error messages and validation logic

## FluentValidation
This project uses FluentValidation library for robust validation:

```csharp
public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("Please enter a valid email address");

        RuleFor(x => x.Age)
            .GreaterThan(0).WithMessage("Age must be greater than 0")
            .LessThan(150).WithMessage("Age must be realistic");
    }
}
```

## Integration with ViewModels

### Option 1: Built-in Validation Attributes
For simple validation, use data annotations:

```csharp
public partial class SimpleViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(50)]
    private string name = string.Empty;
}
```

### Option 2: FluentValidation (Recommended for Complex Rules)
For complex business rules, use FluentValidation:

```csharp
public partial class ComplexViewModel : FluentValidatableViewModelBase<ComplexViewModel>
{
    [ObservableProperty]
    private string email = string.Empty;

    // Validator is automatically discovered and applied
}

public class ComplexViewModelValidator : AbstractValidator<ComplexViewModel>
{
    public ComplexViewModelValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(BeUniqueEmail).WithMessage("Email already exists");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken token)
    {
        // Async validation logic
        return await CheckEmailUniqueness(email);
    }
}
```

## Advanced Validation Features

### Conditional Validation
```csharp
RuleFor(x => x.CompanyName)
    .NotEmpty()
    .When(x => x.IsBusinessCustomer);
```

### Cross-Property Validation
```csharp
RuleFor(x => x.ConfirmPassword)
    .Equal(x => x.Password)
    .WithMessage("Passwords must match");
```

### Custom Validators
```csharp
RuleFor(x => x.PhoneNumber)
    .Must(BeValidPhoneNumber)
    .WithMessage("Please enter a valid phone number");

private bool BeValidPhoneNumber(string phoneNumber)
{
    // Custom validation logic
    return Regex.IsMatch(phoneNumber, @"^\+?[\d\s\-\(\)]+$");
}
```

## Best Practices
- One validator per model/ViewModel
- Use meaningful error messages
- Leverage async validation for database checks
- Group related rules using `RuleSet`
- Use `When()` for conditional validation