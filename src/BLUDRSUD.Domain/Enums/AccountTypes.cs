namespace BLUDRSUD.Domain.Enums;

/// <summary>
/// Chart of Accounts top-level classification following SAP berbasis akrual (PP 71/2010) and
/// Permendagri 79/2018 BLUD structure (spec section 5). Numbers are NOT used in DB storage;
/// stored as string/enum for configurability — actual account codes are data, not hard-coded.
/// </summary>
public enum AccountType
{
    /// <summary>1 ASET — current and non-current assets.</summary>
    Asset = 1,

    /// <summary>2 KEWAJIBAN — liabilities.</summary>
    Liability = 2,

    /// <summary>3 EKUITAS — equity / SAL.</summary>
    Equity = 3,

    /// <summary>4 PENDAPATAN — accrual revenue (Laporan Operasional).</summary>
    Revenue = 4,

    /// <summary>5 BEBAN — accrual expense (Laporan Operasional).</summary>
    Expense = 5,

    /// <summary>6 BELANJA — LRA budgetary expenditure (LRA).</summary>
    Expenditure = 6,

    /// <summary>7 PEMBIAYAAN — LRA financing.</summary>
    Financing = 7,

    /// <summary>LRA Pendapatan — budgetary revenue (LRA).</summary>
    RevenueLra = 8,

    /// <summary>LRA Belanja — budgetary expenditure (LRA, mirror of Expenditure where applicable).</summary>
    ExpenditureLra = 9,

    /// <summary>Akun memorandum — off-balance disclosures (spec section 5).</summary>
    Memorandum = 99
}

/// <summary>
/// Normal balance direction of an account; determines whether debit or credit increases the balance.
/// Critical for Trial Balance sign and report rendering.
/// </summary>
public enum NormalBalance
{
    /// <summary>Assets & expenses increase on debit.</summary>
    Debit = 1,

    /// <summary>Liabilities, equity, revenue increase on credit.</summary>
    Credit = -1
}

/// <summary>
/// Report grouping used by the Reporting Engine to map accounts to financial statement sections
/// (spec section 17 & 29). Values align with PSAP berbasis akrual reports.
/// </summary>
public enum ReportMapping
{
    None = 0,
    BalanceSheetAsset = 1,
    BalanceSheetLiability = 2,
    BalanceSheetEquity = 3,
    OperatingStatementRevenue = 4,
    OperatingStatementExpense = 5,
    LraRevenue = 6,
    LraExpenditure = 7,
    LraFinancing = 8,
    CashFlowOperating = 9,
    CashFlowInvesting = 10,
    CashFlowFinancing = 11,
    CashFlowTransitory = 12,
    EquityChanges = 13,
    NotesToFinancialStatements = 14
}
