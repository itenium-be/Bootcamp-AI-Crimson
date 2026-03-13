using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

public class QuestionResponseEntity
{
    [Key]
    public int Id { get; set; }

    public int AttemptId { get; set; }

    public QuizAttemptEntity Attempt { get; set; } = null!;

    public int QuestionId { get; set; }

    public QuestionEntity Question { get; set; } = null!;

    public bool IsCorrect { get; set; }
}
