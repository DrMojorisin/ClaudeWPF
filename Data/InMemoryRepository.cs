using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace WPFBase.Data;

/// <summary>
/// In-memory implementation of repository pattern for testing and prototyping
/// </summary>
public class InMemoryRepository<T> : IRepository<T> where T : class
{
    private readonly ConcurrentDictionary<object, T> _data = new();
    private readonly PropertyInfo? _idProperty;
    private int _autoIncrementId = 1;
    private readonly object _idLock = new object();

    public InMemoryRepository()
    {
        // Find Id property
        _idProperty = typeof(T).GetProperty("Id");
    }

    public Task<T?> GetByIdAsync(object id)
    {
        _data.TryGetValue(id, out var entity);
        return Task.FromResult(entity);
    }

    public Task<IEnumerable<T>> GetAllAsync()
    {
        return Task.FromResult(_data.Values.AsEnumerable());
    }

    public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        var compiled = predicate.Compile();
        var result = _data.Values.Where(compiled);
        return Task.FromResult(result);
    }

    public Task<T?> FindSingleAsync(Expression<Func<T, bool>> predicate)
    {
        var compiled = predicate.Compile();
        var result = _data.Values.FirstOrDefault(compiled);
        return Task.FromResult(result);
    }

    public Task<T> AddAsync(T entity)
    {
        if (_idProperty != null)
        {
            var id = _idProperty.GetValue(entity);
            if (id == null || (id is int intId && intId == 0))
            {
                // Auto-increment for int IDs
                if (_idProperty.PropertyType == typeof(int))
                {
                    lock (_idLock)
                    {
                        _idProperty.SetValue(entity, _autoIncrementId++);
                    }
                }
                else if (_idProperty.PropertyType == typeof(Guid))
                {
                    _idProperty.SetValue(entity, Guid.NewGuid());
                }
            }
            
            id = _idProperty.GetValue(entity);
            if (id != null)
            {
                _data[id] = entity;
            }
        }
        
        return Task.FromResult(entity);
    }

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        var added = new List<T>();
        foreach (var entity in entities)
        {
            added.Add(await AddAsync(entity));
        }
        return added;
    }

    public Task<T> UpdateAsync(T entity)
    {
        if (_idProperty != null)
        {
            var id = _idProperty.GetValue(entity);
            if (id != null && _data.ContainsKey(id))
            {
                _data[id] = entity;
                
                // Update audit fields if present
                UpdateAuditFields(entity, false);
            }
        }
        
        return Task.FromResult(entity);
    }

    public Task DeleteAsync(T entity)
    {
        if (_idProperty != null)
        {
            var id = _idProperty.GetValue(entity);
            if (id != null)
            {
                // Check for soft delete
                if (entity is ISoftDeletable softDeletable)
                {
                    softDeletable.IsDeleted = true;
                    softDeletable.DeletedAt = DateTime.UtcNow;
                    _data[id] = entity;
                }
                else
                {
                    _data.TryRemove(id, out _);
                }
            }
        }
        
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(object id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            await DeleteAsync(entity);
        }
    }

    public async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            await DeleteAsync(entity);
        }
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        var result = await FindAsync(predicate);
        return result.Any();
    }

    public Task<int> CountAsync()
    {
        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(T)))
        {
            return Task.FromResult(_data.Values.Count(e => !(e as ISoftDeletable)!.IsDeleted));
        }
        return Task.FromResult(_data.Count);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        var result = await FindAsync(predicate);
        return result.Count();
    }

    private void UpdateAuditFields(T entity, bool isNew)
    {
        if (entity is IAuditableEntity auditable)
        {
            if (isNew)
            {
                auditable.CreatedAt = DateTime.UtcNow;
                auditable.CreatedBy = Environment.UserName;
            }
            else
            {
                auditable.UpdatedAt = DateTime.UtcNow;
                auditable.UpdatedBy = Environment.UserName;
            }
        }
    }
}

/// <summary>
/// In-memory unit of work implementation
/// </summary>
public class InMemoryUnitOfWork : IUnitOfWork
{
    private readonly ConcurrentDictionary<Type, object> _repositories = new();
    private bool _disposed;

    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        return (IRepository<T>)_repositories.GetOrAdd(type, _ => new InMemoryRepository<T>());
    }

    public Task<int> SaveChangesAsync()
    {
        // In-memory implementation saves immediately
        return Task.FromResult(1);
    }

    public Task BeginTransactionAsync()
    {
        // No-op for in-memory
        return Task.CompletedTask;
    }

    public Task CommitAsync()
    {
        // No-op for in-memory
        return Task.CompletedTask;
    }

    public Task RollbackAsync()
    {
        // No-op for in-memory
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _repositories.Clear();
            _disposed = true;
        }
    }
}