using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Itenium.SkillForge.Entities;

public class EnrollmentEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required string UserId { get; set; }

    public int CourseId { get; set; }

    [ForeignKey(nameof(CourseId))]
    public CourseEntity Course { get; set; } = null!;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    public int? LastVisitedLessonId { get; set; }

    public DateTime? CompletedAt { get; set; }
}
