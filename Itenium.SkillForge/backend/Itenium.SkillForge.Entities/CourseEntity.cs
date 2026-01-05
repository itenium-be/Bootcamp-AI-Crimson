using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Itenium.SkillForge.Entities;

/// <summary>
/// Course master data managed by central management.
/// </summary>
public class CourseEntity
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Course name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    /// <summary>
    /// Course description.
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Course category.
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// Course difficulty level.
    /// </summary>
    [MaxLength(50)]
    public string? Level { get; set; }

    /// <summary>
    /// When the course was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<TeamCourseEntity> TeamCourses { get; set; } = new List<TeamCourseEntity>();
}
