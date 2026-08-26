using BLUDRSUD.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BLUDRSUD.Web.Pages.Dashboard;

/// <summary>
/// Dashboard landing page (spec section 22). Phase 1 shows master-data health (accounts, periods,
/// orgs, cost centers, fund sources). Financial cards (Pendapatan, Beban, Surplus/Defisit, Kas, Piutang,
/// Hutang, Persediaan, Aset) are placeholders populated from the General Ledger starting in Phase 2.
/// </summary>
[Authorize(Policy = "dashboard:view")]
public class IndexModel : PageModel
{
    private readonly BludRsudDbContext _db;
    public IndexModel(BludRsudDbContext db) => _db = db;

    public int AccountCount { get; set; }
    public int ActiveAccounts { get; set; }
    public int PeriodCount { get; set; }
    public int OpenPeriods { get; set; }
    public int OrganizationCount { get; set; }
    public int CostCenterCount { get; set; }
    public int FundSourceCount { get; set; }

    public string CurrentPeriod { get; set; } = "-";
    public string FiscalYear { get; set; } = "-";

    public async Task OnGetAsync()
    {
        AccountCount = await _db.Accounts.CountAsync();
        ActiveAccounts = await _db.Accounts.CountAsync(a => a.IsActive);
        PeriodCount = await _db.AccountingPeriods.CountAsync();
        OpenPeriods = await _db.AccountingPeriods.CountAsync(p => p.Status == Domain.Enums.PeriodStatus.Open);
        OrganizationCount = await _db.Organizations.CountAsync();
        CostCenterCount = await _db.CostCenters.CountAsync();
        FundSourceCount = await _db.FundSources.CountAsync();

        var current = await _db.AccountingPeriods
            .Where(p => p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow)
            .FirstOrDefaultAsync();
        if (current != null)
        {
            CurrentPeriod = current.Name;
            FiscalYear = current.FiscalYear.ToString();
        }
    }
}
