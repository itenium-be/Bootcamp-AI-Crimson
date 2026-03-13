using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

public class LessonAnnotationEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public required string UserId { get; set; }

    /// <summary>
    /// Pseudonym/first name shown to other learners.
    /// </summary>
    [MaxLength(100)]
    public string? DisplayName { get; set; }

    public int LessonId { get; set; }

    [Required]
    [MaxLength(4000)]
    public required string Content { get; set; }

    /// <summary>
    /// Optional content rating 1–5.
    /// </summary>
    public int? Rating { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
