using BLUDRSUD.Domain.Common;
using BLUDRSUD.Domain.Enums;

namespace BLUDRSUD.Domain.Entities.Master;

/// <summary>
/// Accounting Period (spec section 19). Monthly periods (2026-01 .. 2026-12) plus
/// a year-end closing period (period 13). Once Locked, transactions and journals
/// cannot be modified — corrections must use reversal/adjustment journals.
/// </summary>
public class AccountingPeriod : BaseEntity, IConcurrencyAware
{
    /// <summary>Period code e.g. "2026-01". Unique.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Fiscal year e.g. 2026.</summary>
    public int FiscalYear { get; set; }

    /// <summary>Period number 1-12 for months, 13 for year-end closing.</summary>
    public int PeriodNumber { get; set; }

    /// <summary>Display name e.g. "Januari 2026".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Inclusive start date of the period.</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Inclusive end date of the period.</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Status controlling entry/edit/posting capability.</summary>
    public PeriodStatus Status { get; set; } = PeriodStatus.Open;

    /// <summary>Monthly or YearEndClosing.</summary>
    public PeriodType PeriodType { get; set; } = PeriodType.Monthly;

    /// <summary>If true, this is the currently-active period for default transaction entry.</summary>
    public bool IsCurrent { get; set; }

    /// <summary>Who closed/locked the period; null while open.</summary>
    public string? ClosedBy { get; set; }

    /// <summary>When the period was closed/locked.</summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>Optimistic concurrency token (spec section 28). MySQL rowversion requires a fixed-length binary value.</summary>
    public byte[] RowVersion { get; set; } = new byte[8];
}
