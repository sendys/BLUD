using BLUDRSUD.Domain.Common;
using BLUDRSUD.Domain.Entities.Master;
using BLUDRSUD.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BLUDRSUD.Infrastructure.Persistence;

/// <summary>
/// Application DbContext using ASP.NET Core Identity + EF Core 9 + Pomelo MySQL.
/// Configuration details (indexes, FKs, unique constraints, column types) live in
/// IEntityTypeConfiguration&lt;T&gt; classes under /Configurations — NOT here (spec section 35).
/// Global soft-delete query filters are applied here for ISoftDeletable entities (spec section 24).
/// </summary>
public class BludRsudDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public BludRsudDbContext(DbContextOptions<BludRsudDbContext> options) : base(options) { }

    // --- Phase 1 Master Data DbSets ---
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<AccountingPeriod> AccountingPeriods => Set<AccountingPeriod>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<FundSource> FundSources => Set<FundSource>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all IEntityTypeConfiguration<T> from this assembly.
        // Per-entity soft-delete query filters live in each configuration class
        // (type-safe, explicit) rather than a reflection loop (spec section 24).
        builder.ApplyConfigurationsFromAssembly(typeof(BludRsudDbContext).Assembly);
    }

    /// <summary>
    /// Override SaveChangesAsync to auto-stamp CreatedAt/UpdatedAt audit fields
    /// before persistence. CreatedBy/UpdatedBy set by caller via ambient context (later phases).
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        StampAuditFields();
        return base.SaveChanges();
    }

    private void StampAuditFields()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    if (string.IsNullOrEmpty(entry.Entity.CreatedBy))
                        entry.Entity.CreatedBy = "system";
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }
    }
}
