using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using WPFBase.Interfaces;

namespace WPFBase.Services;

/// <summary>
/// Performance optimization service implementing 2024 WPF best practices
/// Provides virtualization, object pooling, and memory management patterns
/// </summary>
public class PerformanceOptimizationService : IPerformanceOptimizationService
{
    private readonly ILoggingService _logger;
    private readonly ConcurrentDictionary<Type, ConcurrentQueue<object>> _objectPools;
    private readonly WeakReferenceTable _weakReferences;

    public PerformanceOptimizationService(ILoggingService logger)
    {
        _logger = logger;
        _objectPools = new ConcurrentDictionary<Type, ConcurrentQueue<object>>();
        _weakReferences = new WeakReferenceTable();
    }

    #region Object Pooling (2024 Best Practice)

    /// <summary>
    /// Get object from pool or create new one
    /// Implements object pooling pattern for memory optimization
    /// </summary>
    public T GetPooledObject<T>() where T : class, new()
    {
        var type = typeof(T);
        var pool = _objectPools.GetOrAdd(type, _ => new ConcurrentQueue<object>());

        if (pool.TryDequeue(out var obj))
        {
            _logger.LogDebug("Retrieved {Type} from object pool", type.Name);
            return (T)obj;
        }

        _logger.LogDebug("Created new {Type} instance", type.Name);
        return new T();
    }

    /// <summary>
    /// Return object to pool for reuse
    /// </summary>
    public void ReturnPooledObject<T>(T obj) where T : class
    {
        if (obj == null) return;

        var type = typeof(T);
        var pool = _objectPools.GetOrAdd(type, _ => new ConcurrentQueue<object>());

        // Reset object state if it implements IPoolable
        if (obj is IPoolable poolable)
        {
            poolable.Reset();
        }

        pool.Enqueue(obj);
        _logger.LogDebug("Returned {Type} to object pool", type.Name);
    }

    /// <summary>
    /// Create a pooled collection optimized for WPF binding
    /// </summary>
    public ObservableCollection<T> CreateOptimizedCollection<T>(int initialCapacity = 16)
    {
        var collection = GetPooledObject<ObservableCollection<T>>();

        // Pre-allocate capacity for better performance
        if (collection.Count == 0 && initialCapacity > 0)
        {
            // Reserve space to avoid repeated allocations
            for (int i = 0; i < Math.Min(initialCapacity, 1000); i++)
            {
                collection.Add(default(T)!);
            }
            collection.Clear();
        }

        return collection;
    }

    #endregion

    #region Virtualization Support

    /// <summary>
    /// Configure ListView/ListBox for optimal virtualization
    /// Implements 2024 virtualization best practices
    /// </summary>
    public void OptimizeListControl(ItemsControl listControl, int estimatedItemCount = 1000)
    {
        if (listControl == null) return;

        _logger.LogDebug("Optimizing {ControlType} for {EstimatedCount} items",
            listControl.GetType().Name, estimatedItemCount);

        // Enable UI virtualization
        VirtualizingPanel.SetIsVirtualizing(listControl, true);

        // Enable container recycling for better performance
        VirtualizingPanel.SetVirtualizationMode(listControl, VirtualizationMode.Recycling);

        // Optimize scrolling behavior
        VirtualizingPanel.SetScrollUnit(listControl, ScrollUnit.Item);

        // Enable caching for better performance with large datasets
        VirtualizingPanel.SetCacheLengthUnit(listControl, VirtualizationCacheLengthUnit.Item);
        VirtualizingPanel.SetCacheLength(listControl, new VirtualizationCacheLength(20, 20));

        // For very large collections, use pixel-based scrolling
        if (estimatedItemCount > 10000)
        {
            VirtualizingPanel.SetScrollUnit(listControl, ScrollUnit.Pixel);
            _logger.LogDebug("Enabled pixel-based scrolling for large dataset");
        }
    }

    /// <summary>
    /// Create virtualized collection with lazy loading support
    /// </summary>
    public VirtualizedObservableCollection<T> CreateVirtualizedCollection<T>(
        Func<int, int, Task<IEnumerable<T>>> dataLoader,
        int totalCount,
        int pageSize = 50)
    {
        return new VirtualizedObservableCollection<T>(dataLoader, totalCount, pageSize, _logger);
    }

    #endregion

    #region Memory Management

    /// <summary>
    /// Create weak reference for memory-conscious scenarios
    /// Prevents memory leaks in large applications
    /// </summary>
    public WeakReference<T> CreateWeakReference<T>(T target) where T : class
    {
        var weakRef = new WeakReference<T>(target);
        _weakReferences.Add(weakRef);
        return weakRef;
    }

    /// <summary>
    /// Cleanup weak references and trigger GC if needed
    /// </summary>
    public void PerformMemoryCleanup(bool forceGC = false)
    {
        _logger.LogDebug("Performing memory cleanup");

        // Clean up dead weak references
        _weakReferences.CleanupDeadReferences();

        // Clean up empty object pools
        foreach (var kvp in _objectPools.ToList())
        {
            if (kvp.Value.IsEmpty)
            {
                _objectPools.TryRemove(kvp.Key, out _);
            }
        }

        if (forceGC)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            _logger.LogDebug("Forced garbage collection completed");
        }
    }

    /// <summary>
    /// Monitor memory usage and provide optimization suggestions
    /// </summary>
    public MemoryUsageReport GetMemoryUsageReport()
    {
        var beforeGC = GC.GetTotalMemory(false);
        var afterGC = GC.GetTotalMemory(true);

        return new MemoryUsageReport
        {
            TotalMemoryBeforeGC = beforeGC,
            TotalMemoryAfterGC = afterGC,
            MemoryFreedByGC = beforeGC - afterGC,
            ObjectPoolsCount = _objectPools.Count,
            WeakReferencesCount = _weakReferences.Count,
            Recommendations = GenerateMemoryRecommendations(beforeGC, afterGC)
        };
    }

    #endregion

    #region Async Optimization

    /// <summary>
    /// Create optimized task scheduler for UI operations
    /// </summary>
    public TaskScheduler CreateUITaskScheduler()
    {
        return TaskScheduler.FromCurrentSynchronizationContext();
    }

    /// <summary>
    /// Execute CPU-intensive work on background thread with progress reporting
    /// </summary>
    public async Task<TResult> ExecuteWithProgressAsync<TResult>(
        Func<IProgress<double>, CancellationToken, Task<TResult>> work,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(async () =>
        {
            var internalProgress = progress ?? new Progress<double>();
            return await work(internalProgress, cancellationToken);
        }, cancellationToken);
    }

    #endregion

    private List<string> GenerateMemoryRecommendations(long beforeGC, long afterGC)
    {
        var recommendations = new List<string>();
        var freedMemory = beforeGC - afterGC;

        if (freedMemory > 50 * 1024 * 1024) // 50MB
        {
            recommendations.Add("Consider more frequent cleanup - significant memory was freed by GC");
        }

        if (_objectPools.Count > 20)
        {
            recommendations.Add("Consider consolidating object pools - many types are being pooled");
        }

        if (afterGC > 500 * 1024 * 1024) // 500MB
        {
            recommendations.Add("High memory usage detected - consider using virtualization for large collections");
        }

        return recommendations;
    }
}

/// <summary>
/// Interface for objects that can be pooled and reused
/// </summary>
public interface IPoolable
{
    void Reset();
}

/// <summary>
/// Memory usage report for optimization insights
/// </summary>
public class MemoryUsageReport
{
    public long TotalMemoryBeforeGC { get; set; }
    public long TotalMemoryAfterGC { get; set; }
    public long MemoryFreedByGC { get; set; }
    public int ObjectPoolsCount { get; set; }
    public int WeakReferencesCount { get; set; }
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Virtualized collection implementation for large datasets
/// </summary>
public class VirtualizedObservableCollection<T> : ObservableCollection<T>
{
    private readonly Func<int, int, Task<IEnumerable<T>>> _dataLoader;
    private readonly int _totalCount;
    private readonly int _pageSize;
    private readonly ILoggingService _logger;
    private readonly Dictionary<int, bool> _loadedPages = new();

    public VirtualizedObservableCollection(
        Func<int, int, Task<IEnumerable<T>>> dataLoader,
        int totalCount,
        int pageSize,
        ILoggingService logger)
    {
        _dataLoader = dataLoader;
        _totalCount = totalCount;
        _pageSize = pageSize;
        _logger = logger;

        // Initialize with placeholder items
        for (int i = 0; i < totalCount; i++)
        {
            Add(default(T)!);
        }
    }

    public async Task LoadPageAsync(int pageIndex)
    {
        if (_loadedPages.ContainsKey(pageIndex)) return;

        var startIndex = pageIndex * _pageSize;
        var count = Math.Min(_pageSize, _totalCount - startIndex);

        try
        {
            var items = await _dataLoader(startIndex, count);
            var itemList = items.ToList();

            for (int i = 0; i < itemList.Count; i++)
            {
                this[startIndex + i] = itemList[i];
            }

            _loadedPages[pageIndex] = true;
            _logger.LogDebug("Loaded page {PageIndex} with {Count} items", pageIndex, itemList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load page {PageIndex}", pageIndex);
        }
    }
}

/// <summary>
/// Manages weak references and provides cleanup functionality
/// </summary>
internal class WeakReferenceTable
{
    private readonly List<WeakReference> _references = new();
    private readonly object _lock = new();

    public void Add(WeakReference reference)
    {
        lock (_lock)
        {
            _references.Add(reference);
        }
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _references.Count;
            }
        }
    }

    public void CleanupDeadReferences()
    {
        lock (_lock)
        {
            for (int i = _references.Count - 1; i >= 0; i--)
            {
                if (!_references[i].IsAlive)
                {
                    _references.RemoveAt(i);
                }
            }
        }
    }
}