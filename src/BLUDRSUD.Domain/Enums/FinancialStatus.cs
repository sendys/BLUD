namespace BLUDRSUD.Domain.Enums;

/// <summary>
/// Status of a posted journal (spec section 4). POST journals are immutable;
/// corrections use Reversal or Correction statuses.
/// </summary>
public enum JournalStatus
{
    /// <summary>Draft — editable, not posted to General Ledger.</summary>
    Draft = 1,

    /// <summary>Validated (Debit = Credit) and ready for posting.</summary>
    Validated = 2,

    /// <summary>Posted to General Ledger — immutable.</summary>
    Posted = 3,

    /// <summary>Reversal of a previously posted journal (creates counter-entries).</summary>
    Reversed = 4,

    /// <summary>Corrected by an adjustment journal.</summary>
    Corrected = 5,

    /// <summary>Cancelled (draft only).</summary>
    Cancelled = 6
}

/// <summary>
/// Source module that emitted the accounting event (spec section 16).
/// Used by the Accounting Rule Engine to match AccountingRule.EventType.
/// </summary>
public enum SourceModule
{
    Manual = 0,
    GeneralLedger = 1,
    Revenue = 2,
    Receivable = 3,
    CashReceipt = 4,
    CashDisbursement = 5,
    BankTransfer = 6,
    Purchase = 7,
    Payable = 8,
    Inventory = 9,
    FixedAsset = 10,
    Depreciation = 11,
    Payroll = 12,
    Tax = 13,
    Budget = 14,
    Closing = 15,
    Reconciliation = 16
}

/// <summary>
/// RBA workflow status (spec section 8). Draft → Review → Approval → Approved → Revision → Final.
/// </summary>
public enum RbaStatus
{
    Draft = 1,
    Review = 2,
    Approval = 3,
    Approved = 4,
    Revision = 5,
    Final = 6
}
