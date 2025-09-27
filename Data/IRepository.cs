using System.Linq.Expressions;

namespace WPFBase.Data;

/// <summary>
/// Generic repository interface for data access
/// </summary>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Get entity by ID
    /// </summary>
    Task<T?> GetByIdAsync(object id);
    
    /// <summary>
    /// Get all entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();
    
    /// <summary>
    /// Find entities matching a predicate
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// Find single entity matching a predicate
    /// </summary>
    Task<T?> FindSingleAsync(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// Add new entity
    /// </summary>
    Task<T> AddAsync(T entity);
    
    /// <summary>
    /// Add multiple entities
    /// </summary>
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    
    /// <summary>
    /// Update existing entity
    /// </summary>
    Task<T> UpdateAsync(T entity);
    
    /// <summary>
    /// Delete entity
    /// </summary>
    Task DeleteAsync(T entity);
    
    /// <summary>
    /// Delete entity by ID
    /// </summary>
    Task DeleteAsync(object id);
    
    /// <summary>
    /// Delete multiple entities
    /// </summary>
    Task DeleteRangeAsync(IEnumerable<T> entities);
    
    /// <summary>
    /// Check if entity exists
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// Count entities
    /// </summary>
    Task<int> CountAsync();
    
    /// <summary>
    /// Count entities matching predicate
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
}

/// <summary>
/// Unit of Work interface for managing transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Get repository for entity type
    /// </summary>
    IRepository<T> Repository<T>() where T : class;
    
    /// <summary>
    /// Save all changes
    /// </summary>
    Task<int> SaveChangesAsync();
    
    /// <summary>
    /// Begin transaction
    /// </summary>
    Task BeginTransactionAsync();
    
    /// <summary>
    /// Commit transaction
    /// </summary>
    Task CommitAsync();
    
    /// <summary>
    /// Rollback transaction
    /// </summary>
    Task RollbackAsync();
}

/// <summary>
/// Base entity interface
/// </summary>
public interface IEntity<TKey>
{
    TKey Id { get; set; }
}

/// <summary>
/// Auditable entity interface
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}

/// <summary>
/// Soft-deletable entity interface
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}