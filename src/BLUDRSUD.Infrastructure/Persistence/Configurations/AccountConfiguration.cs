using BLUDRSUD.Domain.Entities.Master;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BLUDRSUD.Infrastructure.Persistence.Configurations;

/// <summary>
/// Chart of Accounts configuration (spec section 5 & 24).
/// - Hierarchical self-reference via ParentId
/// - Unique Code (prevents duplicate account codes)
/// - Composite index (ParentId, Code) for tree traversal
/// - Optimistic concurrency RowVersion
/// - Soft-delete filter inherited from DbContext
/// </summary>
public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> b)
    {
        b.ToTable("accounts");
        b.HasKey(a => a.Id);

        // Global soft-delete filter (spec section 24).
        b.HasQueryFilter(a => a.DeletedAt == null);

        b.Property(a => a.Code).HasMaxLength(50).IsRequired();
        b.Property(a => a.Name).HasMaxLength(255).IsRequired();
        b.Property(a => a.AccountType).HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(a => a.NormalBalance).HasConversion<string>().HasMaxLength(10).IsRequired();
        b.Property(a => a.ReportMapping).HasConversion<string>().HasMaxLength(40);
        b.Property(a => a.ExternalCode).HasMaxLength(50);
        b.Property(a => a.CreatedBy).HasMaxLength(100).IsRequired();
        b.Property(a => a.UpdatedBy).HasMaxLength(100);
        b.Property(a => a.DeletedBy).HasMaxLength(100);
        b.Property(a => a.CreatedAt).HasColumnType("datetime(6)");
        b.Property(a => a.UpdatedAt).HasColumnType("datetime(6)");
        b.Property(a => a.DeletedAt).HasColumnType("datetime(6)");
        b.Property(a => a.RowVersion)
            .HasColumnType("longblob")
            .IsConcurrencyToken()
            .ValueGeneratedNever();

        // Self-referential hierarchy
        b.HasOne(a => a.Parent)
         .WithMany(p => p.Children)
         .HasForeignKey(a => a.ParentId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(a => a.Code).IsUnique();
        b.HasIndex(a => new { a.ParentId, a.Code });
        b.HasIndex(a => a.AccountType);
        b.HasIndex(a => a.IsActive);
    }
}
