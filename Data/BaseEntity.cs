using System.ComponentModel.DataAnnotations;

namespace WPFBase.Data;

/// <summary>
/// Base class for all entities
/// </summary>
public abstract class BaseEntity<TKey> : IEntity<TKey>
{
    [Key]
    public virtual TKey Id { get; set; } = default!;
}

/// <summary>
/// Base class for entities with int ID
/// </summary>
public abstract class BaseEntity : BaseEntity<int>
{
}

/// <summary>
/// Base class for auditable entities
/// </summary>
public abstract class AuditableEntity<TKey> : BaseEntity<TKey>, IAuditableEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Base class for auditable entities with int ID
/// </summary>
public abstract class AuditableEntity : AuditableEntity<int>
{
}

/// <summary>
/// Base class for soft-deletable entities
/// </summary>
public abstract class SoftDeletableEntity<TKey> : AuditableEntity<TKey>, ISoftDeletable
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

/// <summary>
/// Base class for soft-deletable entities with int ID
/// </summary>
public abstract class SoftDeletableEntity : SoftDeletableEntity<int>
{
}