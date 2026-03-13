using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Itenium.SkillForge.WebApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class EnrollmentController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ISkillForgeUser _currentUser;

    public EnrollmentController(AppDbContext db, ISkillForgeUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Enroll the current user in a course. Idempotent.
    /// </summary>
    [HttpPost("courses/{courseId:int}/enroll")]
    public async Task<ActionResult<EnrollmentEntity>> Enroll(int courseId)
    {
        var userId = _currentUser.Id;
        if (userId == null)
        {
            return Unauthorized();
        }

        var course = await _db.Courses.FindAsync(courseId);
        if (course == null)
        {
            return NotFound();
        }

        if (course.Status != CourseStatus.Published)
        {
            return BadRequest("Course is not published.");
        }

        var existing = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

        if (existing != null)
        {
            return Ok(existing);
        }

        var enrollment = new EnrollmentEntity
        {
            UserId = userId,
            CourseId = courseId,
        };

        _db.Enrollments.Add(enrollment);
        await _db.SaveChangesAsync();

        return Ok(enrollment);
    }

    /// <summary>
    /// Get all enrollments for the current user, optionally filtered by status (active/completed).
    /// </summary>
    [HttpGet("enrollments/me")]
    public async Task<ActionResult<IList<EnrollmentResponse>>> GetMyEnrollments([FromQuery] string? status = null)
    {
        var userId = _currentUser.Id;
        if (userId == null)
        {
            return Unauthorized();
        }

        var query = _db.Enrollments
            .Include(e => e.Course)
            .Where(e => e.UserId == userId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<EnrollmentStatus>(status, ignoreCase: true, out var statusEnum))
            {
                query = query.Where(e => e.Status == statusEnum);
            }
        }

        var enrollments = await query
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        var moduleIds = enrollments
            .Where(e => e.Course.ModuleId.HasValue)
            .Select(e => e.Course.ModuleId!.Value)
            .Distinct()
            .ToList();

        var modules = await _db.Modules
            .AsNoTracking()
            .Where(m => moduleIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, m => m.Name);

        var result = enrollments.Select(e => new EnrollmentResponse(
            e.Id,
            e.CourseId,
            e.Course.Name,
            e.Course.Category,
            e.Course.Level,
            e.EnrolledAt,
            e.Status.ToString(),
            e.CompletedAt,
            e.Course.ModuleId.HasValue && modules.TryGetValue(e.Course.ModuleId.Value, out var moduleName) ? moduleName : null
        )).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Returns the next lesson to resume for the current user in a course.
    /// First lesson where status != done, ordered by SortOrder.
    /// If all done, returns IsComplete=true with LastVisitedLessonId as revisit option.
    /// </summary>
    [HttpGet("courses/{courseId:int}/resume")]
    public async Task<ActionResult<ResumeResponse>> Resume(int courseId)
    {
        var userId = _currentUser.Id;
        var enrollment = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

        if (enrollment == null)
            return NotFound();

        var lessons = await _db.Lessons
            .AsNoTracking()
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.SortOrder)
            .ToListAsync();

        if (lessons.Count == 0)
            return Ok(new ResumeResponse(null, true));

        var lessonIds = lessons.Select(l => l.Id).ToList();
        var doneIds = await _db.LessonStatuses
            .AsNoTracking()
            .Where(s => s.UserId == userId && lessonIds.Contains(s.LessonId) && s.Status == LessonStatusValue.Done)
            .Select(s => s.LessonId)
            .ToListAsync();

        var nextLesson = lessons.FirstOrDefault(l => !doneIds.Contains(l.Id));
        if (nextLesson != null)
            return Ok(new ResumeResponse(nextLesson.Id, false));

        return Ok(new ResumeResponse(enrollment.LastVisitedLessonId, true));
    }

    /// <summary>
    /// Track the last visited lesson for the current user in a course.
    /// </summary>
    [HttpPut("courses/{courseId:int}/last-visited/{lessonId:int}")]
    public async Task<IActionResult> TrackLastVisited(int courseId, int lessonId)
    {
        var userId = _currentUser.Id;
        var enrollment = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

        if (enrollment == null)
            return NotFound();

        enrollment.LastVisitedLessonId = lessonId;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record ResumeResponse(int? LessonId, bool IsComplete);

public record EnrollmentResponse(
    int Id,
    int CourseId,
    string CourseName,
    string? CourseCategory,
    string? CourseLevel,
    DateTime EnrolledAt,
    string Status,
    DateTime? CompletedAt = null,
    string? ModuleName = null
);
