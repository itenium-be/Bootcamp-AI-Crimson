using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

public class QuizEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public double PassScore { get; set; } = 60;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public IList<QuestionEntity> Questions { get; set; } = [];

    public IList<QuizAttemptEntity> Attempts { get; set; } = [];
}
