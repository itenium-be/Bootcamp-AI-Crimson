using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

public class CourseAssignmentEntity
{
    [Key]
    public int Id { get; set; }

    public int CourseId { get; set; }

    public CourseEntity Course { get; set; } = null!;

    public AssigneeType AssigneeType { get; set; }

    /// <summary>Team ID or User ID depending on AssigneeType.</summary>
    [Required]
    [MaxLength(450)]
    public required string AssigneeId { get; set; }

    /// <summary>Display name of the assignee (team name or user name).</summary>
    [MaxLength(200)]
    public string? AssigneeName { get; set; }

    public AssignmentType Type { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(450)]
    public required string AssignedBy { get; set; }
}
