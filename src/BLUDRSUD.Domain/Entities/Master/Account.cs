using BLUDRSUD.Domain.Common;
using BLUDRSUD.Domain.Enums;

namespace BLUDRSUD.Domain.Entities.Master;

/// <summary>
/// Chart of Accounts (spec section 5). Hierarchical account structure following
/// Permendagri 79/2018 & PP 71/2010. Account codes and structure are configurable DATA
/// (not hard-coded) so each RSUD can map to its regional accounting policy.
///
/// Example hierarchy:
///   1 ASET (header, IsHeader=true)
///   11 ASET LANCAR (header)
///   1101 KAS (header)
///   110101 Kas Bendahara Penerimaan (posting account, AllowPosting=true)
/// </summary>
public class Account : BaseEntity, IConcurrencyAware
{
    /// <summary>Account code e.g. "110101". Unique per fiscal year set; hierarchical padding convention.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Human-readable name e.g. "Kas Bendahara Penerimaan".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Top-level classification (Asset/Liability/Equity/Revenue/Expense/Expenditure/Financing/Memorandum).</summary>
    public AccountType AccountType { get; set; }

    /// <summary>Parent account Id (null = root). Self-reference for tree traversal.</summary>
    public Guid? ParentId { get; set; }

    /// <summary>Navigational parent.</summary>
    public Account? Parent { get; set; }

    /// <summary>Child accounts for hierarchical rendering and rollup.</summary>
    public ICollection<Account> Children { get; set; } = new List<Account>();

    /// <summary>Debit = increases assets/expenses; Credit = increases liabilities/equity/revenue.</summary>
    public NormalBalance NormalBalance { get; set; }

    /// <summary>Depth in tree (1 = root). Used for indentation and rollup ordering.</summary>
    public int Level { get; set; } = 1;

    /// <summary>If true, this is a grouping header — postings NOT allowed (only leaf accounts post).</summary>
    public bool IsHeader { get; set; }

    /// <summary>If false, account is archived and excluded from new postings.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Which financial statement section this account rolls up to (Reporting Engine).</summary>
    public ReportMapping ReportMapping { get; set; } = ReportMapping.None;

    /// <summary>If true, journal lines may debit/credit this account. Headers disallow posting.</summary>
    public bool AllowPosting { get; set; } = true;

    /// <summary>Free-text foreign key to a regional/accounting-policy code (configurable per Pemda).</summary>
    public string? ExternalCode { get; set; }

    /// <summary>Optimistic concurrency token (spec section 28). MySQL rowversion requires a fixed-length binary value.</summary>
    public byte[] RowVersion { get; set; } = new byte[8];
}
