using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

public class QuizAttemptEntity
{
    [Key]
    public int Id { get; set; }

    public int QuizId { get; set; }

    public QuizEntity Quiz { get; set; } = null!;

    [Required]
    [MaxLength(450)]
    public required string UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public required string UserName { get; set; }

    public int? TeamId { get; set; }

    public double Score { get; set; }

    public bool IsPassed { get; set; }

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    public IList<QuestionResponseEntity> Responses { get; set; } = [];
}
