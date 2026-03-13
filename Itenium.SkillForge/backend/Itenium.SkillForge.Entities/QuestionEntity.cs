using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

public class QuestionEntity
{
    [Key]
    public int Id { get; set; }

    public int QuizId { get; set; }

    public QuizEntity Quiz { get; set; } = null!;

    [Required]
    [MaxLength(1000)]
    public required string Text { get; set; }

    public int Order { get; set; }

    public IList<QuestionResponseEntity> Responses { get; set; } = [];
}
