using BLUDRSUD.Domain.Common;

namespace BLUDRSUD.Domain.Entities.Master;

/// <summary>
/// Cost Center (spec section 6 & 7) — accounting dimension used on journal lines.
/// A Cost Center may map 1:1 to a Service Unit (e.g. RAWAT_INAP) or to an administrative org unit.
/// Used by Reporting Engine and Accounting Rule Engine for dimension-based journal mapping.
/// </summary>
public class CostCenter : BaseEntity
{
    /// <summary>Short unique code e.g. "RAWAT_INAP", "FARMASI", "ADM".</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Parent cost center for rollup reporting (nullable = top-level).</summary>
    public Guid? ParentId { get; set; }

    public CostCenter? Parent { get; set; }
    public ICollection<CostCenter> Children { get; set; } = new List<CostCenter>();

    /// <summary>Optional link to an Organization unit.</summary>
    public Guid? OrganizationId { get; set; }

    public Organization? Organization { get; set; }

    /// <summary>If true, journal lines may post to this cost center.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Cost-center type for grouping (Operational / Support / Administrative).</summary>
    public string CostCenterType { get; set; } = "Operational";
}
