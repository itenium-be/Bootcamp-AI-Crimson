using Itenium.SkillForge.Data;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

public record SetLessonStatusRequest(string Status);

public record LessonWithStatusDto(int Id, string Title, int SortOrder, string Status);

public record LessonProgressSummaryDto(int LessonId, int CompletedCount);

[ApiController]
[Route("api/lessons")]
[Authorize]
public class LessonController : ControllerBase
{
    // ---- Manager CRUD endpoints ----

    /// <summary>
    /// Get all lessons for a course (manager view with EstimatedDuration).
    /// </summary>
    [HttpGet("/api/courses/{courseId:int}/lessons")]
    public async Task<ActionResult<IList<LessonDto>>> GetCourseLessons(int courseId)
    {
        if (!_user.IsManager) return Forbid();
        var lessons = await _db.Lessons
            .AsNoTracking()
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.SortOrder)
            .Select(l => new LessonDto(l.Id, l.Title, l.EstimatedDuration, l.SortOrder))
            .ToListAsync();
        return Ok(lessons);
    }

    /// <summary>
    /// Get a single lesson by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<LessonDto>> GetLesson(int id)
    {
        var lesson = await _db.Lessons.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
        if (lesson == null) return NotFound();
        return Ok(new LessonDto(lesson.Id, lesson.Title, lesson.EstimatedDuration, lesson.SortOrder));
    }

    /// <summary>
    /// Create a lesson for a course.
    /// </summary>
    [HttpPost("/api/courses/{courseId:int}/lessons")]
    public async Task<ActionResult<LessonDto>> CreateLesson(int courseId, [FromBody] CreateLessonRequest request)
    {
        if (!_user.IsManager) return Forbid();
        var lesson = new LessonEntity
        {
            CourseId = courseId,
            Title = request.Title,
            EstimatedDuration = request.EstimatedDuration,
            SortOrder = request.SortOrder,
        };
        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetLesson), new { id = lesson.Id }, new LessonDto(lesson.Id, lesson.Title, lesson.EstimatedDuration, lesson.SortOrder));
    }

    /// <summary>
    /// Update a lesson's title, duration, and sort order.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateLesson(int id, [FromBody] UpdateLessonRequest request)
    {
        if (!_user.IsManager) return Forbid();
        var lesson = await _db.Lessons.FindAsync(id);
        if (lesson == null) return NotFound();
        lesson.Title = request.Title;
        lesson.EstimatedDuration = request.EstimatedDuration;
        lesson.SortOrder = request.SortOrder;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Delete a lesson. Returns 409 Conflict if any learner has completed it.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLesson(int id)
    {
        if (!_user.IsManager) return Forbid();
        var hasCompletions = await _db.LessonStatuses.AnyAsync(s => s.LessonId == id && s.Status == LessonStatusValue.Done);
        if (hasCompletions) return Conflict();
        var lesson = await _db.Lessons.FindAsync(id);
        if (lesson == null) return NotFound();
        _db.Lessons.Remove(lesson);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Reorder lessons within a course.
    /// </summary>
    [HttpPut("/api/courses/{courseId:int}/lessons/reorder")]
    public async Task<IActionResult> ReorderLessons(int courseId, [FromBody] ReorderLessonsRequest request)
    {
        if (!_user.IsManager) return Forbid();
        var lessons = await _db.Lessons.Where(l => l.CourseId == courseId).ToListAsync();
        for (var i = 0; i < request.OrderedLessonIds.Length; i++)
        {
            var lesson = lessons.FirstOrDefault(l => l.Id == request.OrderedLessonIds[i]);
            if (lesson != null) lesson.SortOrder = i + 1;
        }
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ---- Learner status endpoints ----

    private readonly AppDbContext _db;
    private readonly ISkillForgeUser _user;

    public LessonController(AppDbContext db, ISkillForgeUser user)
    {
        _db = db;
        _user = user;
    }

    /// <summary>
    /// Get all lessons for a course with the current user's status.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IList<LessonWithStatusDto>>> GetLessons([FromQuery] int courseId)
    {
        var lessons = await _db.Lessons
            .AsNoTracking()
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.SortOrder)
            .ToListAsync();

        var lessonIds = lessons.Select(l => l.Id).ToList();
        var statuses = await _db.LessonStatuses
            .AsNoTracking()
            .Where(s => s.UserId == _user.UserId && lessonIds.Contains(s.LessonId))
            .ToDictionaryAsync(s => s.LessonId, s => s.Status);

        var result = lessons.Select(l =>
        {
            var status = statuses.TryGetValue(l.Id, out var s) ? StatusToString(s) : "new";
            return new LessonWithStatusDto(l.Id, l.Title, l.SortOrder, status);
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Get the current user's status for a specific lesson.
    /// </summary>
    [HttpGet("{lessonId:int}/status")]
    public async Task<ActionResult<string>> GetStatus(int lessonId)
    {
        var row = await _db.LessonStatuses
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.UserId == _user.UserId && s.LessonId == lessonId);

        return Ok(row == null ? "new" : StatusToString(row.Status));
    }

    /// <summary>
    /// Set the current user's status for a lesson. Use "new" to remove/reset.
    /// </summary>
    [HttpPut("{lessonId:int}/status")]
    public async Task<IActionResult> SetStatus(int lessonId, [FromBody] SetLessonStatusRequest request)
    {
        if (!TryParseStatus(request.Status, out var statusValue, out var isNew))
        {
            return BadRequest($"Invalid status '{request.Status}'. Use 'done', 'later', or 'new'.");
        }

        var lessonExists = await _db.Lessons.AnyAsync(l => l.Id == lessonId);
        if (!lessonExists)
        {
            return NotFound();
        }

        var row = await _db.LessonStatuses
            .SingleOrDefaultAsync(s => s.UserId == _user.UserId && s.LessonId == lessonId);

        if (isNew)
        {
            if (row != null)
            {
                _db.LessonStatuses.Remove(row);
                await _db.SaveChangesAsync();
                await UpdateEnrollmentCompletionAsync(lessonId, null);
            }
            return NoContent();
        }

        if (row == null)
        {
            _db.LessonStatuses.Add(new LessonStatusEntity
            {
                UserId = _user.UserId!,
                LessonId = lessonId,
                Status = statusValue,
                UpdatedAt = DateTime.UtcNow,
            });
        }
        else
        {
            row.Status = statusValue;
            row.UpdatedAt = DateTime.UtcNow;
        }

        // Record completion in LessonProgress when status is set to "done"
        if (statusValue == LessonStatusValue.Done)
        {
            var alreadyCompleted = await _db.LessonProgresses
                .AnyAsync(p => p.UserId == _user.UserId && p.LessonId == lessonId);
            if (!alreadyCompleted)
            {
                _db.LessonProgresses.Add(new LessonProgressEntity
                {
                    UserId = _user.UserId!,
                    LessonId = lessonId,
                });
            }
        }

        await _db.SaveChangesAsync();

        // Auto-complete enrollment when all lessons in the course are done
        await UpdateEnrollmentCompletionAsync(lessonId, statusValue);

        return NoContent();
    }

    /// <summary>
    /// Get the number of learners who have completed a lesson.
    /// </summary>
    [HttpGet("{lessonId:int}/progress-summary")]
    public async Task<ActionResult<LessonProgressSummaryDto>> GetProgressSummary(int lessonId)
    {
        var count = await _db.LessonProgresses
            .AsNoTracking()
            .CountAsync(p => p.LessonId == lessonId);

        return Ok(new LessonProgressSummaryDto(lessonId, count));
    }

    /// <summary>
    /// Reset all learner progress for a lesson. Manager/backoffice only.
    /// </summary>
    [HttpDelete("{lessonId:int}/progress")]
    public async Task<IActionResult> ResetProgress(int lessonId)
    {
        if (!_user.IsBackOffice)
            return Forbid();

        var rows = await _db.LessonProgresses.Where(p => p.LessonId == lessonId).ToListAsync();
        _db.LessonProgresses.RemoveRange(rows);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Mark a lesson as completed for the current user.
    /// </summary>
    [HttpPost("{lessonId:int}/complete")]
    public async Task<IActionResult> CompleteLesson(int lessonId)
    {
        var lessonExists = await _db.Lessons.AnyAsync(l => l.Id == lessonId);
        if (!lessonExists)
        {
            return NotFound();
        }

        var row = await _db.LessonStatuses
            .SingleOrDefaultAsync(s => s.UserId == _user.UserId && s.LessonId == lessonId);

        if (row == null)
        {
            _db.LessonStatuses.Add(new LessonStatusEntity
            {
                UserId = _user.UserId!,
                LessonId = lessonId,
                Status = LessonStatusValue.Done,
                UpdatedAt = DateTime.UtcNow,
            });
        }
        else
        {
            row.Status = LessonStatusValue.Done;
            row.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task UpdateEnrollmentCompletionAsync(int lessonId, LessonStatusValue? newStatus)
    {
        var lesson = await _db.Lessons.AsNoTracking().FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson == null) return;

        var enrollment = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == _user.UserId && e.CourseId == lesson.CourseId);
        if (enrollment == null) return;

        var allLessonIds = await _db.Lessons
            .AsNoTracking()
            .Where(l => l.CourseId == lesson.CourseId)
            .Select(l => l.Id)
            .ToListAsync();

        var doneCount = await _db.LessonStatuses
            .AsNoTracking()
            .CountAsync(s => s.UserId == _user.UserId && allLessonIds.Contains(s.LessonId) && s.Status == LessonStatusValue.Done);

        var allDone = doneCount == allLessonIds.Count;

        if (allDone && enrollment.Status != EnrollmentStatus.Completed)
        {
            enrollment.Status = EnrollmentStatus.Completed;
            enrollment.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        else if (!allDone && enrollment.Status == EnrollmentStatus.Completed)
        {
            enrollment.Status = EnrollmentStatus.Active;
            enrollment.CompletedAt = null;
            await _db.SaveChangesAsync();
        }
    }

    private static string StatusToString(LessonStatusValue status) => status switch
    {
        LessonStatusValue.Done => "done",
        LessonStatusValue.Later => "later",
        _ => "new",
    };

    private static bool TryParseStatus(string status, out LessonStatusValue value, out bool isNew)
    {
        isNew = false;
        value = default;

        switch (status.ToLowerInvariant())
        {
            case "new":
                isNew = true;
                return true;
            case "done":
                value = LessonStatusValue.Done;
                return true;
            case "later":
                value = LessonStatusValue.Later;
                return true;
            default:
                return false;
        }
    }
}
