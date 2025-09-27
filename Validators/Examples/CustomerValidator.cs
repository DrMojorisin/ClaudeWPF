using FluentValidation;
using WPFBase.Models.Examples;
using System;

namespace WPFBase.Validators.Examples
{
    /// <summary>
    /// Example FluentValidation validator for Customer model
    /// Shows common validation patterns
    /// </summary>
    public class CustomerValidator : AbstractValidator<Customer>
    {
        public CustomerValidator()
        {
            // Basic required field validation
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .Length(1, 50).WithMessage("First name must be between 1 and 50 characters")
                .Matches("^[a-zA-Z ]+$").WithMessage("First name can only contain letters and spaces");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .Length(1, 50).WithMessage("Last name must be between 1 and 50 characters")
                .Matches("^[a-zA-Z ]+$").WithMessage("Last name can only contain letters and spaces");

            // Email validation with custom message
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .Must(BeUniqueEmail).WithMessage("Email already exists"); // Custom validation

            // Optional field with conditional validation
            RuleFor(x => x.Phone)
                .Matches(@"^\d{3}-\d{4}$").WithMessage("Phone must be in format XXX-XXXX")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            // Date validation
            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past")
                .GreaterThan(DateTime.Today.AddYears(-120)).WithMessage("Invalid date of birth")
                .When(x => x.DateOfBirth.HasValue);

            // Nested object validation
            When(x => x.Address != null, () =>
            {
                RuleFor(x => x.Address!.Street)
                    .NotEmpty().WithMessage("Street is required");

                RuleFor(x => x.Address!.City)
                    .NotEmpty().WithMessage("City is required");

                RuleFor(x => x.Address!.State)
                    .Length(2).WithMessage("State must be 2 characters")
                    .Matches("^[A-Z]{2}$").WithMessage("State must be uppercase letters");

                RuleFor(x => x.Address!.ZipCode)
                    .Matches(@"^\d{5}(-\d{4})?$").WithMessage("Invalid ZIP code format");
            });

            // Complex business rule
            RuleFor(x => x)
                .Must(BeEligibleForService)
                .WithMessage("Customer must be at least 18 years old and active")
                .WithName("Eligibility");
        }

        // Custom validation method example
        private bool BeUniqueEmail(string email)
        {
            // In real app, check against database
            // This is just an example
            return !email.Equals("admin@example.com", StringComparison.OrdinalIgnoreCase);
        }

        // Business rule validation
        private bool BeEligibleForService(Customer customer)
        {
            if (!customer.IsActive) return false;
            if (customer.Age == null || customer.Age < 18) return false;
            return true;
        }
    }

    /// <summary>
    /// Example of a ViewModel validator using FluentValidation
    /// </summary>
    public class CustomerViewModelValidator : AbstractValidator<CustomerViewModel>
    {
        public CustomerViewModelValidator()
        {
            // Reuse model validator rules
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .Length(1, 50);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .Length(1, 50);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            // ViewModel-specific validation
            RuleFor(x => x.ConfirmEmail)
                .Equal(x => x.Email).WithMessage("Email confirmation does not match")
                .When(x => !string.IsNullOrEmpty(x.Email));

            // Password validation for new customers
            When(x => x.IsNewCustomer, () =>
            {
                RuleFor(x => x.Password)
                    .NotEmpty().WithMessage("Password is required")
                    .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                    .Matches("[A-Z]").WithMessage("Password must contain uppercase letter")
                    .Matches("[a-z]").WithMessage("Password must contain lowercase letter")
                    .Matches("[0-9]").WithMessage("Password must contain number")
                    .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain special character");

                RuleFor(x => x.ConfirmPassword)
                    .Equal(x => x.Password).WithMessage("Password confirmation does not match");
            });
        }
    }

    // Example ViewModel for validation
    public class CustomerViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ConfirmEmail { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public bool IsNewCustomer { get; set; }
    }
}