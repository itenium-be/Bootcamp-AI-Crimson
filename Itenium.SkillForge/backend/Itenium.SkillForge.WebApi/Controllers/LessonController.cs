using Itenium.SkillForge.Data;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

public record SetLessonStatusRequest(string Status);

public record LessonWithStatusDto(int Id, string Title, int SortOrder, string Status);

[ApiController]
[Route("api/lessons")]
[Authorize]
public class LessonController : ControllerBase
{
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

        await _db.SaveChangesAsync();
        return NoContent();
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
