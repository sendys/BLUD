using BLUDRSUD.Domain.Common;

namespace BLUDRSUD.Domain.Entities.Master;

/// <summary>
/// Fund Source (spec section 6) — accounting dimension tracking the source of funds for
/// a transaction/journal line. For BLUD this typically includes BLUD itself, APBD transfers,
/// hibah, pendapatan sendiri, etc. Configurable per Pemda/RSUD policy.
/// </summary>
public class FundSource : BaseEntity
{
    /// <summary>Short unique code e.g. "BLUD", "APBD", "HIBAH".</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Display name e.g. "Dana BLUD".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description of the fund source.</summary>
    public string? Description { get; set; }

    /// <summary>Classification: BLUD / APBD / APBN / Hibah / Lainnya.</summary>
    public string FundCategory { get; set; } = "BLUD";

    /// <summary>If true, available for selection on journal/transaction lines.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Optional parent fund source for hierarchy/rollup.</summary>
    public Guid? ParentId { get; set; }

    public FundSource? Parent { get; set; }
    public ICollection<FundSource> Children { get; set; } = new List<FundSource>();
}
