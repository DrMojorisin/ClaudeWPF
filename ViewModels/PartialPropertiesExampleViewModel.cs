using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using WPFBase.Interfaces;

namespace WPFBase.ViewModels;

/// <summary>
/// Demonstrates CommunityToolkit.Mvvm 8.4 partial properties feature
/// Showcases the latest 2024 MVVM patterns with enhanced code generation
/// </summary>
public partial class PartialPropertiesExampleViewModel : ObservableValidator
{
    private readonly IDialogService _dialogService;
    private readonly IPerformanceOptimizationService _performanceService;

    #region CommunityToolkit.Mvvm 8.4 Partial Properties

    // LATEST FEATURE: Partial properties with enhanced nullability and modifiers
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public partial string Email { get; set; } = string.Empty;

    // Partial property with custom accessibility
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

    // Partial property with override modifier (new in 8.4)
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ProcessCommand))]
    public new partial bool IsBusy { get; set; }

    // Partial property with sealed modifier support
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusDisplay))]
    public sealed partial string Status { get; set; } = "Ready";

    // Partial property with required modifier (C# 11 feature)
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    public required partial string TenantId { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Computed property that updates automatically when FirstName or LastName changes
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Validation state computed property
    /// </summary>
    public bool IsValid => !HasErrors;

    /// <summary>
    /// Status display with enhanced formatting
    /// </summary>
    public string StatusDisplay => $"Status: {Status} | {DateTime.Now:HH:mm:ss}";

    #endregion

    #region Performance-Optimized Collections

    // Use performance service for optimized collections
    public ObservableCollection<string> Items { get; }
    public ObservableCollection<ProcessingResult> Results { get; }

    #endregion

    #region Constructor with Required Properties

    /// <summary>
    /// Constructor demonstrating required property initialization
    /// </summary>
    public PartialPropertiesExampleViewModel(
        IDialogService dialogService,
        IPerformanceOptimizationService performanceService,
        string tenantId) // Required for TenantId property
    {
        _dialogService = dialogService;
        _performanceService = performanceService;

        // Initialize required property
        TenantId = tenantId;

        // Initialize performance-optimized collections
        Items = _performanceService.CreateOptimizedCollection<string>(100);
        Results = _performanceService.CreateOptimizedCollection<ProcessingResult>(50);

        Title = "Partial Properties Example";
    }

    #endregion

    #region Modern Command Patterns

    /// <summary>
    /// Save command with automatic CanExecute based on validation
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        ValidateAllProperties();

        if (HasErrors)
        {
            await _dialogService.ShowWarningAsync("Please correct validation errors before saving.");
            return;
        }

        Status = "Saving...";
        IsBusy = true;

        try
        {
            // Simulate save operation with performance optimization
            await _performanceService.ExecuteWithProgressAsync(
                async (progress, ct) =>
                {
                    for (int i = 0; i <= 100; i += 10)
                    {
                        ct.ThrowIfCancellationRequested();
                        progress.Report(i / 100.0);
                        await Task.Delay(100, ct);
                    }
                    return true;
                },
                cancellationToken: cancellationToken);

            Status = "Saved successfully";
            await _dialogService.ShowInformationAsync($"User {FullName} saved successfully!");
        }
        catch (OperationCanceledException)
        {
            Status = "Save cancelled";
        }
        catch (Exception ex)
        {
            Status = "Save failed";
            await _dialogService.ShowErrorAsync($"Save failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave() => !HasErrors && !IsBusy && !string.IsNullOrEmpty(Email);

    /// <summary>
    /// Processing command with cancellation support
    /// </summary>
    [RelayCommand(IncludeCancelCommand = true)]
    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        Status = "Processing large dataset...";
        IsBusy = true;

        try
        {
            // Demonstrate virtualized collection for large datasets
            var virtualizedData = _performanceService.CreateVirtualizedCollection<ProcessingResult>(
                LoadDataPageAsync,
                totalCount: 10000,
                pageSize: 100);

            // Load first few pages
            await virtualizedData.LoadPageAsync(0);
            await virtualizedData.LoadPageAsync(1);

            Status = "Processing completed";
            await _dialogService.ShowInformationAsync("Large dataset processed successfully using virtualization!");
        }
        catch (OperationCanceledException)
        {
            Status = "Processing cancelled";
        }
        catch (Exception ex)
        {
            Status = $"Processing failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Memory cleanup command demonstrating performance optimization
    /// </summary>
    [RelayCommand]
    private async Task OptimizeMemoryAsync()
    {
        Status = "Optimizing memory...";

        try
        {
            var beforeReport = _performanceService.GetMemoryUsageReport();
            _performanceService.PerformMemoryCleanup(forceGC: true);
            var afterReport = _performanceService.GetMemoryUsageReport();

            var message = $"""
                Memory Optimization Complete:

                Before: {beforeReport.TotalMemoryBeforeGC:N0} bytes
                After: {afterReport.TotalMemoryAfterGC:N0} bytes
                Freed: {beforeReport.MemoryFreedByGC:N0} bytes

                Object Pools: {afterReport.ObjectPoolsCount}
                Weak References: {afterReport.WeakReferencesCount}

                Recommendations:
                {string.Join("\n", afterReport.Recommendations)}
                """;

            await _dialogService.ShowInformationAsync(message, "Memory Report");
            Status = "Memory optimization completed";
        }
        catch (Exception ex)
        {
            Status = $"Memory optimization failed: {ex.Message}";
        }
    }

    #endregion

    #region Property Change Handlers

    // Partial property change handlers benefit from enhanced nullability in 8.4
    partial void OnEmailChanged(string? oldValue, string newValue)
    {
        // Custom logic when email changes
        if (!string.IsNullOrEmpty(newValue) && newValue.Contains("@"))
        {
            Status = $"Email domain: {newValue.Split('@').LastOrDefault()}";
        }
    }

    partial void OnStatusChanged(string? oldValue, string newValue)
    {
        // Log status changes for audit trail
        Items.Add($"{DateTime.Now:HH:mm:ss} - Status: {newValue}");

        // Keep only last 10 status updates for performance
        while (Items.Count > 10)
        {
            Items.RemoveAt(0);
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Simulate loading data pages for virtualization demo
    /// </summary>
    private async Task<IEnumerable<ProcessingResult>> LoadDataPageAsync(int startIndex, int count)
    {
        await Task.Delay(100); // Simulate async data loading

        return Enumerable.Range(startIndex, count)
            .Select(i => new ProcessingResult
            {
                Id = i,
                Name = $"Item {i}",
                ProcessedAt = DateTime.Now,
                Status = "Completed"
            });
    }

    #endregion
}

/// <summary>
/// Example data model for performance optimization demonstrations
/// </summary>
public class ProcessingResult
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}