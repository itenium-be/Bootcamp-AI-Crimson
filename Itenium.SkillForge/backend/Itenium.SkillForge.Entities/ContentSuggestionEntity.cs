using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

public enum ContentSuggestionStatus
{
    Pending,
    Approved,
    Rejected,
}

public class ContentSuggestionEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public required string SubmittedBy { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string Description { get; set; }

    [MaxLength(500)]
    public string? Url { get; set; }

    public int? RelatedCourseId { get; set; }

    [MaxLength(200)]
    public string? Topic { get; set; }

    public ContentSuggestionStatus Status { get; set; } = ContentSuggestionStatus.Pending;

    [MaxLength(450)]
    public string? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    [MaxLength(1000)]
    public string? ReviewNote { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
