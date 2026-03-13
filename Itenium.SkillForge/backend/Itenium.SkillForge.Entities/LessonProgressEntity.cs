using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

/// <summary>
/// Tracks lesson completion per learner. Decoupled from lesson content — edits do not reset this.
/// </summary>
public class LessonProgressEntity
{
    [Required]
    [MaxLength(450)]
    public required string UserId { get; set; }

    public int LessonId { get; set; }

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
