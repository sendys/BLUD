namespace BLUDRSUD.Infrastructure.Authorization;

/// <summary>
/// Catalog of permission strings stored as Identity role claims (spec section 21).
/// Authorization uses Policies that require these permission claims — NOT role checks alone.
/// Format: {module}.{resource}.{action} for grep-ability and consistency.
/// </summary>
public static class Permissions
{
    public const string DashboardView = "dashboard.view";

    // --- Financial / RBA (spec section 8) ---
    public const string RbaView = "financial.rba.view";
    public const string RbaCreate = "financial.rba.create";
    public const string RbaApprove = "financial.rba.approve";

    // --- Accounting / Journal (spec section 4) ---
    public const string JournalView = "accounting.journal.view";
    public const string JournalCreate = "accounting.journal.create";
    public const string JournalPost = "accounting.journal.post";
    public const string JournalReverse = "accounting.journal.reverse";

    // --- Master Data (spec section 7) ---
    public const string AccountView = "master.account.view";
    public const string AccountManage = "master.account.manage";
    public const string PeriodView = "master.period.view";
    public const string PeriodManage = "master.period.manage";

    // --- Reports (spec section 17) ---
    public const string ReportBalanceSheetView = "report.balance_sheet.view";
    public const string ReportOperatingStatementView = "report.operating_statement.view";
    public const string ReportCashFlowView = "report.cash_flow.view";
    public const string ReportLraView = "report.lra.view";

    // --- Closing (spec section 19) ---
    public const string PeriodClose = "closing.period.execute";
    public const string PeriodReopen = "closing.period.reopen";

    // --- Audit (spec section 20) ---
    public const string AuditView = "audit.trail.view";

    public static readonly string[] All =
    {
        DashboardView,
        RbaView, RbaCreate, RbaApprove,
        JournalView, JournalCreate, JournalPost, JournalReverse,
        AccountView, AccountManage, PeriodView, PeriodManage,
        ReportBalanceSheetView, ReportOperatingStatementView, ReportCashFlowView, ReportLraView,
        PeriodClose, PeriodReopen,
        AuditView
    };
}

/// <summary>
/// Role names (spec section 21). Roles are containers for permission claims — never checked directly
/// for authorization, only used as claim groups.
/// </summary>
public static class Roles
{
    public const string SuperAdmin = "Super Admin";
    public const string Direktur = "Direktur";
    public const string PejabatKeuangan = "Pejabat Keuangan";
    public const string BendaharaPenerimaan = "Bendahara Penerimaan";
    public const string BendaharaPengeluaran = "Bendahara Pengeluaran";
    public const string Akuntansi = "Akuntansi";
    public const string Verifikator = "Verifikator";
    public const string Pengadaan = "Pengadaan";
    public const string Gudang = "Gudang";
    public const string Aset = "Aset";
    public const string Auditor = "Auditor";
    public const string Manajemen = "Manajemen";

    public static readonly string[] All =
    {
        SuperAdmin, Direktur, PejabatKeuangan, BendaharaPenerimaan, BendaharaPengeluaran,
        Akuntansi, Verifikator, Pengadaan, Gudang, Aset, Auditor, Manajemen
    };
}
