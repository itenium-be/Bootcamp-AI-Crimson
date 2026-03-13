using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

public record SubmitFeedbackRequest(int Rating, string? Comment);

public record FeedbackEntryDto(int Id, string? UserId, int Rating, string? Comment, DateTime SubmittedAt, bool IsFlagged);

public record CourseFeedbackSummaryDto(double AverageRating, IList<FeedbackEntryDto> Entries);

public record CourseFeedbackRankingDto(int CourseId, string CourseName, double AverageRating, int Count);

[ApiController]
[Authorize]
public class FeedbackController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ISkillForgeUser _user;

    public FeedbackController(AppDbContext db, ISkillForgeUser user)
    {
        _db = db;
        _user = user;
    }

    /// <summary>
    /// Submit feedback for a course.
    /// </summary>
    [HttpPost("api/courses/{courseId:int}/feedback")]
    public async Task<ActionResult<FeedbackEntryDto>> SubmitFeedback(int courseId, [FromBody] SubmitFeedbackRequest request)
    {
        var courseExists = await _db.Courses.AnyAsync(c => c.Id == courseId);
        if (!courseExists)
            return NotFound();

        if (request.Rating < 1 || request.Rating > 5)
            return BadRequest("Rating must be between 1 and 5.");

        var fb = new CourseFeedbackEntity
        {
            UserId = _user.Id!,
            CourseId = courseId,
            Rating = request.Rating,
            Comment = request.Comment,
        };

        _db.CourseFeedbacks.Add(fb);
        await _db.SaveChangesAsync();

        return Ok(ToDto(fb, anonymize: false));
    }

    /// <summary>
    /// Get all feedback for a course (anonymized).
    /// </summary>
    [HttpGet("api/courses/{courseId:int}/feedback")]
    public async Task<ActionResult<CourseFeedbackSummaryDto>> GetCourseFeedback(int courseId, [FromQuery] int? minRating = null)
    {
        var query = _db.CourseFeedbacks
            .AsNoTracking()
            .Where(f => f.CourseId == courseId && f.LessonId == null);

        if (minRating.HasValue)
            query = query.Where(f => f.Rating >= minRating.Value);

        var entries = await query.OrderByDescending(f => f.SubmittedAt).ToListAsync();
        var average = entries.Count > 0 ? entries.Average(f => f.Rating) : 0.0;

        return Ok(new CourseFeedbackSummaryDto(
            average,
            entries.Select(f => ToDto(f, anonymize: true)).ToList()
        ));
    }

    /// <summary>
    /// Get all feedback for a lesson (anonymized).
    /// </summary>
    [HttpGet("api/lessons/{lessonId:int}/feedback")]
    public async Task<ActionResult<CourseFeedbackSummaryDto>> GetLessonFeedback(int lessonId)
    {
        var entries = await _db.CourseFeedbacks
            .AsNoTracking()
            .Where(f => f.LessonId == lessonId)
            .OrderByDescending(f => f.SubmittedAt)
            .ToListAsync();

        var average = entries.Count > 0 ? entries.Average(f => f.Rating) : 0.0;

        return Ok(new CourseFeedbackSummaryDto(
            average,
            entries.Select(f => ToDto(f, anonymize: true)).ToList()
        ));
    }

    /// <summary>
    /// Flag a feedback entry for review.
    /// </summary>
    [HttpPut("api/feedback/{id:int}/flag")]
    public async Task<IActionResult> FlagFeedback(int id)
    {
        var fb = await _db.CourseFeedbacks.FindAsync(id);
        if (fb is null)
            return NotFound();

        fb.IsFlagged = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Dismiss a flagged feedback entry.
    /// </summary>
    [HttpPut("api/feedback/{id:int}/dismiss")]
    public async Task<IActionResult> DismissFeedback(int id)
    {
        var fb = await _db.CourseFeedbacks.FindAsync(id);
        if (fb is null)
            return NotFound();

        fb.IsDismissed = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Get a ranking of courses by average feedback rating. Backoffice only.
    /// </summary>
    [HttpGet("api/reports/feedback-summary")]
    public async Task<ActionResult<IList<CourseFeedbackRankingDto>>> GetFeedbackSummary()
    {
        if (!_user.IsBackOffice)
            return Forbid();

        var ranking = await _db.CourseFeedbacks
            .AsNoTracking()
            .GroupBy(f => f.CourseId)
            .Select(g => new
            {
                CourseId = g.Key,
                AverageRating = g.Average(f => f.Rating),
                Count = g.Count(),
            })
            .OrderByDescending(x => x.AverageRating)
            .Join(
                _db.Courses.AsNoTracking(),
                g => g.CourseId,
                c => c.Id,
                (g, c) => new CourseFeedbackRankingDto(g.CourseId, c.Name, g.AverageRating, g.Count)
            )
            .ToListAsync();

        return Ok(ranking);
    }

    private static FeedbackEntryDto ToDto(CourseFeedbackEntity fb, bool anonymize) =>
        new(fb.Id, anonymize ? null : fb.UserId, fb.Rating, fb.Comment, fb.SubmittedAt, fb.IsFlagged);
}
