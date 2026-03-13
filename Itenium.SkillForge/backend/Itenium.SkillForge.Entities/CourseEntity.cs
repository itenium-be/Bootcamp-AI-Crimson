using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

/// <summary>
/// Course master data managed by central management.
/// </summary>
public class CourseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(50)]
    public string? Level { get; set; }

    public int? EstimatedDuration { get; set; }

    public CourseStatus Status { get; set; } = CourseStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? ModuleId { get; set; }

    public int ModuleOrder { get; set; }

    public override string ToString() => $"{Name} ({Category})";
}
