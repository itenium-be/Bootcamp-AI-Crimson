using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

public record LearnerCourseProgress(
    int CourseId,
    string CourseName,
    int CompletedLessons,
    int TotalLessons,
    int CompletionPercent,
    bool IsMandatory
);

public record LearnerModuleResponse(
    int Id,
    string Name,
    string? Description,
    int CompletionPercent,
    IList<LearnerCourseProgress> Courses
);

[ApiController]
[Route("api")]
[Authorize]
public class LearnerModulesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ISkillForgeUser _user;

    public LearnerModulesController(AppDbContext db, ISkillForgeUser user)
    {
        _db = db;
        _user = user;
    }

    /// <summary>
    /// Returns all modules with course completion progress for the current learner.
    /// </summary>
    [HttpGet("learners/me/modules")]
    public async Task<ActionResult<IList<LearnerModuleResponse>>> GetMyModules()
    {
        var response = await BuildModulesWithProgress();
        return Ok(response);
    }

    /// <summary>
    /// Returns detailed progress for a single module for the current learner.
    /// </summary>
    [HttpGet("modules/{id:int}/progress")]
    public async Task<ActionResult<LearnerModuleResponse>> GetModuleProgress(int id)
    {
        var moduleExists = await _db.Modules.AnyAsync(m => m.Id == id);
        if (!moduleExists)
            return NotFound();

        var all = await BuildModulesWithProgress(id);
        var module = all.FirstOrDefault(m => m.Id == id);
        if (module is null)
            return NotFound();

        return Ok(module);
    }

    private async Task<IList<LearnerModuleResponse>> BuildModulesWithProgress(int? filterModuleId = null)
    {
        var userId = _user.Id ?? string.Empty;

        var moduleQuery = _db.Modules.AsNoTracking();
        if (filterModuleId.HasValue)
            moduleQuery = moduleQuery.Where(m => m.Id == filterModuleId.Value);

        var modules = await moduleQuery.ToListAsync();
        if (modules.Count == 0)
            return [];

        var moduleIds = modules.Select(m => m.Id).ToList();

        var courses = await _db.Courses
            .AsNoTracking()
            .Where(c => c.ModuleId != null && moduleIds.Contains(c.ModuleId.Value))
            .OrderBy(c => c.ModuleOrder)
            .ToListAsync();

        if (courses.Count == 0)
            return modules.Select(m => new LearnerModuleResponse(m.Id, m.Name, m.Description, 0, [])).ToList();

        var courseIds = courses.Select(c => c.Id).ToList();

        var lessons = await _db.Lessons
            .AsNoTracking()
            .Where(l => courseIds.Contains(l.CourseId))
            .ToListAsync();

        var lessonIds = lessons.Select(l => l.Id).ToList();

        var doneStatuses = await _db.LessonStatuses
            .AsNoTracking()
            .Where(s => s.UserId == userId && lessonIds.Contains(s.LessonId) && s.Status == LessonStatusValue.Done)
            .Select(s => s.LessonId)
            .ToHashSetAsync();

        var mandatoryAssignments = await _db.CourseAssignments
            .AsNoTracking()
            .Where(a => a.AssigneeType == AssigneeType.User && a.AssigneeId == userId
                        && a.Type == AssignmentType.Mandatory && courseIds.Contains(a.CourseId))
            .Select(a => a.CourseId)
            .ToHashSetAsync();

        var lessonsByCourse = lessons.GroupBy(l => l.CourseId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var coursesByCourse = courses
            .GroupBy(c => c.ModuleId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = modules.Select(m =>
        {
            var moduleCourses = coursesByCourse.TryGetValue(m.Id, out var cl) ? cl : [];
            var courseProgressList = moduleCourses.Select(c =>
            {
                var courseLessons = lessonsByCourse.TryGetValue(c.Id, out var ll) ? ll : [];
                var total = courseLessons.Count;
                var completed = courseLessons.Count(l => doneStatuses.Contains(l.Id));
                var pct = total == 0 ? 0 : (int)Math.Round(100.0 * completed / total);
                var isMandatory = mandatoryAssignments.Contains(c.Id);
                return new LearnerCourseProgress(c.Id, c.Name, completed, total, pct, isMandatory);
            }).ToList();

            var moduleCompletionPct = courseProgressList.Count == 0
                ? 0
                : (int)Math.Round(courseProgressList.Average(cp => cp.CompletionPercent));

            return new LearnerModuleResponse(m.Id, m.Name, m.Description, moduleCompletionPct, courseProgressList);
        }).ToList();

        return result;
    }
}
