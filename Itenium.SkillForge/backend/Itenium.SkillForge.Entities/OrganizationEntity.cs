using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

/// <summary>
/// Represents an organization (tenant) in the system.
/// </summary>
public class OrganizationEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    public override string ToString() => Name;
}
