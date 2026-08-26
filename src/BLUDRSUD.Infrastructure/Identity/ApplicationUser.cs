using BLUDRSUD.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace BLUDRSUD.Infrastructure.Identity;

/// <summary>
/// Application user extending ASP.NET Core Identity with BLUD-specific audit fields.
/// Implements IAuditable/ISoftDeletable so soft-deleted users remain for audit trail
/// (spec section 20 — audit trail must survive deletion).
/// </summary>
public class ApplicationUser : IdentityUser, IAuditable, ISoftDeletable
{
    /// <summary>Full name of the user (NIK/employee name).</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Employee number / NIP.</summary>
    public string? EmployeeNumber { get; set; }

    /// <summary>Optional link to the user's primary Organization unit.</summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>If false, user cannot login (e.g. inactive employee).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Last successful login timestamp (audit).</summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>Last login IP address (audit).</summary>
    public string? LastLoginIpAddress { get; set; }

    // --- IAuditable ---
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "system";
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

/// <summary>
/// Application role with description; permissions are modeled as claims (spec section 21 —
/// use Permission claims, not just roles). Policy-based authorization consumes these claims.
/// </summary>
public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
}
