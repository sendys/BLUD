using BLUDRSUD.Domain.Entities.Master;
using BLUDRSUD.Domain.Enums;
using BLUDRSUD.Infrastructure.Authorization;
using BLUDRSUD.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLUDRSUD.Infrastructure.Persistence;

/// <summary>
/// Database seeder (spec section 32). Seeds:
/// - Admin user + all Roles + permission claims
/// - Chart of Accounts (SAP berbasis akrual structure; configurable per Pemda)
/// - RSUD organization tree + Cost Centers + Fund Sources (demo data)
/// - 2026 monthly accounting periods (2026-01 .. 2026-12) + year-end closing period
///
/// DISCLAIMER (spec section 3 & 41): Chart of accounts structure follows Permendagri 79/2018 & PP 71/2010
/// generically. Account codes MUST be validated by a BLUD finance officer and regional accounting policy
/// before use in official reports — regional chart of accounts may differ.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(
        BludRsudDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILoggerFactory loggerFactory,
        CancellationToken ct = default)
    {
        var log = loggerFactory.CreateLogger(nameof(DbSeeder));

        await SeedRolesAsync(roleManager, log, ct);
        await SeedAdminUserAsync(userManager, log, ct);
        await SeedChartOfAccountsAsync(db, log, ct);
        await SeedOrganizationsAsync(db, log, ct);
        await SeedCostCentersAsync(db, log, ct);
        await SeedFundSourcesAsync(db, log, ct);
        await SeedAccountingPeriodsAsync(db, log, ct);

        await db.SaveChangesAsync(ct);
        log.LogInformation("Seeding complete.");
    }

    // ---------------------------------------------------------------------------
    // ROLES + PERMISSION CLAIMS
    // ---------------------------------------------------------------------------
    private static async Task SeedRolesAsync(
        RoleManager<ApplicationRole> roleManager, ILogger log, CancellationToken ct)
    {
        foreach (var roleName in Roles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName)) continue;
            var role = new ApplicationRole { Name = roleName, Description = $"Role {roleName}" };
            var r = await roleManager.CreateAsync(role);
            if (!r.Succeeded)
            {
                log.LogError("Failed to create role {Role}: {Errors}", roleName,
                    string.Join("; ", r.Errors.Select(e => e.Description)));
                continue;
            }

            // Super Admin gets ALL permissions; others get a subset appropriate to their role.
            var perms = roleName == Roles.SuperAdmin
                ? Permissions.All
                : GetRolePermissions(roleName);

            foreach (var p in perms)
            {
                await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("permission", p));
            }
        }
    }

    /// <summary>
    /// Permission matrix per non-admin role (spec section 21). Intentionally conservative;
    /// production deployments should refine per RSUD policy.
    /// </summary>
    private static string[] GetRolePermissions(string role) => role switch
    {
        Roles.Direktur => new[] { Permissions.DashboardView, Permissions.RbaView, Permissions.RbaApprove,
            Permissions.ReportBalanceSheetView, Permissions.ReportOperatingStatementView,
            Permissions.ReportCashFlowView, Permissions.ReportLraView },
        Roles.PejabatKeuangan => new[] { Permissions.DashboardView, Permissions.RbaView, Permissions.RbaCreate,
            Permissions.RbaApprove, Permissions.PeriodClose, Permissions.ReportLraView },
        Roles.BendaharaPenerimaan => new[] { Permissions.DashboardView, Permissions.JournalView },
        Roles.BendaharaPengeluaran => new[] { Permissions.DashboardView, Permissions.JournalView },
        Roles.Akuntansi => new[] { Permissions.DashboardView, Permissions.JournalView, Permissions.JournalCreate,
            Permissions.JournalPost, Permissions.JournalReverse, Permissions.AccountView, Permissions.PeriodView,
            Permissions.ReportBalanceSheetView, Permissions.ReportOperatingStatementView },
        Roles.Verifikator => new[] { Permissions.DashboardView, Permissions.RbaView },
        Roles.Pengadaan => new[] { Permissions.DashboardView },
        Roles.Gudang => new[] { Permissions.DashboardView },
        Roles.Aset => new[] { Permissions.DashboardView },
        Roles.Auditor => new[] { Permissions.DashboardView, Permissions.AuditView, Permissions.JournalView,
            Permissions.ReportBalanceSheetView, Permissions.ReportOperatingStatementView, Permissions.ReportLraView },
        Roles.Manajemen => new[] { Permissions.DashboardView, Permissions.RbaView,
            Permissions.ReportBalanceSheetView, Permissions.ReportOperatingStatementView, Permissions.ReportLraView },
        _ => Array.Empty<string>()
    };

    // ---------------------------------------------------------------------------
    // ADMIN USER
    // ---------------------------------------------------------------------------
    private static async Task SeedAdminUserAsync(
        UserManager<ApplicationUser> userManager, ILogger log, CancellationToken ct)
    {
        const string adminEmail = "admin@bludrsud.local";
        var existing = await userManager.FindByEmailAsync(adminEmail);
        if (existing != null) return;

        var admin = new ApplicationUser
        {
            UserName = "admin",
            Email = adminEmail,
            EmailConfirmed = true,
            FullName = "System Administrator",
            IsActive = true,
            CreatedBy = "system"
        };

        var r = await userManager.CreateAsync(admin, "Blud@2026!");
        if (!r.Succeeded)
        {
            log.LogError("Failed to create admin user: {Errors}",
                string.Join("; ", r.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(admin, Roles.SuperAdmin);
        foreach (var p in Permissions.All)
            await userManager.AddClaimAsync(admin, new System.Security.Claims.Claim("permission", p));

        log.LogInformation("Admin user created. Username='admin', Password='Blud@2026!' — CHANGE IMMEDIATELY.");
    }

    // ---------------------------------------------------------------------------
    // CHART OF ACCOUNTS (spec section 5) — configurable, SAP berbasis akrual
    // ---------------------------------------------------------------------------
    private static async Task SeedChartOfAccountsAsync(BludRsudDbContext db, ILogger log, CancellationToken ct)
    {
        if (await db.Accounts.AnyAsync(ct)) return;

        var now = DateTime.UtcNow;
        var byCode = new Dictionary<string, Account>();

        Account Make(string code, string name, AccountType type, NormalBalance bal, ReportMapping rpt,
            bool header, int level, string? parentCode = null, bool allowPosting = true)
        {
            var a = new Account
            {
                Code = code,
                Name = name,
                AccountType = type,
                NormalBalance = bal,
                ReportMapping = rpt,
                IsHeader = header,
                AllowPosting = allowPosting && !header,
                Level = level,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = "system",
                RowVersion = new byte[8]
            };
            if (parentCode != null && byCode.TryGetValue(parentCode, out var parent))
            {
                a.ParentId = parent.Id;
            }
            db.Accounts.Add(a);
            byCode[code] = a;
            return a;
        }

        // 1 — ASET
        Make("1", "ASET", AccountType.Asset, NormalBalance.Debit, ReportMapping.BalanceSheetAsset, true, 1);
        Make("11", "ASET LANCAR", AccountType.Asset, NormalBalance.Debit, ReportMapping.BalanceSheetAsset, true, 2, "1");
        Make("1101", "KAS", AccountType.Asset, NormalBalance.Debit, ReportMapping.BalanceSheetAsset, true, 3, "11");
        Make("110101", "Kas Bendahara Penerimaan", AccountType.Asset, NormalBalance.Debit, ReportMapping.BalanceSheetAsset, false, 4, "1101");
        Make("110102", "Kas Bendahara Pengeluaran", AccountType.Asset, NormalBalance.Debit, ReportMapping.BalanceSheetAsset, false, 4, "1101");
        Make("1102", "BANK", AccountType.Asset, NormalBalance.Debit, ReportMapping.BalanceSheetAsset, true, 3, "11");
        Make("110201", "Bank Operasional", AccountType.Asset, NormalBalance.Debit, ReportMapping.BalanceSheetAsset, false, 4, "1102");
        Make("110202", "Bank BLUD", AccountType.Asset, NormalBalance.Debit, ReportMapping.BalanceSheetAsset, false, 4, "1102");
        Make("12", "PIUTANG", AccountType.Asset, NormalBalance.Debit, ReportMapping.BalanceSheetAsset, true, 2, "1");
        Make("1201", "Piutang Pelayanan", AccountType.Asset, NormalBalance.Debit, ReportMapping.BalanceSheetAsset, false, 3, "12");
        Make("1202", "Piutang BPJS", AccountType.Asset, NormalBalance.Debit, ReportMapping.BalanceSheetAsset, false, 3, "12");
        Make("13", "PERSEDIAAN", AccountType.Asset, NormalBalance.Debit, ReportMapping.BalanceSheetAsset, true, 2, "1");
        Make("1301", "Persediaan Obat", AccountType.Asset, NormalBalance.Debit, ReportMapping.BalanceSheetAsset, false, 3, "13");
        Make("1302", "Persediaan BHP", AccountType.Asset, NormalBalance.Debit, ReportMapping.BalanceSheetAsset, false, 3, "13");
        Make("14", "ASET TETAP", AccountType.Asset, NormalBalance.Debit, ReportMapping.BalanceSheetAsset, true, 2, "1");
        Make("1401", "Tanah", AccountType.Asset, NormalBalance.Debit, ReportMapping.BalanceSheetAsset, false, 3, "14");
        Make("1402", "Gedung dan Bangunan", AccountType.Asset, NormalBalance.Debit, ReportMapping.BalanceSheetAsset, false, 3, "14");
        Make("1403", "Peralatan Medis", AccountType.Asset, NormalBalance.Debit, ReportMapping.BalanceSheetAsset, false, 3, "14");
        Make("1404", "Akumulasi Penyusutan Aset Tetap", AccountType.Asset, NormalBalance.Credit, ReportMapping.BalanceSheetAsset, false, 3, "14", allowPosting: true);

        // 2 — KEWAJIBAN
        Make("2", "KEWAJIBAN", AccountType.Liability, NormalBalance.Credit, ReportMapping.BalanceSheetLiability, true, 1);
        Make("21", "Utang Jangka Pendek", AccountType.Liability, NormalBalance.Credit, ReportMapping.BalanceSheetLiability, true, 2, "2");
        Make("2101", "Utang Vendor", AccountType.Liability, NormalBalance.Credit, ReportMapping.BalanceSheetLiability, false, 3, "21");
        Make("2102", "Utang Pajak", AccountType.Liability, NormalBalance.Credit, ReportMapping.BalanceSheetLiability, false, 3, "21");
        Make("2103", "Utang Pegawai", AccountType.Liability, NormalBalance.Credit, ReportMapping.BalanceSheetLiability, false, 3, "21");

        // 3 — EKUITAS
        Make("3", "EKUITAS", AccountType.Equity, NormalBalance.Credit, ReportMapping.BalanceSheetEquity, true, 1);
        Make("31", "Ekuitas BLUD", AccountType.Equity, NormalBalance.Credit, ReportMapping.BalanceSheetEquity, false, 2, "3");
        Make("32", "Surplus/Defisit Tahun Berjalan", AccountType.Equity, NormalBalance.Credit, ReportMapping.BalanceSheetEquity, false, 2, "3");

        // 4 — PENDAPATAN (akrual, LO)
        Make("4", "PENDAPATAN", AccountType.Revenue, NormalBalance.Credit, ReportMapping.OperatingStatementRevenue, true, 1);
        Make("41", "Pendapatan Pelayanan", AccountType.Revenue, NormalBalance.Credit, ReportMapping.OperatingStatementRevenue, true, 2, "4");
        Make("4101", "Pendapatan Rawat Jalan", AccountType.Revenue, NormalBalance.Credit, ReportMapping.OperatingStatementRevenue, false, 3, "41");
        Make("4102", "Pendapatan Rawat Inap", AccountType.Revenue, NormalBalance.Credit, ReportMapping.OperatingStatementRevenue, false, 3, "41");
        Make("4103", "Pendapatan IGD", AccountType.Revenue, NormalBalance.Credit, ReportMapping.OperatingStatementRevenue, false, 3, "41");
        Make("4104", "Pendapatan Laboratorium", AccountType.Revenue, NormalBalance.Credit, ReportMapping.OperatingStatementRevenue, false, 3, "41");
        Make("4105", "Pendapatan Radiologi", AccountType.Revenue, NormalBalance.Credit, ReportMapping.OperatingStatementRevenue, false, 3, "41");
        Make("4106", "Pendapatan Farmasi", AccountType.Revenue, NormalBalance.Credit, ReportMapping.OperatingStatementRevenue, false, 3, "41");
        Make("42", "Pendapatan Lain-Lain", AccountType.Revenue, NormalBalance.Credit, ReportMapping.OperatingStatementRevenue, true, 2, "4");

        // 5 — BEBAN (akrual, LO)
        Make("5", "BEBAN", AccountType.Expense, NormalBalance.Debit, ReportMapping.OperatingStatementExpense, true, 1);
        Make("51", "Beban Pegawai", AccountType.Expense, NormalBalance.Debit, ReportMapping.OperatingStatementExpense, true, 2, "5");
        Make("5101", "Beban Gaji", AccountType.Expense, NormalBalance.Debit, ReportMapping.OperatingStatementExpense, false, 3, "51");
        Make("5102", "Beban Honor", AccountType.Expense, NormalBalance.Debit, ReportMapping.OperatingStatementExpense, false, 3, "51");
        Make("52", "Beban Operasional", AccountType.Expense, NormalBalance.Debit, ReportMapping.OperatingStatementExpense, true, 2, "5");
        Make("5201", "Beban Persediaan", AccountType.Expense, NormalBalance.Debit, ReportMapping.OperatingStatementExpense, false, 3, "52");
        Make("5202", "Beban Penyusutan", AccountType.Expense, NormalBalance.Debit, ReportMapping.OperatingStatementExpense, false, 3, "52");
        Make("5203", "Beban Pemeliharaan", AccountType.Expense, NormalBalance.Debit, ReportMapping.OperatingStatementExpense, false, 3, "52");

        // 6 — BELANJA (LRA)
        Make("6", "BELANJA", AccountType.Expenditure, NormalBalance.Debit, ReportMapping.LraExpenditure, true, 1);
        Make("61", "Belanja Pegawai", AccountType.Expenditure, NormalBalance.Debit, ReportMapping.LraExpenditure, true, 2, "6");
        Make("62", "Belanja Barang dan Jasa", AccountType.Expenditure, NormalBalance.Debit, ReportMapping.LraExpenditure, true, 2, "6");
        Make("63", "Belanja Modal", AccountType.Expenditure, NormalBalance.Debit, ReportMapping.LraExpenditure, true, 2, "6");

        // 7 — PEMBIAYAAN (LRA)
        Make("7", "PEMBIAYAAN", AccountType.Financing, NormalBalance.Debit, ReportMapping.LraFinancing, true, 1);

        log.LogInformation("Seeded {Count} chart-of-accounts entries.", db.Accounts.Local.Count);
    }

    // ---------------------------------------------------------------------------
    // ORGANIZATION + COST CENTERS + FUND SOURCES (spec section 7, demo data)
    // ---------------------------------------------------------------------------
    private static async Task SeedOrganizationsAsync(BludRsudDbContext db, ILogger log, CancellationToken ct)
    {
        if (await db.Organizations.AnyAsync(ct)) return;

        var rsud = new Organization { Code = "RSUD", Name = "RSUD Daerah", Level = 1, OrganizationType = "RSUD", IsActive = true, CreatedBy = "system" };
        db.Organizations.Add(rsud);

        Organization Unit(string code, string name, string type, Guid parent, int level) =>
            new() { Code = code, Name = name, Level = level, OrganizationType = type, ParentId = parent, IsActive = true, CreatedBy = "system" };

        db.Organizations.AddRange(
            Unit("DIREKSI", "Direksi", "Direksi", rsud.Id, 2),
            Unit("KEU", "Bagian Keuangan", "Bagian", rsud.Id, 2),
            Unit("RJ", "Rawat Jalan", "Instalasi", rsud.Id, 2),
            Unit("RI", "Rawat Inap", "Instalasi", rsud.Id, 2),
            Unit("IGD", "IGD", "Instalasi", rsud.Id, 2),
            Unit("FARMASI", "Instalasi Farmasi", "Instalasi", rsud.Id, 2),
            Unit("LAB", "Laboratorium", "Instalasi", rsud.Id, 2),
            Unit("RAD", "Radiologi", "Instalasi", rsud.Id, 2),
            Unit("PENUNJANG", "Unit Penunjang", "Unit", rsud.Id, 2)
        );

        log.LogInformation("Seeded organization tree.");
    }

    private static async Task SeedCostCentersAsync(BludRsudDbContext db, ILogger log, CancellationToken ct)
    {
        if (await db.CostCenters.AnyAsync(ct)) return;

        var orgs = await db.Organizations.ToDictionaryAsync(o => o.Code, ct);

        Guid? OrgId(string? code) => code != null && orgs.TryGetValue(code, out var o) ? o.Id : null;

        db.CostCenters.AddRange(
            new CostCenter { Code = "RAWAT_JALAN", Name = "Rawat Jalan", CostCenterType = "Operational", OrganizationId = OrgId("RJ"), IsActive = true, CreatedBy = "system" },
            new CostCenter { Code = "RAWAT_INAP", Name = "Rawat Inap", CostCenterType = "Operational", OrganizationId = OrgId("RI"), IsActive = true, CreatedBy = "system" },
            new CostCenter { Code = "IGD", Name = "IGD", CostCenterType = "Operational", OrganizationId = OrgId("IGD"), IsActive = true, CreatedBy = "system" },
            new CostCenter { Code = "FARMASI", Name = "Farmasi", CostCenterType = "Operational", OrganizationId = OrgId("FARMASI"), IsActive = true, CreatedBy = "system" },
            new CostCenter { Code = "LAB", Name = "Laboratorium", CostCenterType = "Operational", OrganizationId = OrgId("LAB"), IsActive = true, CreatedBy = "system" },
            new CostCenter { Code = "RADIOLOGI", Name = "Radiologi", CostCenterType = "Operational", OrganizationId = OrgId("RAD"), IsActive = true, CreatedBy = "system" },
            new CostCenter { Code = "ADM", Name = "Administrasi", CostCenterType = "Administrative", OrganizationId = OrgId("KEU"), IsActive = true, CreatedBy = "system" }
        );

        log.LogInformation("Seeded cost centers.");
    }

    private static async Task SeedFundSourcesAsync(BludRsudDbContext db, ILogger log, CancellationToken ct)
    {
        if (await db.FundSources.AnyAsync(ct)) return;

        db.FundSources.AddRange(
            new FundSource { Code = "BLUD", Name = "Dana BLUD", FundCategory = "BLUD", IsActive = true, CreatedBy = "system" },
            new FundSource { Code = "APBD", Name = "Dana APBD", FundCategory = "APBD", IsActive = true, CreatedBy = "system" },
            new FundSource { Code = "APBN", Name = "Dana APBN", FundCategory = "APBN", IsActive = true, CreatedBy = "system" },
            new FundSource { Code = "HIBAH", Name = "Hibah", FundCategory = "Hibah", IsActive = true, CreatedBy = "system" },
            new FundSource { Code = "PENDAPATAN_SENDIRI", Name = "Pendapatan Sendiri", FundCategory = "BLUD", IsActive = true, CreatedBy = "system" }
        );

        log.LogInformation("Seeded fund sources.");
    }

    // ---------------------------------------------------------------------------
    // ACCOUNTING PERIODS 2026-01 .. 2026-12 + year-end (spec section 19)
    // ---------------------------------------------------------------------------
    private static async Task SeedAccountingPeriodsAsync(BludRsudDbContext db, ILogger log, CancellationToken ct)
    {
        if (await db.AccountingPeriods.AnyAsync(ct)) return;

        var culture = new System.Globalization.CultureInfo("id-ID");
        var months = culture.DateTimeFormat.MonthNames;
        const int year = 2026;

        for (int m = 1; m <= 12; m++)
        {
            var start = new DateTime(year, m, 1);
            var end = start.AddMonths(1).AddDays(-1);
            db.AccountingPeriods.Add(new AccountingPeriod
            {
                Code = $"{year}-{m:D2}",
                FiscalYear = year,
                PeriodNumber = m,
                Name = $"{months[m - 1]} {year}".ToUpperInvariant(),
                StartDate = start,
                EndDate = end,
                Status = m == DateTime.UtcNow.Month ? PeriodStatus.Open : PeriodStatus.Open,
                PeriodType = PeriodType.Monthly,
                IsCurrent = false,
                CreatedBy = "system"
            });
        }

        // Year-end closing period (period 13)
        db.AccountingPeriods.Add(new AccountingPeriod
        {
            Code = $"{year}-13",
            FiscalYear = year,
            PeriodNumber = 13,
            Name = $"PENUTUPAN TAHUN {year}",
            StartDate = new DateTime(year, 12, 31),
            EndDate = new DateTime(year, 12, 31, 23, 59, 59),
            Status = PeriodStatus.Open,
            PeriodType = PeriodType.YearEndClosing,
            IsCurrent = false,
            CreatedBy = "system"
        });

        log.LogInformation("Seeded {Year} accounting periods.", year);
    }
}
