using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

/// <summary>
/// Junction table linking Teams to their available Courses.
/// </summary>
public class TeamCourseEntity
{
    [Key]
    public int Id { get; set; }

    public int TeamId { get; set; }
    public TeamEntity? Team { get; set; }

    public int CourseId { get; set; }
    public CourseEntity? Course { get; set; }

    /// <summary>
    /// When this course was made available to the team.
    /// </summary>
    public DateTime EnabledAt { get; set; } = DateTime.UtcNow;
}
