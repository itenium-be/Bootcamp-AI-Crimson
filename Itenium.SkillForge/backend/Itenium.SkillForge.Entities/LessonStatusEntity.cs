using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

public class LessonStatusEntity
{
    [Required]
    [MaxLength(100)]
    public required string UserId { get; set; }

    public int LessonId { get; set; }

    public LessonStatusValue Status { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
