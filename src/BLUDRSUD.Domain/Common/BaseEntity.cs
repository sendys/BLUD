namespace BLUDRSUD.Domain.Common;

/// <summary>
/// Base interface for all domain entities with audit fields.
/// Audit fields implement PSAP/Permendagri BLUD traceability requirements:
/// CreatedAt/CreatedBy/UpdatedAt/UpdatedBy/DeletedAt/DeletedBy (spec section 24).
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    string CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}

/// <summary>
/// Soft-delete marker so master data retains history (spec: soft delete untuk master data yang membutuhkan histori).
/// </summary>
public interface ISoftDeletable
{
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}

/// <summary>
/// Optimistic concurrency token on critical entities to prevent
/// double posting / duplicate payment / duplicate approval (spec section 28).
/// </summary>
public interface IConcurrencyAware
{
    byte[] RowVersion { get; set; }
}

/// <summary>
/// Base entity for all master and transactional data. Uses Guid PK for distributed-safety
/// and to avoid identity-table hotspots on high-volume journal inserts.
/// </summary>
public abstract class BaseEntity : IAuditable, ISoftDeletable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "system";
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
