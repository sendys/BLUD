using BLUDRSUD.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BLUDRSUD.Infrastructure.Persistence.Configurations;

/// <summary>
/// Renames Identity tables to BLUD conventions and adds audit columns for ApplicationUser.
/// </summary>
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> b)
    {
        b.ToTable("users");
        b.HasQueryFilter(u => u.DeletedAt == null);
        b.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        b.Property(u => u.EmployeeNumber).HasMaxLength(50);
        b.Property(u => u.LastLoginIpAddress).HasMaxLength(50);
        b.Property(u => u.CreatedBy).HasMaxLength(100).IsRequired();
        b.Property(u => u.UpdatedBy).HasMaxLength(100);
        b.Property(u => u.DeletedBy).HasMaxLength(100);
        b.Property(u => u.CreatedAt).HasColumnType("datetime(6)");
        b.Property(u => u.UpdatedAt).HasColumnType("datetime(6)");
        b.Property(u => u.DeletedAt).HasColumnType("datetime(6)");
        b.HasIndex(u => u.EmployeeNumber);
    }
}

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> b)
    {
        b.ToTable("roles");
        b.Property(r => r.Description).HasMaxLength(255);
    }
}
