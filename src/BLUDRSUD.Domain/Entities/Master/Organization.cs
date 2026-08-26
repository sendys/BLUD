using BLUDRSUD.Domain.Common;

namespace BLUDRSUD.Domain.Entities.Master;

/// <summary>
/// Organization unit (spec section 7): RSUD → Direksi → Bagian/Bidang → Instalasi → Unit → Ruangan.
/// Hierarchical self-reference. Cost Centers and Service Units reference an Organization.
/// </summary>
public class Organization : BaseEntity
{
    /// <summary>Short code e.g. "DIR", "RI", "IGD" — unique.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Full name e.g. "Rumah Sakit Umum Daerah".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Level in org tree (1 = RSUD root).</summary>
    public int Level { get; set; } = 1;

    /// <summary>Parent organization Id (null = root).</summary>
    public Guid? ParentId { get; set; }

    public Organization? Parent { get; set; }
    public ICollection<Organization> Children { get; set; } = new List<Organization>();

    /// <summary>Classification: Direksi / Bagian / Bidang / Instalasi / Unit / Ruangan.</summary>
    public string OrganizationType { get; set; } = "Unit";

    /// <summary>If true, available for operational use.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Optional head of the unit.</summary>
    public string? HeadName { get; set; }
}
