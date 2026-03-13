using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

public class CourseFeedbackEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public required string UserId { get; set; }

    public int CourseId { get; set; }

    public int? LessonId { get; set; }

    /// <summary>
    /// Rating 1–5.
    /// </summary>
    public int Rating { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsFlagged { get; set; }

    public bool IsDismissed { get; set; }
}
