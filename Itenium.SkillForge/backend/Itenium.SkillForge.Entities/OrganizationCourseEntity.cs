using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

/// <summary>
/// Junction table linking Organizations to their available Courses.
/// </summary>
public class OrganizationCourseEntity
{
    [Key]
    public int Id { get; set; }

    public int OrganizationId { get; set; }
    public OrganizationEntity? Organization { get; set; }

    public int CourseId { get; set; }
    public CourseEntity? Course { get; set; }

    /// <summary>
    /// When this course was made available to the organization.
    /// </summary>
    public DateTime EnabledAt { get; set; } = DateTime.UtcNow;
}
