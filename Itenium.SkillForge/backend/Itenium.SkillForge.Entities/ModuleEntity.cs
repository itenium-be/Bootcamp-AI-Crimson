using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

public class ModuleEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Goal { get; set; }

    [MaxLength(450)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
