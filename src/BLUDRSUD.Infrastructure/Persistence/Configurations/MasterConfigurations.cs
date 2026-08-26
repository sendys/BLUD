using BLUDRSUD.Domain.Entities.Master;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BLUDRSUD.Infrastructure.Persistence.Configurations;

/// <summary>
/// Accounting Period (spec section 19).
/// - Unique Code ("2026-01")
/// - Unique (FiscalYear, PeriodNumber)
/// - Optimistic concurrency RowVersion
/// </summary>
public class AccountingPeriodConfiguration : IEntityTypeConfiguration<AccountingPeriod>
{
    public void Configure(EntityTypeBuilder<AccountingPeriod> b)
    {
        b.ToTable("accounting_periods");
        b.HasKey(p => p.Id);

        b.HasQueryFilter(p => p.DeletedAt == null);

        b.Property(p => p.Code).HasMaxLength(20).IsRequired();
        b.Property(p => p.Name).HasMaxLength(100).IsRequired();
        b.Property(p => p.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(p => p.PeriodType).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(p => p.ClosedBy).HasMaxLength(100);
        b.Property(p => p.StartDate).HasColumnType("datetime(6)");
        b.Property(p => p.EndDate).HasColumnType("datetime(6)");
        b.Property(p => p.ClosedAt).HasColumnType("datetime(6)");
        b.Property(p => p.CreatedAt).HasColumnType("datetime(6)");
        b.Property(p => p.CreatedBy).HasMaxLength(100).IsRequired();
        b.Property(p => p.UpdatedAt).HasColumnType("datetime(6)");
        b.Property(p => p.UpdatedBy).HasMaxLength(100);
        b.Property(p => p.DeletedAt).HasColumnType("datetime(6)");
        b.Property(p => p.DeletedBy).HasMaxLength(100);
        b.Property(p => p.RowVersion)
            .HasColumnType("longblob")
            .IsConcurrencyToken()
            .ValueGeneratedNever();

        b.HasIndex(p => p.Code).IsUnique();
        b.HasIndex(p => new { p.FiscalYear, p.PeriodNumber }).IsUnique();
        b.HasIndex(p => p.Status);
        b.HasIndex(p => p.FiscalYear);
    }
}

/// <summary>
/// Organization (spec section 7) — hierarchical self-reference.
/// </summary>
public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> b)
    {
        b.ToTable("organizations");
        b.HasKey(o => o.Id);

        b.HasQueryFilter(o => o.DeletedAt == null);

        b.Property(o => o.Code).HasMaxLength(50).IsRequired();
        b.Property(o => o.Name).HasMaxLength(255).IsRequired();
        b.Property(o => o.OrganizationType).HasMaxLength(50).IsRequired();
        b.Property(o => o.HeadName).HasMaxLength(200);
        b.Property(o => o.CreatedBy).HasMaxLength(100).IsRequired();
        b.Property(o => o.UpdatedBy).HasMaxLength(100);
        b.Property(o => o.DeletedBy).HasMaxLength(100);
        b.Property(o => o.CreatedAt).HasColumnType("datetime(6)");
        b.Property(o => o.UpdatedAt).HasColumnType("datetime(6)");
        b.Property(o => o.DeletedAt).HasColumnType("datetime(6)");

        b.HasOne(o => o.Parent)
         .WithMany(p => p.Children)
         .HasForeignKey(o => o.ParentId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(o => o.Code).IsUnique();
        b.HasIndex(o => o.ParentId);
        b.HasIndex(o => o.IsActive);
    }
}

/// <summary>
/// Cost Center (spec section 6 & 7) — accounting dimension on journal lines.
/// </summary>
public class CostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> b)
    {
        b.ToTable("cost_centers");
        b.HasKey(c => c.Id);

        b.HasQueryFilter(c => c.DeletedAt == null);

        b.Property(c => c.Code).HasMaxLength(50).IsRequired();
        b.Property(c => c.Name).HasMaxLength(255).IsRequired();
        b.Property(c => c.Description).HasMaxLength(500);
        b.Property(c => c.CostCenterType).HasMaxLength(50).IsRequired();
        b.Property(c => c.CreatedBy).HasMaxLength(100).IsRequired();
        b.Property(c => c.UpdatedBy).HasMaxLength(100);
        b.Property(c => c.DeletedBy).HasMaxLength(100);
        b.Property(c => c.CreatedAt).HasColumnType("datetime(6)");
        b.Property(c => c.UpdatedAt).HasColumnType("datetime(6)");
        b.Property(c => c.DeletedAt).HasColumnType("datetime(6)");

        b.HasOne(c => c.Parent)
         .WithMany(p => p.Children)
         .HasForeignKey(c => c.ParentId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(c => c.Organization)
         .WithMany()
         .HasForeignKey(c => c.OrganizationId)
         .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(c => c.Code).IsUnique();
        b.HasIndex(c => c.ParentId);
        b.HasIndex(c => c.OrganizationId);
        b.HasIndex(c => c.IsActive);
    }
}

/// <summary>
/// Fund Source (spec section 6) — accounting dimension on journal lines.
/// </summary>
public class FundSourceConfiguration : IEntityTypeConfiguration<FundSource>
{
    public void Configure(EntityTypeBuilder<FundSource> b)
    {
        b.ToTable("fund_sources");
        b.HasKey(f => f.Id);

        b.HasQueryFilter(f => f.DeletedAt == null);

        b.Property(f => f.Code).HasMaxLength(50).IsRequired();
        b.Property(f => f.Name).HasMaxLength(255).IsRequired();
        b.Property(f => f.Description).HasMaxLength(500);
        b.Property(f => f.FundCategory).HasMaxLength(50).IsRequired();
        b.Property(f => f.CreatedBy).HasMaxLength(100).IsRequired();
        b.Property(f => f.UpdatedBy).HasMaxLength(100);
        b.Property(f => f.DeletedBy).HasMaxLength(100);
        b.Property(f => f.CreatedAt).HasColumnType("datetime(6)");
        b.Property(f => f.UpdatedAt).HasColumnType("datetime(6)");
        b.Property(f => f.DeletedAt).HasColumnType("datetime(6)");

        b.HasOne(f => f.Parent)
         .WithMany(p => p.Children)
         .HasForeignKey(f => f.ParentId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(f => f.Code).IsUnique();
        b.HasIndex(f => f.IsActive);
        b.HasIndex(f => f.FundCategory);
    }
}
