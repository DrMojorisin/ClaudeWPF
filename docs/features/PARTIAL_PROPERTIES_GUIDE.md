# Partial Properties Guide - CommunityToolkit.Mvvm 8.4+ (2024-2025)

## Executive Summary

**Partial properties** are the bleeding-edge 2024-2025 MVVM pattern that combines C# 11 features with CommunityToolkit.Mvvm 8.4's enhanced source generators. They eliminate 90% of boilerplate code while providing superior compile-time safety and IntelliSense support.

**Status**: Production-ready (WPFBase implementation validated)
**Feature**: CommunityToolkit.Mvvm 8.4.0 partial properties
**Example**: `ViewModels/PartialPropertiesExampleViewModel.cs`

## Overview - What Are Partial Properties?

### The Evolution of MVVM Properties

```csharp
// 2010: Manual implementation (100 lines)
private string _email;
public string Email
{
    get => _email;
    set
    {
        if (_email != value)
        {
            _email = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsValid));
            SaveCommand.NotifyCanExecuteChanged();
            ValidateProperty(value);
        }
    }
}

// 2020: CommunityToolkit.Mvvm 7.x (30 lines)
[ObservableProperty]
private string _email;

partial void OnEmailChanged(string value)
{
    OnPropertyChanged(nameof(IsValid));
    SaveCommand.NotifyCanExecuteChanged();
}

// 2024-2025: Partial properties 8.4+ (1 line!)
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(IsValid))]
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
[NotifyDataErrorInfo]
[Required]
[EmailAddress]
public partial string Email { get; set; } = string.Empty;
```

**Key Innovation**: Properties declared as `public partial` with source generators providing the full implementation automatically.

## Why Partial Properties?

### Traditional vs Partial Properties Comparison

| Feature | Old Pattern | Partial Properties | Benefit |
|---------|------------|-------------------|---------|
| **Declaration** | Private field + attributes | Direct property declaration | 50% less code |
| **IntelliSense** | Shows backing field in completion | Shows actual property | Better developer experience |
| **Refactoring** | Rename field + property separately | Single rename operation | Safer refactoring |
| **Nullability** | Field annotation only | Property-level annotation | More precise null handling |
| **Accessibility** | Always public property | Custom accessibility possible | Better encapsulation |
| **Validation** | Manual or attributes | Integrated attributes | Cleaner validation |
| **Change tracking** | Manual notifications | Declarative attributes | Zero boilerplate |

### Real-World Code Reduction

**Before (Traditional Pattern - 45 lines):**
```csharp
private string _firstName;
private string _lastName;
private string _email;

[Required]
[MinLength(2)]
public string FirstName
{
    get => _firstName;
    set
    {
        if (SetProperty(ref _firstName, value))
        {
            OnPropertyChanged(nameof(FullName));
        }
    }
}

[Required]
[MinLength(2)]
public string LastName
{
    get => _lastName;
    set
    {
        if (SetProperty(ref _lastName, value))
        {
            OnPropertyChanged(nameof(FullName));
        }
    }
}

[Required]
[EmailAddress]
public string Email
{
    get => _email;
    set
    {
        if (SetProperty(ref _email, value))
        {
            ValidateProperty(value);
            SaveCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(IsValid));
        }
    }
}
```

**After (Partial Properties - 12 lines):**
```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(FullName))]
[Required]
[MinLength(2)]
public partial string FirstName { get; set; } = string.Empty;

[ObservableProperty]
[NotifyPropertyChangedFor(nameof(FullName))]
[Required]
[MinLength(2)]
public partial string LastName { get; set; } = string.Empty;

[ObservableProperty]
[NotifyDataErrorInfo]
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
[NotifyPropertyChangedFor(nameof(IsValid))]
[Required]
[EmailAddress]
public partial string Email { get; set; } = string.Empty;
```

**Result**: 73% code reduction with identical functionality!

## Basic Pattern - Simple Partial Property

### Minimal Example

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

public partial class SimpleViewModel : ObservableObject
{
    // Single line declaration - generator creates full implementation
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    // Generated code (invisible to you):
    // - Backing field
    // - Property getter/setter
    // - INotifyPropertyChanged implementation
    // - Null handling
    // - Change notifications
}
```

### What Gets Generated

The source generator automatically creates:

```csharp
// You write this:
[ObservableProperty]
public partial string Email { get; set; } = string.Empty;

// Generator produces (conceptual - actual implementation is optimized):
private string _email = string.Empty;

public string Email
{
    get => _email;
    set
    {
        if (!EqualityComparer<string>.Default.Equals(_email, value))
        {
            OnEmailChanging(value);
            _email = value;
            OnEmailChanged(value);
            OnPropertyChanged(nameof(Email));
        }
    }
}

partial void OnEmailChanging(string value);
partial void OnEmailChanged(string value);
```

### Default Value Initialization

```csharp
// String with default value
[ObservableProperty]
public partial string Title { get; set; } = "Untitled";

// Numeric with default
[ObservableProperty]
public partial int Count { get; set; } = 0;

// Boolean with default
[ObservableProperty]
public partial bool IsEnabled { get; set; } = true;

// Object with default
[ObservableProperty]
public partial DateTime CreatedAt { get; set; } = DateTime.Now;

// Nullable with null default
[ObservableProperty]
public partial string? OptionalValue { get; set; } = null;

// Collection initialization
[ObservableProperty]
public partial ObservableCollection<string> Items { get; set; } = new();
```

## Attribute Stacking - The Power Pattern

### Multiple Attributes Work Together

Attributes stack to create sophisticated behaviors declaratively:

```csharp
[ObservableProperty]                              // 1. Make property observable
[NotifyPropertyChangedFor(nameof(IsValid))]       // 2. Update IsValid when this changes
[NotifyPropertyChangedFor(nameof(StatusDisplay))] // 3. Update StatusDisplay too
[NotifyCanExecuteChangedFor(nameof(SaveCommand))] // 4. Update command CanExecute
[NotifyDataErrorInfo]                             // 5. Enable validation
[Required(ErrorMessage = "Email is required")]    // 6. Validation rule 1
[EmailAddress(ErrorMessage = "Invalid format")]   // 7. Validation rule 2
public partial string Email { get; set; } = string.Empty;
```

**Order doesn't matter** - all attributes are processed correctly regardless of order.

### Complete Attribute Reference

#### Core Attribute: ObservableProperty

```csharp
[ObservableProperty]
public partial string Name { get; set; } = string.Empty;
```

**Purpose**: Marks property for source generation
**Required**: Yes (always first in mental model, but order doesn't matter)
**Generates**: Full property implementation with INPC

#### Property Change Notifications

```csharp
// Update other properties when this changes
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(FullName))]
[NotifyPropertyChangedFor(nameof(DisplayName))]
public partial string FirstName { get; set; } = string.Empty;

// Computed property that auto-updates
public string FullName => $"{FirstName} {LastName}".Trim();
```

**Purpose**: Automatically raise PropertyChanged for dependent properties
**Use Case**: Computed properties, derived values, UI-specific formatting
**Performance**: Zero overhead - compiled into direct notifications

#### Command State Updates

```csharp
[ObservableProperty]
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
[NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
public partial string SelectedId { get; set; } = string.Empty;

[RelayCommand(CanExecute = nameof(CanSave))]
private async Task SaveAsync() { /* ... */ }

private bool CanSave() => !string.IsNullOrEmpty(SelectedId);
```

**Purpose**: Automatically update command CanExecute state
**Use Case**: Buttons that enable/disable based on property values
**Benefit**: Commands stay in sync automatically - no manual NotifyCanExecuteChanged calls

#### Validation Integration

```csharp
[ObservableProperty]
[NotifyDataErrorInfo]  // Essential for validation
[Required(ErrorMessage = "Name is required")]
[MinLength(2, ErrorMessage = "Minimum 2 characters")]
[MaxLength(50, ErrorMessage = "Maximum 50 characters")]
public partial string Name { get; set; } = string.Empty;
```

**Purpose**: Integrate DataAnnotations validation automatically
**Requires**: ViewModel inherits from `ObservableValidator`
**Benefit**: Real-time validation with zero manual code

### Complete Working Example

```csharp
public partial class UserFormViewModel : ObservableValidator
{
    // Email: validation + command updates + computed property updates
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email")]
    public partial string Email { get; set; } = string.Empty;

    // First name: validation + updates full name
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    [Required]
    [MinLength(2)]
    public partial string FirstName { get; set; } = string.Empty;

    // Last name: validation + updates full name
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    [Required]
    [MinLength(2)]
    public partial string LastName { get; set; } = string.Empty;

    // Status: updates command + computed display
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(StatusDisplay))]
    public partial string Status { get; set; } = "Ready";

    // Busy state: disables commands
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    public partial bool IsBusy { get; set; }

    // Computed properties - automatically updated
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsValid => !HasErrors;
    public string StatusDisplay => $"Status: {Status} | {DateTime.Now:HH:mm:ss}";

    // Command with automatic state management
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        IsBusy = true; // Automatically disables SaveCommand
        try
        {
            Status = "Saving..."; // Automatically updates StatusDisplay
            await Task.Delay(1000);
            Status = "Saved successfully";
        }
        finally
        {
            IsBusy = false; // Automatically re-enables SaveCommand
        }
    }

    private bool CanSave() => !HasErrors && !IsBusy && !string.IsNullOrEmpty(Email);
}
```

**What this achieves without ANY manual code:**
- Email changes trigger validation
- Email changes update IsValid property
- Email changes enable/disable SaveCommand
- FirstName/LastName changes update FullName
- Status changes update StatusDisplay
- IsBusy changes enable/disable ALL commands
- All validation runs automatically
- All UI updates happen automatically

## Validation Integration - DataAnnotations with Partial Properties

### Prerequisites for Validation

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

// REQUIRED: Inherit from ObservableValidator (not ObservableObject)
public partial class ValidatedViewModel : ObservableValidator
{
    // REQUIRED: [NotifyDataErrorInfo] attribute on validated properties
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    public partial string Email { get; set; } = string.Empty;
}
```

### Built-in Validation Attributes

```csharp
public partial class ValidationExampleViewModel : ObservableValidator
{
    // Required field
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public partial string Email { get; set; } = string.Empty;

    // String length constraints
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [MinLength(2, ErrorMessage = "Minimum 2 characters")]
    [MaxLength(50, ErrorMessage = "Maximum 50 characters")]
    public partial string Username { get; set; } = string.Empty;

    // Numeric range
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public partial int Age { get; set; }

    // Regular expression pattern
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [RegularExpression(@"^\d{5}(-\d{4})?$",
        ErrorMessage = "Please enter a valid ZIP code")]
    public partial string ZipCode { get; set; } = string.Empty;

    // Phone number
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    public partial string PhoneNumber { get; set; } = string.Empty;

    // URL validation
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Url(ErrorMessage = "Please enter a valid URL")]
    public partial string Website { get; set; } = string.Empty;

    // Credit card (basic validation)
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CreditCard(ErrorMessage = "Please enter a valid credit card number")]
    public partial string CardNumber { get; set; } = string.Empty;
}
```

### Custom Validation with Static Methods

```csharp
public partial class PasswordViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [MinLength(8)]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [CustomValidation(typeof(PasswordViewModel), nameof(ValidatePasswordMatch))]
    public partial string ConfirmPassword { get; set; } = string.Empty;

    // Custom validation method (must be static)
    public static ValidationResult? ValidatePasswordMatch(
        string confirmPassword,
        ValidationContext context)
    {
        var instance = context.ObjectInstance as PasswordViewModel;

        if (string.IsNullOrEmpty(confirmPassword))
            return new ValidationResult("Please confirm password");

        if (confirmPassword != instance?.Password)
            return new ValidationResult("Passwords do not match");

        return ValidationResult.Success;
    }
}
```

### Validation in Commands

```csharp
[RelayCommand(CanExecute = nameof(CanSubmit))]
private async Task SubmitAsync()
{
    // Validate all properties before processing
    ValidateAllProperties();

    // Check if any validation errors exist
    if (HasErrors)
    {
        await _dialogService.ShowWarningAsync(
            "Please correct validation errors before submitting.");
        return;
    }

    // Process valid data
    await ProcessFormAsync();
}

private bool CanSubmit()
{
    // Disable button if form has errors or is busy
    return !HasErrors && !IsBusy;
}

// Alternative: Validate single property
partial void OnEmailChanged(string value)
{
    ValidateProperty(value, nameof(Email));
}
```

### XAML Validation Display

```xml
<TextBox Text="{Binding Email, UpdateSourceTrigger=PropertyChanged, ValidatesOnNotifyDataErrors=True}">
    <Validation.ErrorTemplate>
        <ControlTemplate>
            <DockPanel>
                <TextBlock DockPanel.Dock="Bottom"
                          Foreground="Red"
                          FontSize="11"
                          Text="{Binding [0].ErrorContent}"/>
                <Border BorderBrush="Red" BorderThickness="1">
                    <AdornedElementPlaceholder/>
                </Border>
            </DockPanel>
        </ControlTemplate>
    </Validation.ErrorTemplate>
</TextBox>

<!-- Or use WPF-UI validation styling -->
<ui:TextBox Text="{Binding Email, UpdateSourceTrigger=PropertyChanged, ValidatesOnNotifyDataErrors=True}"
            Header="Email Address"
            PlaceholderText="Enter your email"/>
```

### Accessing Validation Errors

```csharp
// Check if specific property has errors
if (GetErrors(nameof(Email)).Any())
{
    var errors = GetErrors(nameof(Email)).Cast<ValidationResult>();
    var errorMessage = string.Join(", ", errors.Select(e => e.ErrorMessage));
}

// Check all validation errors
if (HasErrors)
{
    var allErrors = GetErrors()
        .Cast<ValidationResult>()
        .Select(e => e.ErrorMessage);

    await _dialogService.ShowErrorAsync(
        string.Join("\n", allErrors),
        "Validation Failed");
}
```

## Required Properties - C# 11 Pattern

### The Required Modifier

C# 11 introduced the `required` keyword for properties that MUST be initialized:

```csharp
public partial class ConfigViewModel : ObservableValidator
{
    // Required property - MUST be set during object creation
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    public required partial string TenantId { get; set; }

    // Optional properties - can be initialized later
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;
}
```

### Required Property Rules

**1. Must use object initializer (constructor parameters don't work):**

```csharp
// Correct initialization
var viewModel = new ConfigViewModel(dialogService)
{
    TenantId = "tenant-123"  // MUST be provided
};

// Compiler error - TenantId not set
var viewModel = new ConfigViewModel(dialogService); // ERROR!

// Constructor parameters DON'T satisfy required properties
public ConfigViewModel(
    IDialogService dialogService,
    string tenantId)  // This does NOT satisfy required property
{
    _dialogService = dialogService;
    // TenantId = tenantId; // Would need explicit assignment
}
```

**2. Dependency injection requires special handling:**

```csharp
// Option 1: Factory pattern
services.AddTransient<Func<string, ConfigViewModel>>(sp => tenantId =>
{
    var dialog = sp.GetRequiredService<IDialogService>();
    return new ConfigViewModel(dialog) { TenantId = tenantId };
});

// Usage:
var factory = serviceProvider.GetRequiredService<Func<string, ConfigViewModel>>();
var viewModel = factory("tenant-123");

// Option 2: Post-construction initialization
services.AddTransient<ConfigViewModel>();

// Usage:
var viewModel = serviceProvider.GetRequiredService<ConfigViewModel>();
viewModel.TenantId = "tenant-123"; // Set after construction
```

**3. Derived classes must initialize required base properties:**

```csharp
public abstract partial class BaseViewModel : ObservableValidator
{
    [ObservableProperty]
    public required partial string Id { get; set; }
}

public partial class DerivedViewModel : BaseViewModel
{
    // Inherits required Id property - callers must set it
}

// Usage - must set all required properties from entire hierarchy
var viewModel = new DerivedViewModel()
{
    Id = "derived-123"  // Required from base class
};
```

### When to Use Required Properties

**Good use cases:**
- Configuration values that MUST exist (TenantId, ApiKey, etc.)
- Mandatory context for ViewModel (UserId, SessionId, etc.)
- Non-nullable reference types that can't have reasonable defaults

**Avoid when:**
- Using dependency injection without factories
- Value can be loaded asynchronously later
- Reasonable default value exists
- Need backward compatibility with existing code

### Required vs Validation Attributes

```csharp
// Required MODIFIER - Compile-time enforcement
[ObservableProperty]
public required partial string TenantId { get; set; }
// Compiler error if not initialized during construction

// Required ATTRIBUTE - Runtime validation
[ObservableProperty]
[NotifyDataErrorInfo]
[Required]
public partial string Email { get; set; } = string.Empty;
// Validation error if empty after construction

// Both together - maximum safety
[ObservableProperty]
[NotifyDataErrorInfo]
[Required(ErrorMessage = "TenantId is required")]
public required partial string TenantId { get; set; }
// Compile-time + runtime enforcement
```

## Computed Properties - Dependent Properties That Auto-Update

### Basic Computed Property Pattern

```csharp
public partial class UserViewModel : ObservableValidator
{
    // Source properties with auto-update attributes
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    public partial string FirstName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    public partial string LastName { get; set; } = string.Empty;

    // Computed property - read-only expression
    public string FullName => $"{FirstName} {LastName}".Trim();
}
```

**How it works:**
1. FirstName or LastName changes
2. Source generator automatically raises PropertyChanged for FullName
3. UI re-evaluates FullName expression
4. Display updates automatically

### Multiple Dependencies

```csharp
public partial class OrderViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Total))]
    [NotifyPropertyChangedFor(nameof(Tax))]
    [NotifyPropertyChangedFor(nameof(GrandTotal))]
    [NotifyPropertyChangedFor(nameof(DisplaySummary))]
    public partial decimal Subtotal { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GrandTotal))]
    [NotifyPropertyChangedFor(nameof(DisplaySummary))]
    public partial decimal Discount { get; set; }

    // Computed properties cascade automatically
    public decimal Total => Subtotal - Discount;
    public decimal Tax => Total * 0.08m;
    public decimal GrandTotal => Total + Tax;

    // Complex computed property
    public string DisplaySummary =>
        $"Subtotal: {Subtotal:C}\n" +
        $"Discount: {Discount:C}\n" +
        $"Tax: {Tax:C}\n" +
        $"Total: {GrandTotal:C}";
}
```

### Computed Boolean Properties for UI State

```csharp
public partial class FormViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [NotifyPropertyChangedFor(nameof(IsInvalid))]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    [NotifyDataErrorInfo]
    [Required]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    public partial bool IsBusy { get; set; }

    // Computed UI state properties
    public bool IsValid => !HasErrors;
    public bool IsInvalid => HasErrors;
    public bool CanSubmit => IsValid && !IsBusy;
    public string StatusMessage => IsBusy
        ? "Processing..."
        : IsValid
            ? "Ready to submit"
            : "Please correct errors";
}
```

**XAML usage:**
```xml
<Button Content="Submit"
        Command="{Binding SubmitCommand}"
        IsEnabled="{Binding CanSubmit}"/>

<TextBlock Text="{Binding StatusMessage}"
          Foreground="{Binding IsValid, Converter={StaticResource BoolToColorConverter}}"/>
```

### Computed Properties with Collections

```csharp
public partial class ShoppingCartViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ItemCount))]
    [NotifyPropertyChangedFor(nameof(HasItems))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(TotalItems))]
    public partial ObservableCollection<CartItem> Items { get; set; } = new();

    // Computed properties based on collection
    public int ItemCount => Items?.Count ?? 0;
    public bool HasItems => ItemCount > 0;
    public bool IsEmpty => ItemCount == 0;
    public int TotalItems => Items?.Sum(i => i.Quantity) ?? 0;

    // Must manually trigger updates when collection contents change
    private void OnItemQuantityChanged()
    {
        OnPropertyChanged(nameof(TotalItems));
    }
}
```

**Note**: Collection property changes trigger notifications, but changes WITHIN the collection (adding/removing items) require manual notification or CollectionChanged event handling.

### Performance Considerations

```csharp
public partial class PerformanceViewModel : ObservableObject
{
    // Expensive computed property
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExpensiveCalculation))]
    public partial ObservableCollection<DataItem> Data { get; set; } = new();

    // Bad: Recalculates every time UI requests it
    public double ExpensiveCalculation =>
        Data.Select(d => d.ComplexOperation()).Sum(); // Runs every time!

    // Good: Cache the result
    private double _cachedResult;

    public double CachedCalculation => _cachedResult;

    partial void OnDataChanged(ObservableCollection<DataItem> value)
    {
        RecalculateExpensiveOperation(); // Calculate once
    }

    private void RecalculateExpensiveOperation()
    {
        _cachedResult = Data.Select(d => d.ComplexOperation()).Sum();
        OnPropertyChanged(nameof(CachedCalculation));
    }
}
```

## Property Change Handlers - Partial Void Methods

### Basic Change Handler Pattern

```csharp
public partial class NotificationViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    // Auto-generated partial method - implement to handle changes
    partial void OnEmailChanged(string value)
    {
        // Called AFTER Email property has changed
        // 'value' is the NEW value
        // Old value not directly available (use overload if needed)

        Console.WriteLine($"Email changed to: {value}");
    }

    // Two-parameter overload for old and new values
    partial void OnEmailChanged(string oldValue, string newValue)
    {
        Console.WriteLine($"Email changed from '{oldValue}' to '{newValue}'");
    }
}
```

### Available Partial Methods

For each partial property, the source generator creates TWO partial methods:

```csharp
[ObservableProperty]
public partial string Status { get; set; } = "Ready";

// Generated partial method signatures (you can implement either or both):

// 1. OnPropertyChanging - called BEFORE value changes
partial void OnStatusChanging(string value);

// 2. OnPropertyChanged - called AFTER value changes
partial void OnStatusChanged(string value);

// Alternative overloads with old value:
partial void OnStatusChanging(string oldValue, string newValue);
partial void OnStatusChanged(string oldValue, string newValue);
```

### Practical Use Cases

#### 1. Logging Changes (Audit Trail)

```csharp
public partial class AuditedViewModel : ObservableObject
{
    private readonly ILoggingService _logger;

    [ObservableProperty]
    public partial string Status { get; set; } = "Ready";

    [ObservableProperty]
    public partial ObservableCollection<string> AuditLog { get; set; } = new();

    partial void OnStatusChanged(string oldValue, string newValue)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var entry = $"{timestamp} - Status: {oldValue} → {newValue}";

        AuditLog.Add(entry);
        _logger.LogInformation(entry);

        // Keep only last 50 entries
        while (AuditLog.Count > 50)
            AuditLog.RemoveAt(0);
    }
}
```

#### 2. Derived Value Calculation

```csharp
public partial class EmailViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmailDomain { get; set; } = string.Empty;

    partial void OnEmailChanged(string value)
    {
        // Extract domain when email changes
        if (!string.IsNullOrEmpty(value) && value.Contains("@"))
        {
            EmailDomain = value.Split('@').LastOrDefault() ?? string.Empty;
        }
        else
        {
            EmailDomain = string.Empty;
        }
    }
}
```

#### 3. Side Effects and Notifications

```csharp
public partial class MessagingViewModel : ObservableObject
{
    private readonly IMessageBus _messageBus;

    [ObservableProperty]
    public partial UserData SelectedUser { get; set; }

    partial void OnSelectedUserChanged(UserData? oldValue, UserData? newValue)
    {
        // Unsubscribe from old user events
        if (oldValue != null)
        {
            oldValue.DataChanged -= OnUserDataChanged;
        }

        // Subscribe to new user events
        if (newValue != null)
        {
            newValue.DataChanged += OnUserDataChanged;
        }

        // Notify other parts of application
        _messageBus.Publish(new UserSelectionChangedMessage
        {
            OldUser = oldValue,
            NewUser = newValue
        });
    }
}
```

#### 4. Validation Triggering

```csharp
public partial class ValidationViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    public partial string ConfirmEmail { get; set; } = string.Empty;

    partial void OnEmailChanged(string value)
    {
        // Re-validate ConfirmEmail when Email changes
        // (ensures "emails must match" validation updates)
        if (!string.IsNullOrEmpty(ConfirmEmail))
        {
            ValidateProperty(ConfirmEmail, nameof(ConfirmEmail));
        }
    }

    partial void OnConfirmEmailChanged(string value)
    {
        // Validate immediately as user types
        ValidateProperty(value, nameof(ConfirmEmail));
    }
}
```

#### 5. Debouncing and Throttling

```csharp
using System.Reactive.Linq;

public partial class SearchViewModel : ObservableObject
{
    private readonly ISearchService _searchService;
    private CancellationTokenSource _searchCts;

    [ObservableProperty]
    public partial string SearchTerm { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<SearchResult> Results { get; set; } = new();

    partial void OnSearchTermChanged(string value)
    {
        // Cancel previous search
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();

        // Debounce: Wait 300ms after user stops typing
        Task.Delay(300, _searchCts.Token)
            .ContinueWith(async t =>
            {
                if (!t.IsCanceled)
                {
                    var results = await _searchService.SearchAsync(value, _searchCts.Token);
                    Results.Clear();
                    Results.AddRange(results);
                }
            }, _searchCts.Token);
    }
}
```

### Timing: Changing vs Changed

```csharp
partial void OnValueChanging(string oldValue, string newValue)
{
    // Called BEFORE property changes
    // 'this.Value' still contains oldValue
    // Can validate/cancel here (though cancellation requires custom code)

    if (newValue.Length > 100)
    {
        // Can't actually cancel, but can log warning
        _logger.LogWarning("Value exceeds maximum length");
    }
}

partial void OnValueChanged(string oldValue, string newValue)
{
    // Called AFTER property changes
    // 'this.Value' now contains newValue
    // Use for side effects, notifications, derived values

    UpdateDerivedValues();
}
```

### Performance Note

Partial methods have ZERO performance cost when not implemented. The compiler completely removes them from generated code if you don't provide an implementation.

## Commands Integration - CanExecute That Responds to Property Changes

### Basic Command with Auto-Updating CanExecute

```csharp
public partial class FormViewModel : ObservableValidator
{
    // Property that affects command state
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]  // Magic attribute
    [NotifyDataErrorInfo]
    [Required]
    public partial string Name { get; set; } = string.Empty;

    // Command automatically re-evaluates CanExecute when Name changes
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        await _dataService.SaveAsync(Name);
    }

    private bool CanSave() => !string.IsNullOrEmpty(Name) && !HasErrors;
}
```

**What happens automatically:**
1. User types in Name textbox
2. Name property setter fires
3. `[NotifyCanExecuteChangedFor]` automatically calls `SaveCommand.NotifyCanExecuteChanged()`
4. WPF evaluates `CanSave()` method
5. Save button enables/disables based on result

**Zero manual code required!**

### Multiple Properties Affecting Single Command

```csharp
public partial class MultiPropertyViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    [NotifyDataErrorInfo]
    [Required]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    [NotifyDataErrorInfo]
    [Required]
    [MinLength(8)]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    public partial bool AgreedToTerms { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    public partial bool IsBusy { get; set; }

    // Command responds to ALL four properties
    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        IsBusy = true; // Automatically disables SubmitCommand
        try
        {
            await _authService.RegisterAsync(Email, Password);
        }
        finally
        {
            IsBusy = false; // Automatically re-enables SubmitCommand
        }
    }

    private bool CanSubmit() =>
        !string.IsNullOrEmpty(Email) &&
        !string.IsNullOrEmpty(Password) &&
        AgreedToTerms &&
        !IsBusy &&
        !HasErrors;
}
```

### Single Property Affecting Multiple Commands

```csharp
public partial class MultiCommandViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveAndCloseCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExportCommand))]
    public partial bool HasUnsavedChanges { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveAndCloseCommand))]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExportCommand))]
    public partial bool IsBusy { get; set; }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync() { /* ... */ }
    private bool CanSave() => HasUnsavedChanges && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanSaveAndClose))]
    private async Task SaveAndCloseAsync() { /* ... */ }
    private bool CanSaveAndClose() => HasUnsavedChanges && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportAsync() { /* ... */ }
    private bool CanExport() => !HasUnsavedChanges && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync() { /* ... */ }
    private bool CanLoad() => !IsBusy;
}
```

### Commands with Computed Property Dependencies

```csharp
public partial class ComputedCanExecuteViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyDataErrorInfo]
    [Required]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyDataErrorInfo]
    [Required]
    public partial string Name { get; set; } = string.Empty;

    // Computed property
    public bool IsValid => !HasErrors;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync() { /* ... */ }

    // CanExecute can use computed property
    private bool CanSave() => IsValid && !IsBusy;
}
```

### Advanced: IAsyncRelayCommand for Automatic IsBusy

```csharp
public partial class AsyncCommandViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ProcessCommand))]
    public partial string InputData { get; set; } = string.Empty;

    // AsyncRelayCommand has built-in IsRunning property
    [RelayCommand(CanExecute = nameof(CanProcess))]
    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        // ProcessCommand.IsRunning automatically becomes true
        await Task.Delay(2000, cancellationToken);
        // ProcessCommand.IsRunning automatically becomes false
    }

    private bool CanProcess()
    {
        // Can check if command is already running
        return !string.IsNullOrEmpty(InputData) &&
               (ProcessCommand == null || !ProcessCommand.IsRunning);
    }
}
```

### XAML Binding Examples

```xml
<!-- Basic command binding with auto-updating CanExecute -->
<Button Content="Save"
        Command="{Binding SaveCommand}"/>

<!-- Button automatically disables when CanSave() returns false -->
<Button Content="Save and Close"
        Command="{Binding SaveAndCloseCommand}"/>

<!-- Command with parameter -->
<Button Content="Delete"
        Command="{Binding DeleteCommand}"
        CommandParameter="{Binding SelectedItem}"/>

<!-- Async command with cancellation -->
<StackPanel>
    <Button Content="Start Processing"
            Command="{Binding ProcessCommand}"/>
    <Button Content="Cancel"
            Command="{Binding ProcessCancelCommand}"/>
</StackPanel>

<!-- Status binding shows command state -->
<TextBlock Text="{Binding ProcessCommand.IsRunning,
                  Converter={StaticResource BoolToStatusConverter}}"/>
```

### Performance Note

`[NotifyCanExecuteChangedFor]` is extremely efficient:
- Directly calls `command.NotifyCanExecuteChanged()`
- No reflection or string lookups at runtime
- Compiled into direct method calls
- Zero performance overhead vs manual implementation

### Migration from Manual Pattern

**Before (manual notifications):**
```csharp
private string _name;
public string Name
{
    get => _name;
    set
    {
        if (SetProperty(ref _name, value))
        {
            SaveCommand.NotifyCanExecuteChanged();        // Manual
            SaveAndCloseCommand.NotifyCanExecuteChanged(); // Manual
            ExportCommand.NotifyCanExecuteChanged();       // Manual
        }
    }
}
```

**After (automatic notifications):**
```csharp
[ObservableProperty]
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
[NotifyCanExecuteChangedFor(nameof(SaveAndCloseCommand))]
[NotifyCanExecuteChangedFor(nameof(ExportCommand))]
public partial string Name { get; set; } = string.Empty;
```

**Result**: 70% less code, zero chance of forgetting to update a command!

## Claude Code Examples - Prompts for Generating Perfect MVVM Code

### Example 1: Basic Form ViewModel

**Prompt to Claude:**
```
Create a UserFormViewModel using WPFBase partial properties pattern with:
- FirstName, LastName, Email properties with validation
- FullName computed property
- IsValid computed property
- SaveCommand that validates before saving
- All properties should automatically update commands and computed properties
```

**Expected Result:**
```csharp
public partial class UserFormViewModel : ObservableValidator
{
    private readonly IDialogService _dialogService;
    private readonly IUserService _userService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "First name is required")]
    [MinLength(2, ErrorMessage = "Minimum 2 characters")]
    public partial string FirstName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Last name is required")]
    [MinLength(2, ErrorMessage = "Minimum 2 characters")]
    public partial string LastName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email")]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial bool IsBusy { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsValid => !HasErrors;

    public UserFormViewModel(
        IDialogService dialogService,
        IUserService userService)
    {
        _dialogService = dialogService;
        _userService = userService;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors)
        {
            await _dialogService.ShowWarningAsync("Please correct validation errors.");
            return;
        }

        IsBusy = true;
        try
        {
            await _userService.SaveUserAsync(FirstName, LastName, Email);
            await _dialogService.ShowInformationAsync($"User {FullName} saved successfully!");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Save failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave() => !HasErrors && !IsBusy && IsValid;
}
```

### Example 2: Search ViewModel with Debouncing

**Prompt to Claude:**
```
Create a SearchViewModel using WPFBase partial properties with:
- SearchTerm property that triggers debounced search after 300ms
- Results collection
- IsSearching status property
- SearchCommand with cancellation support
- ClearCommand to reset search
- Use property change handler for debouncing logic
```

**Expected Result:**
```csharp
public partial class SearchViewModel : ObservableObject
{
    private readonly ISearchService _searchService;
    private CancellationTokenSource _searchCts;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearCommand))]
    public partial string SearchTerm { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResults))]
    public partial ObservableCollection<SearchResult> Results { get; set; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    public partial bool IsSearching { get; set; }

    public bool HasResults => Results.Count > 0;

    public SearchViewModel(ISearchService searchService)
    {
        _searchService = searchService;
    }

    partial void OnSearchTermChanged(string value)
    {
        // Cancel previous search
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();

        if (string.IsNullOrWhiteSpace(value))
        {
            Results.Clear();
            return;
        }

        // Debounce: Wait 300ms after user stops typing
        Task.Delay(300, _searchCts.Token)
            .ContinueWith(async t =>
            {
                if (!t.IsCanceled)
                {
                    await SearchCommand.ExecuteAsync(null);
                }
            }, _searchCts.Token);
    }

    [RelayCommand(CanExecute = nameof(CanSearch), IncludeCancelCommand = true)]
    private async Task SearchAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(SearchTerm)) return;

        IsSearching = true;
        try
        {
            var results = await _searchService.SearchAsync(SearchTerm, cancellationToken);
            Results.Clear();
            foreach (var result in results)
            {
                Results.Add(result);
            }
        }
        catch (OperationCanceledException)
        {
            // Search cancelled - ignore
        }
        finally
        {
            IsSearching = false;
        }
    }

    private bool CanSearch() => !IsSearching;

    [RelayCommand(CanExecute = nameof(CanClear))]
    private void Clear()
    {
        SearchTerm = string.Empty;
        Results.Clear();
        _searchCts?.Cancel();
    }

    private bool CanClear() => !string.IsNullOrEmpty(SearchTerm) || Results.Count > 0;
}
```

### Example 3: Settings ViewModel with Persistence

**Prompt to Claude:**
```
Create a SettingsViewModel using WPFBase partial properties with:
- Theme (string), FontSize (int), AutoSave (bool) properties
- All properties should mark HasUnsavedChanges when modified
- SaveCommand to persist settings
- ResetCommand to restore defaults
- Property change handlers to log changes
- Validation on FontSize (10-24 range)
```

**Expected Result:**
```csharp
public partial class SettingsViewModel : ObservableValidator
{
    private readonly IUserSettingsService _settingsService;
    private readonly ILoggingService _logger;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial string Theme { get; set; } = "Light";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyDataErrorInfo]
    [Range(10, 24, ErrorMessage = "Font size must be between 10 and 24")]
    public partial int FontSize { get; set; } = 12;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial bool AutoSave { get; set; } = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial bool IsBusy { get; set; }

    private bool _isLoading;
    public bool HasUnsavedChanges => !_isLoading && (!string.Equals(Theme, _originalTheme) ||
                                                      FontSize != _originalFontSize ||
                                                      AutoSave != _originalAutoSave);

    private string _originalTheme;
    private int _originalFontSize;
    private bool _originalAutoSave;

    public SettingsViewModel(
        IUserSettingsService settingsService,
        ILoggingService logger)
    {
        _settingsService = settingsService;
        _logger = logger;
        LoadSettings();
    }

    private void LoadSettings()
    {
        _isLoading = true;

        Theme = _settingsService.GetSetting("Theme", "Light");
        FontSize = _settingsService.GetSetting("FontSize", 12);
        AutoSave = _settingsService.GetSetting("AutoSave", true);

        _originalTheme = Theme;
        _originalFontSize = FontSize;
        _originalAutoSave = AutoSave;

        _isLoading = false;
        OnPropertyChanged(nameof(HasUnsavedChanges));
    }

    partial void OnThemeChanged(string oldValue, string newValue)
    {
        _logger.LogInformation($"Theme changed from {oldValue} to {newValue}");
    }

    partial void OnFontSizeChanged(int oldValue, int newValue)
    {
        _logger.LogInformation($"Font size changed from {oldValue} to {newValue}");
    }

    partial void OnAutoSaveChanged(bool oldValue, bool newValue)
    {
        _logger.LogInformation($"Auto-save changed from {oldValue} to {newValue}");
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        IsBusy = true;
        try
        {
            _settingsService.SetSetting("Theme", Theme);
            _settingsService.SetSetting("FontSize", FontSize);
            _settingsService.SetSetting("AutoSave", AutoSave);

            await _settingsService.SaveAsync();

            _originalTheme = Theme;
            _originalFontSize = FontSize;
            _originalAutoSave = AutoSave;

            OnPropertyChanged(nameof(HasUnsavedChanges));
            _logger.LogInformation("Settings saved successfully");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave() => HasUnsavedChanges && !HasErrors && !IsBusy;

    [RelayCommand]
    private void Reset()
    {
        Theme = "Light";
        FontSize = 12;
        AutoSave = true;
    }
}
```

### Key Patterns for Claude Code Prompts

**Always specify:**
1. "using WPFBase partial properties pattern"
2. What properties need validation
3. What properties affect other properties (computed properties)
4. What properties should update which commands
5. Required services from constructor
6. Commands with their enable/disable logic

**Claude will automatically:**
- Use `[ObservableProperty]` on all properties
- Add appropriate `[NotifyCanExecuteChangedFor]` attributes
- Add appropriate `[NotifyPropertyChangedFor]` attributes
- Include `[NotifyDataErrorInfo]` when validation is mentioned
- Implement proper validation attributes
- Create computed properties as read-only expressions
- Follow IsBusy pattern for async operations
- Add proper error handling with dialog service
- Use CancellationToken for async operations

## Migration Guide - Converting Old Properties to Partial Properties

### Step 1: Identify Old Pattern Properties

**Look for these patterns in your ViewModels:**

```csharp
// Pattern 1: Manual SetProperty
private string _name;
public string Name
{
    get => _name;
    set => SetProperty(ref _name, value);
}

// Pattern 2: Manual notifications
private string _email;
public string Email
{
    get => _email;
    set
    {
        if (SetProperty(ref _email, value))
        {
            OnPropertyChanged(nameof(IsValid));
            SaveCommand.NotifyCanExecuteChanged();
        }
    }
}

// Pattern 3: Manual validation
private string _phone;
[Required]
public string Phone
{
    get => _phone;
    set
    {
        if (SetProperty(ref _phone, value))
        {
            ValidateProperty(value);
        }
    }
}
```

### Step 2: Convert to Partial Properties

**Pattern 1 → Partial Property (Simple):**

```csharp
// Before:
private string _name;
public string Name
{
    get => _name;
    set => SetProperty(ref _name, value);
}

// After:
[ObservableProperty]
public partial string Name { get; set; } = string.Empty;
```

**Pattern 2 → Partial Property (With Notifications):**

```csharp
// Before:
private string _email;
public string Email
{
    get => _email;
    set
    {
        if (SetProperty(ref _email, value))
        {
            OnPropertyChanged(nameof(IsValid));
            SaveCommand.NotifyCanExecuteChanged();
        }
    }
}

// After:
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(IsValid))]
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
public partial string Email { get; set; } = string.Empty;
```

**Pattern 3 → Partial Property (With Validation):**

```csharp
// Before:
private string _phone;
[Required]
public string Phone
{
    get => _phone;
    set
    {
        if (SetProperty(ref _phone, value))
        {
            ValidateProperty(value);
        }
    }
}

// After:
[ObservableProperty]
[NotifyDataErrorInfo]  // Automatically validates
[Required]
public partial string Phone { get; set; } = string.Empty;
```

### Step 3: Update ViewModel Base Class

**Ensure you're inheriting from the correct base:**

```csharp
// Before (if using validation):
public class MyViewModel : ValidatableViewModelBase
{
    // Old pattern properties
}

// After:
public partial class MyViewModel : ObservableValidator
{
    // Partial properties
}

// Before (no validation):
public class MyViewModel : ViewModelBase
{
    // Old pattern properties
}

// After:
public partial class MyViewModel : ObservableObject
{
    // Partial properties
}
```

**CRITICAL**: Add `partial` keyword to class declaration!

### Step 4: Move Validation Attributes

```csharp
// Before: Attributes on public property
private string _email;

[Required]
[EmailAddress]
public string Email
{
    get => _email;
    set => SetPropertyWithValidation(ref _email, value);
}

// After: Attributes on partial property
[ObservableProperty]
[NotifyDataErrorInfo]
[Required]
[EmailAddress]
public partial string Email { get; set; } = string.Empty;
```

### Step 5: Convert Property Change Handlers

```csharp
// Before: Manual handler with manual calls
private string _status;
public string Status
{
    get => _status;
    set
    {
        if (SetProperty(ref _status, value))
        {
            // Custom logic here
            LogStatusChange(value);
        }
    }
}

// After: Partial property + partial void
[ObservableProperty]
public partial string Status { get; set; } = "Ready";

partial void OnStatusChanged(string value)
{
    // Custom logic here
    LogStatusChange(value);
}
```

### Step 6: Update Computed Properties

```csharp
// Before: Manual notification in dependent properties
private string _firstName;
public string FirstName
{
    get => _firstName;
    set
    {
        if (SetProperty(ref _firstName, value))
        {
            OnPropertyChanged(nameof(FullName));
        }
    }
}

private string _lastName;
public string LastName
{
    get => _lastName;
    set
    {
        if (SetProperty(ref _lastName, value))
        {
            OnPropertyChanged(nameof(FullName));
        }
    }
}

public string FullName => $"{FirstName} {LastName}".Trim();

// After: Automatic notification via attributes
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(FullName))]
public partial string FirstName { get; set; } = string.Empty;

[ObservableProperty]
[NotifyPropertyChangedFor(nameof(FullName))]
public partial string LastName { get; set; } = string.Empty;

public string FullName => $"{FirstName} {LastName}".Trim();
```

### Step 7: Handle Collections

```csharp
// Before:
private ObservableCollection<string> _items = new();
public ObservableCollection<string> Items
{
    get => _items;
    set => SetProperty(ref _items, value);
}

// After: Initialize directly in property
[ObservableProperty]
public partial ObservableCollection<string> Items { get; set; } = new();

// Or if initialized elsewhere:
public ObservableCollection<string> Items { get; }

public MyViewModel()
{
    Items = new ObservableCollection<string>();
}
```

### Step 8: Remove Obsolete Code

After conversion, you can delete:

```csharp
// DELETE: Old backing fields
// private string _name;
// private string _email;
// private int _age;

// DELETE: Old property implementations
// (replaced by partial properties)

// DELETE: Manual NotifyCanExecuteChanged calls in property setters
// SaveCommand.NotifyCanExecuteChanged();

// DELETE: Manual OnPropertyChanged calls
// OnPropertyChanged(nameof(OtherProperty));

// DELETE: SetPropertyWithValidation calls
// (replaced by [NotifyDataErrorInfo])
```

### Complete Migration Example

**Before (Old Pattern - 80 lines):**

```csharp
using System.ComponentModel.DataAnnotations;

public class OldViewModel : ValidatableViewModelBase
{
    private readonly IDialogService _dialogService;

    private string _firstName;
    private string _lastName;
    private string _email;
    private bool _isBusy;

    [Required]
    [MinLength(2)]
    public string FirstName
    {
        get => _firstName;
        set
        {
            if (SetPropertyWithValidation(ref _firstName, value))
            {
                OnPropertyChanged(nameof(FullName));
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    [Required]
    [MinLength(2)]
    public string LastName
    {
        get => _lastName;
        set
        {
            if (SetPropertyWithValidation(ref _lastName, value))
            {
                OnPropertyChanged(nameof(FullName));
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    [Required]
    [EmailAddress]
    public string Email
    {
        get => _email;
        set
        {
            if (SetPropertyWithValidation(ref _email, value))
            {
                OnPropertyChanged(nameof(IsValid));
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsValid => !HasErrors;

    public IRelayCommand SaveCommand { get; }

    public OldViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        SaveCommand = new RelayCommand(ExecuteSave, CanSave);
    }

    private bool CanSave() => !HasErrors && !IsBusy;

    private async void ExecuteSave()
    {
        // Save logic
    }
}
```

**After (Partial Properties - 35 lines):**

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;

public partial class ModernViewModel : ObservableValidator
{
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyDataErrorInfo]
    [Required]
    [MinLength(2)]
    public partial string FirstName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyDataErrorInfo]
    [Required]
    [MinLength(2)]
    public partial string LastName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyDataErrorInfo]
    [Required]
    [EmailAddress]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial bool IsBusy { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsValid => !HasErrors;

    public ModernViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        // Save logic
    }

    private bool CanSave() => !HasErrors && !IsBusy;
}
```

**Result**: 56% code reduction + better maintainability!

### Migration Checklist

- [ ] Add `partial` keyword to class declaration
- [ ] Update base class (`ObservableValidator` or `ObservableObject`)
- [ ] Convert each property to partial property pattern
- [ ] Add `[ObservableProperty]` to all properties
- [ ] Add `[NotifyDataErrorInfo]` to validated properties
- [ ] Add `[NotifyPropertyChangedFor]` for computed property dependencies
- [ ] Add `[NotifyCanExecuteChangedFor]` for command dependencies
- [ ] Move validation attributes to partial properties
- [ ] Convert property change handlers to `partial void OnXChanged`
- [ ] Update commands to use `[RelayCommand]` if needed
- [ ] Delete old backing fields
- [ ] Delete old property implementations
- [ ] Delete manual notification calls
- [ ] Build and verify all properties work correctly
- [ ] Test validation still works
- [ ] Test commands enable/disable correctly
- [ ] Test computed properties update correctly

## Best Practices - Do's and Don'ts

### Do's (Follow These Patterns)

#### 1. Always Use Partial Keyword

```csharp
// DO: Partial class with partial properties
public partial class MyViewModel : ObservableValidator
{
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;
}

// DON'T: Missing partial keyword
public class MyViewModel : ObservableValidator // ERROR!
{
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;
}
```

#### 2. Initialize All Non-Nullable Properties

```csharp
// DO: Explicit initialization
[ObservableProperty]
public partial string Name { get; set; } = string.Empty;

[ObservableProperty]
public partial ObservableCollection<string> Items { get; set; } = new();

// DON'T: Leave non-nullable without default
[ObservableProperty]
public partial string Name { get; set; } // Warning CS8618
```

#### 3. Stack Notification Attributes Appropriately

```csharp
// DO: Comprehensive notification chain
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(FullName))]
[NotifyPropertyChangedFor(nameof(DisplayName))]
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
[NotifyDataErrorInfo]
[Required]
public partial string FirstName { get; set; } = string.Empty;

// DON'T: Forget notifications that should happen
[ObservableProperty]
[Required]
public partial string FirstName { get; set; } = string.Empty;
// Missing: Updates to FullName won't trigger!
```

#### 4. Use ObservableValidator for Validation

```csharp
// DO: Inherit from ObservableValidator for validation
public partial class ValidatedViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    public partial string Email { get; set; } = string.Empty;
}

// DON'T: Use ObservableObject with validation attributes
public partial class InvalidViewModel : ObservableObject
{
    [ObservableProperty]
    [Required] // Won't work without ObservableValidator!
    public partial string Email { get; set; } = string.Empty;
}
```

#### 5. Prefer Partial Void Handlers Over Complex Setters

```csharp
// DO: Clean partial property + handler
[ObservableProperty]
public partial string Status { get; set; } = "Ready";

partial void OnStatusChanged(string value)
{
    LogStatusChange(value);
    UpdateStatusDisplay();
}

// DON'T: Try to add logic to partial property setter
[ObservableProperty]
public partial string Status
{
    get;
    set
    {
        // ERROR: Can't add custom logic here!
        LogStatusChange(value);
    }
}
```

#### 6. Use Computed Properties for Derived Values

```csharp
// DO: Read-only computed property
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(FullName))]
public partial string FirstName { get; set; } = string.Empty;

public string FullName => $"{FirstName} {LastName}".Trim();

// DON'T: Store computed value as property
[ObservableProperty]
public partial string FullName { get; set; } = string.Empty; // Wasteful + error-prone
```

### Don'ts (Avoid These Patterns)

#### 1. Don't Mix Old and New Patterns

```csharp
// DON'T: Mix manual properties with partial properties
public partial class MixedViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty; // New way

    private string _email;
    public string Email // Old way - AVOID
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }
}

// DO: Convert everything to partial properties
public partial class ConsistentViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;
}
```

#### 2. Don't Manually Call NotifyCanExecuteChanged

```csharp
// DON'T: Manual command notifications
[ObservableProperty]
public partial string Name { get; set; } = string.Empty;

partial void OnNameChanged(string value)
{
    SaveCommand.NotifyCanExecuteChanged(); // UNNECESSARY!
}

// DO: Use attribute
[ObservableProperty]
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
public partial string Name { get; set; } = string.Empty;
```

#### 3. Don't Validate Manually When Attributes Work

```csharp
// DON'T: Manual validation in handler
[ObservableProperty]
public partial string Email { get; set; } = string.Empty;

partial void OnEmailChanged(string value)
{
    ValidateProperty(value, nameof(Email)); // UNNECESSARY!
}

// DO: Use [NotifyDataErrorInfo]
[ObservableProperty]
[NotifyDataErrorInfo]
[Required]
[EmailAddress]
public partial string Email { get; set; } = string.Empty;
// Validation happens automatically!
```

#### 4. Don't Create Backing Fields for Partial Properties

```csharp
// DON'T: Manual backing field
private string _name; // UNNECESSARY!

[ObservableProperty]
public partial string Name { get; set; } = string.Empty;
// Source generator creates backing field automatically!

// DO: Just the partial property
[ObservableProperty]
public partial string Name { get; set; } = string.Empty;
```

#### 5. Don't Use Partial Properties for Simple Non-Notifying Properties

```csharp
// DON'T: Overkill for simple properties
[ObservableProperty]
public partial string ConstantValue { get; set; } = "Never Changes";

// DO: Use simple property
public string ConstantValue { get; } = "Never Changes";

// DON'T: Partial property for private field
[ObservableProperty]
private partial string _internalState { get; set; } = string.Empty;

// DO: Use normal private field
private string _internalState = string.Empty;
```

#### 6. Don't Forget Required Property Initialization

```csharp
// DON'T: Forget to initialize required property
public partial class BadViewModel : ObservableObject
{
    [ObservableProperty]
    public required partial string TenantId { get; set; }
}

var vm = new BadViewModel(); // COMPILER ERROR!

// DO: Always initialize required properties
var vm = new BadViewModel
{
    TenantId = "tenant-123" // REQUIRED
};
```

#### 7. Don't Use Partial Properties in Non-Partial Classes

```csharp
// DON'T: Missing partial keyword on class
public class NonPartialViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty; // ERROR!
}

// DO: Class must be partial
public partial class PartialViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty; // Works!
}
```

### Performance Best Practices

#### 1. Cache Expensive Computed Properties

```csharp
// DON'T: Expensive computation every time
public string ExpensiveComputation =>
    Items.Select(x => x.Calculate()).OrderBy(x => x).First(); // Recalculates every access!

// DO: Cache and invalidate
private string _cachedResult = string.Empty;
public string CachedComputation => _cachedResult;

[ObservableProperty]
[NotifyPropertyChangedFor(nameof(CachedComputation))]
private partial ObservableCollection<Item> Items { get; set; }

partial void OnItemsChanged(ObservableCollection<Item> value)
{
    RecalculateCache();
}
```

#### 2. Limit Notification Chains

```csharp
// DON'T: Too many cascading notifications
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(Prop1))]
[NotifyPropertyChangedFor(nameof(Prop2))]
[NotifyPropertyChangedFor(nameof(Prop3))]
[NotifyPropertyChangedFor(nameof(Prop4))]
[NotifyPropertyChangedFor(nameof(Prop5))]
public partial string Source { get; set; } = string.Empty;
// Consider if all these notifications are really needed

// DO: Only notify what's necessary
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(DisplayValue))]
public partial string Source { get; set; } = string.Empty;
```

### Testing Best Practices

#### 1. Test Property Change Notifications

```csharp
[Fact]
public void FirstName_WhenChanged_NotifiesFullName()
{
    // Arrange
    var vm = new UserViewModel();
    var propertyChangedRaised = false;
    vm.PropertyChanged += (s, e) =>
    {
        if (e.PropertyName == nameof(vm.FullName))
            propertyChangedRaised = true;
    };

    // Act
    vm.FirstName = "John";

    // Assert
    Assert.True(propertyChangedRaised);
}
```

#### 2. Test Command CanExecute Updates

```csharp
[Fact]
public void Email_WhenChanged_UpdatesSaveCommandCanExecute()
{
    // Arrange
    var vm = new FormViewModel();
    var canExecuteChanged = false;
    vm.SaveCommand.CanExecuteChanged += (s, e) => canExecuteChanged = true;

    // Act
    vm.Email = "test@example.com";

    // Assert
    Assert.True(canExecuteChanged);
}
```

#### 3. Test Validation

```csharp
[Fact]
public void Email_WhenInvalid_HasValidationError()
{
    // Arrange
    var vm = new FormViewModel();

    // Act
    vm.Email = "invalid-email";
    vm.ValidateAllProperties();

    // Assert
    Assert.True(vm.HasErrors);
    Assert.True(vm.GetErrors(nameof(vm.Email)).Any());
}
```

## Summary

**Partial properties represent the future of MVVM in WPF:**

- **90% less boilerplate** compared to traditional patterns
- **Compile-time safety** with full IntelliSense support
- **Declarative programming** through attribute stacking
- **Zero runtime overhead** through source generation
- **Production-ready** in CommunityToolkit.Mvvm 8.4+

**Key Takeaways:**

1. Always declare classes as `partial` when using partial properties
2. Stack attributes to create sophisticated behaviors declaratively
3. Use `ObservableValidator` base class for validation scenarios
4. Leverage `[NotifyCanExecuteChangedFor]` for automatic command updates
5. Use `[NotifyPropertyChangedFor]` for computed property updates
6. Initialize all non-nullable properties with defaults
7. Use `partial void OnPropertyChanged` handlers for custom logic
8. Prefer computed properties over stored derived values

**Migration Path:**

1. Convert ViewModels one at a time
2. Start with simple properties, then tackle complex ones
3. Test thoroughly after each conversion
4. Remove old backing fields and manual notification code
5. Enjoy 70%+ code reduction with identical functionality

**This is the recommended pattern for ALL new WPFBase ViewModels going forward.**

For complete working examples, see: `C:\DEVELOPMENT\Projects\WPFBase\ViewModels\PartialPropertiesExampleViewModel.cs`