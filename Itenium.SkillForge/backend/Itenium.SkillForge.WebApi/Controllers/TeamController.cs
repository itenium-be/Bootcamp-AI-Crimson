using System.Globalization;
using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

public record AddTeamMemberRequest(string UserId);

public record TeamProgressDto(IList<TeamMemberProgressDto> Members);

public record TeamMemberProgressDto(
    string UserId,
    string UserName,
    int EnrolledCourses,
    int CompletedCourses,
    double OverallPercent,
    IList<CourseProgressItem> Courses);

public record CourseProgressItem(
    int CourseId,
    string CourseName,
    int TotalLessons,
    int CompletedLessons,
    double PercentComplete,
    bool IsMandatory,
    bool IsOverdue);

public record CourseMemberProgressDto(
    int CourseId,
    string CourseName,
    int TotalLessons,
    IList<CourseMemberItem> Members);

public record CourseMemberItem(
    string UserId,
    string UserName,
    string Status,
    int CompletedLessons,
    double PercentComplete,
    bool IsMandatory,
    bool IsOverdue);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ISkillForgeUser _user;
    private readonly IUserRepository _userRepository;

    public TeamController(AppDbContext db, ISkillForgeUser user, IUserRepository userRepository)
    {
        _db = db;
        _user = user;
        _userRepository = userRepository;
    }

    /// <summary>Get the teams the current user has access to.</summary>
    [HttpGet]
    public async Task<ActionResult<List<TeamEntity>>> GetUserTeams()
    {
        if (_user.IsBackOffice)
        {
            return await _db.Teams.ToListAsync();
        }

        return await _db.Teams
            .Where(t => _user.Teams.Contains(t.Id))
            .ToListAsync();
    }

    /// <summary>Get members of a team. BackOffice or the team's own manager.</summary>
    [HttpGet("{id}/members")]
    public async Task<ActionResult<IList<UserResponse>>> GetTeamMembers(int id)
    {
        if (!_user.IsBackOffice && !_user.Teams.Contains(id)) return Forbid();
        var members = await _userRepository.GetTeamMembersAsync(id);
        return Ok(members);
    }

    /// <summary>Get active learners available to add to a team. BackOffice only.</summary>
    [HttpGet("{id}/available-learners")]
    public async Task<ActionResult<IList<UserResponse>>> GetAvailableLearners(int id)
    {
        if (!_user.IsBackOffice) return Forbid();
        var learners = await _userRepository.GetActiveLearnersAsync();
        return Ok(learners);
    }

    /// <summary>Add a learner to a team. BackOffice only.</summary>
    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddTeamMember(int id, [FromBody] AddTeamMemberRequest request)
    {
        if (!_user.IsBackOffice) return Forbid();
        var found = await _userRepository.AddTeamMemberAsync(id, request.UserId);
        if (!found) return NotFound();
        return NoContent();
    }

    /// <summary>Remove a learner from a team. BackOffice only.</summary>
    [HttpDelete("{id}/members/{userId}")]
    public async Task<IActionResult> RemoveTeamMember(int id, string userId)
    {
        if (!_user.IsBackOffice) return Forbid();
        var found = await _userRepository.RemoveTeamMemberAsync(id, userId);
        if (!found) return NotFound();
        return NoContent();
    }

    /// <summary>Get learning progress for all members of a team. BackOffice or the team's own manager.</summary>
    [HttpGet("{id}/progress")]
    public async Task<ActionResult<TeamProgressDto>> GetTeamProgress(int id)
    {
        if (!_user.IsBackOffice && !_user.Teams.Contains(id)) return Forbid();

        var members = await _userRepository.GetTeamMembersAsync(id);
        var memberIds = members.Select(m => m.Id).ToList();

        var lessonsByCourse = await _db.Lessons
            .AsNoTracking()
            .Select(l => new { l.Id, l.CourseId })
            .ToListAsync();

        var lessonCourseMap = lessonsByCourse.ToDictionary(l => l.Id, l => l.CourseId);
        var lessonCountByCourse = lessonsByCourse.GroupBy(l => l.CourseId)
            .ToDictionary(g => g.Key, g => g.Count());

        var enrollments = await _db.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .Where(e => memberIds.Contains(e.UserId))
            .ToListAsync();

        var lessonProgresses = await _db.LessonProgresses
            .AsNoTracking()
            .Where(p => memberIds.Contains(p.UserId))
            .ToListAsync();

        var mandatoryAssignments = await _db.CourseAssignments
            .AsNoTracking()
            .Where(a => a.Type == AssignmentType.Mandatory &&
                        ((a.AssigneeType == AssigneeType.Team && a.AssigneeId == id.ToString(System.Globalization.CultureInfo.InvariantCulture)) ||
                         (a.AssigneeType == AssigneeType.User && memberIds.Contains(a.AssigneeId))))
            .ToListAsync();

        var mandatoryCourseIds = mandatoryAssignments.Select(a => a.CourseId).ToHashSet();

        var memberDtos = members.Select(member =>
        {
            var memberEnrollments = enrollments.Where(e => e.UserId == member.Id).ToList();
            var memberProgressLessonIds = lessonProgresses
                .Where(p => p.UserId == member.Id)
                .Select(p => p.LessonId)
                .ToHashSet();

            var courses = memberEnrollments.Select(enrollment =>
            {
                var totalLessons = lessonCountByCourse.GetValueOrDefault(enrollment.CourseId, 0);
                var completedLessons = lessonsByCourse
                    .Count(l => l.CourseId == enrollment.CourseId && memberProgressLessonIds.Contains(l.Id));
                var pct = totalLessons > 0 ? Math.Round((double)completedLessons / totalLessons * 100, 1) : 0.0;
                var isMandatory = mandatoryCourseIds.Contains(enrollment.CourseId);
                var isCompleted = totalLessons > 0 && completedLessons == totalLessons;
                return new CourseProgressItem(
                    enrollment.CourseId,
                    enrollment.Course.Name,
                    totalLessons,
                    completedLessons,
                    pct,
                    isMandatory,
                    IsOverdue: isMandatory && !isCompleted);
            }).ToList();

            var totalLessonsAll = courses.Sum(c => c.TotalLessons);
            var completedLessonsAll = courses.Sum(c => c.CompletedLessons);
            var overallPct = totalLessonsAll > 0
                ? Math.Round((double)completedLessonsAll / totalLessonsAll * 100, 1)
                : 0.0;

            return new TeamMemberProgressDto(
                member.Id,
                member.Name ?? member.Email,
                memberEnrollments.Count,
                courses.Count(c => c.TotalLessons > 0 && c.CompletedLessons == c.TotalLessons),
                overallPct,
                courses);
        }).ToList();

        return Ok(new TeamProgressDto(memberDtos));
    }

    /// <summary>Get per-course progress breakdown for a team. BackOffice or the team's own manager.</summary>
    [HttpGet("{id}/courses/{courseId}/progress")]
    public async Task<ActionResult<CourseMemberProgressDto>> GetCourseProgress(int id, int courseId)
    {
        if (!_user.IsBackOffice && !_user.Teams.Contains(id)) return Forbid();

        var members = await _userRepository.GetTeamMembersAsync(id);
        var memberIds = members.Select(m => m.Id).ToList();

        var course = await _db.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == courseId);
        var courseName = course?.Name ?? string.Empty;

        var lessons = await _db.Lessons
            .AsNoTracking()
            .Where(l => l.CourseId == courseId)
            .Select(l => l.Id)
            .ToListAsync();

        var totalLessons = lessons.Count;

        var enrolledUserIds = await _db.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId && memberIds.Contains(e.UserId))
            .Select(e => e.UserId)
            .ToHashSetAsync();

        var progressByUser = await _db.LessonProgresses
            .AsNoTracking()
            .Where(p => memberIds.Contains(p.UserId) && lessons.Contains(p.LessonId))
            .GroupBy(p => p.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        var mandatoryForTeam = await _db.CourseAssignments
            .AsNoTracking()
            .AnyAsync(a => a.CourseId == courseId &&
                           a.Type == AssignmentType.Mandatory &&
                           ((a.AssigneeType == AssigneeType.Team && a.AssigneeId == id.ToString(System.Globalization.CultureInfo.InvariantCulture)) ||
                            (a.AssigneeType == AssigneeType.User && memberIds.Contains(a.AssigneeId))));

        var memberItems = members.Select(member =>
        {
            var completedLessons = progressByUser.GetValueOrDefault(member.Id, 0);
            var pct = totalLessons > 0 ? Math.Round((double)completedLessons / totalLessons * 100, 1) : 0.0;
            var isCompleted = totalLessons > 0 && completedLessons == totalLessons;
            var isEnrolled = enrolledUserIds.Contains(member.Id);

            var status = !isEnrolled ? "NotStarted"
                : isCompleted ? "Completed"
                : "InProgress";

            return new CourseMemberItem(
                member.Id,
                member.Name ?? member.Email,
                status,
                completedLessons,
                pct,
                mandatoryForTeam,
                IsOverdue: mandatoryForTeam && !isCompleted);
        }).ToList();

        return Ok(new CourseMemberProgressDto(courseId, courseName, totalLessons, memberItems));
    }
}
