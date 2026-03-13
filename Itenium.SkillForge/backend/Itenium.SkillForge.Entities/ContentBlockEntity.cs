using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

public class ContentBlockEntity
{
    [Key]
    public int Id { get; set; }

    public int LessonId { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Type { get; set; }

    [Required]
    public required string Content { get; set; }

    public int Order { get; set; }
}
