namespace BLUDRSUD.Domain.Enums;

/// <summary>
/// Status of an accounting period (spec section 19). Workflow: Open → Review → Closing → Locked.
/// Transactions and journals cannot be modified once a period is Locked; corrections use reversal/adjustment.
/// </summary>
public enum PeriodStatus
{
    /// <summary>Open for transaction entry and posting.</summary>
    Open = 1,

    /// <summary>Under period-end review; no new postings allowed.</summary>
    Review = 2,

    /// <summary>Closing in progress; awaiting year-end allocations.</summary>
    Closing = 3,

    /// <summary>Fully closed/locked. Reversal only via adjustment journals.</summary>
    Locked = 4
}

/// <summary>
/// Type of accounting period: monthly operational periods vs year-end closing periods.
/// </summary>
public enum PeriodType
{
    /// <summary>Monthly fiscal period (01–12).</summary>
    Monthly = 1,

    /// <summary>Year-end closing period (period 13) used for closing temporary accounts.</summary>
    YearEndClosing = 2
}
