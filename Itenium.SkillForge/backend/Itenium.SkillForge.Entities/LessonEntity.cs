using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

public class LessonEntity
{
    [Key]
    public int Id { get; set; }

    public int CourseId { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    public int SortOrder { get; set; }
}
