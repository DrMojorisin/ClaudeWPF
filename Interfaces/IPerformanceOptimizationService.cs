using System.Collections.ObjectModel;
using System.Windows.Controls;
using WPFBase.Services;

namespace WPFBase.Interfaces;

/// <summary>
/// Interface for performance optimization service implementing 2024 WPF best practices
/// </summary>
public interface IPerformanceOptimizationService
{
    // Object Pooling
    T GetPooledObject<T>() where T : class, new();
    void ReturnPooledObject<T>(T obj) where T : class;
    ObservableCollection<T> CreateOptimizedCollection<T>(int initialCapacity = 16);

    // Virtualization
    void OptimizeListControl(ItemsControl listControl, int estimatedItemCount = 1000);
    VirtualizedObservableCollection<T> CreateVirtualizedCollection<T>(
        Func<int, int, Task<IEnumerable<T>>> dataLoader,
        int totalCount,
        int pageSize = 50);

    // Memory Management
    // Note: Using non-generic WeakReference for compatibility with WeakReferenceTable
    // While WeakReference<T> is preferred, our internal table infrastructure expects non-generic
    WeakReference CreateWeakReference<T>(T target) where T : class;
    void PerformMemoryCleanup(bool forceGC = false);
    MemoryUsageReport GetMemoryUsageReport();

    // Async Optimization
    TaskScheduler CreateUITaskScheduler();
    Task<TResult> ExecuteWithProgressAsync<TResult>(
        Func<IProgress<double>, CancellationToken, Task<TResult>> work,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);
}