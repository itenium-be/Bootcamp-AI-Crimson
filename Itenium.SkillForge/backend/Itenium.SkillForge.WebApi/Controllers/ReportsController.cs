using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

public record ReportSummaryDto(
    int ActiveLearners,
    int CompletionsThisMonth,
    int TotalEnrollments
);

public record CourseUsageDto(
    int CourseId,
    string CourseName,
    int TotalEnrollments,
    int Completions,
    double CompletionRate
);

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ISkillForgeUser _user;

    public ReportsController(AppDbContext db, ISkillForgeUser user)
    {
        _db = db;
        _user = user;
    }

    private bool CanAccess => _user.IsBackOffice;

    /// <summary>
    /// Platform-wide KPIs: active learners, completions this month, total enrollments.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<ReportSummaryDto>> GetSummary()
    {
        if (!CanAccess) return Forbid();

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var activeLearners = await _db.Enrollments
            .AsNoTracking()
            .Where(e => e.Status == EnrollmentStatus.Active)
            .Select(e => e.UserId)
            .Distinct()
            .CountAsync();

        var completionsThisMonth = await _db.Enrollments
            .AsNoTracking()
            .Where(e => e.Status == EnrollmentStatus.Completed && e.EnrolledAt >= monthStart)
            .CountAsync();

        var totalEnrollments = await _db.Enrollments
            .AsNoTracking()
            .CountAsync();

        return Ok(new ReportSummaryDto(activeLearners, completionsThisMonth, totalEnrollments));
    }

    /// <summary>
    /// Per-course usage report with optional filters. Ordered by total enrollments desc.
    /// </summary>
    [HttpGet("course-usage")]
    public async Task<ActionResult<IList<CourseUsageDto>>> GetCourseUsage(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? courseId)
    {
        if (!CanAccess) return Forbid();

        var query = _db.Enrollments.AsNoTracking().AsQueryable();

        if (from.HasValue) query = query.Where(e => e.EnrolledAt >= from.Value);
        if (to.HasValue) query = query.Where(e => e.EnrolledAt <= to.Value);
        if (courseId.HasValue) query = query.Where(e => e.CourseId == courseId.Value);

        var grouped = await query
            .GroupBy(e => e.CourseId)
            .Select(g => new
            {
                CourseId = g.Key,
                Total = g.Count(),
                Completed = g.Count(e => e.Status == EnrollmentStatus.Completed),
            })
            .OrderByDescending(g => g.Total)
            .ToListAsync();

        var courseIds = grouped.Select(g => g.CourseId).ToList();
        var courses = await _db.Courses
            .AsNoTracking()
            .Where(c => courseIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name);

        var result = grouped.Select(g => new CourseUsageDto(
            g.CourseId,
            courses.TryGetValue(g.CourseId, out var name) ? name : "Unknown",
            g.Total,
            g.Completed,
            g.Total == 0 ? 0 : Math.Round((double)g.Completed / g.Total * 100, 1)
        )).ToList();

        return Ok(result);
    }
}
