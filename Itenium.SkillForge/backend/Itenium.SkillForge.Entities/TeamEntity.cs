using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

/// <summary>
/// A team is a CompetenceCenter like Java or PO-Analysis.
/// </summary>
public class TeamEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    public override string ToString() => Name;
}
