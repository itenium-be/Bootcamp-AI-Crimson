using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/assignments")]
[Authorize]
public class CourseAssignmentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public CourseAssignmentsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>List all assignments for a course.</summary>
    [HttpGet]
    public async Task<ActionResult<List<CourseAssignmentEntity>>> GetAssignments(int courseId)
    {
        var course = await _db.Courses.FindAsync(courseId);
        if (course == null)
            return NotFound();

        var assignments = await _db.CourseAssignments
            .Where(a => a.CourseId == courseId)
            .ToListAsync();

        return Ok(assignments);
    }

    /// <summary>Assign a course to a team or individual.</summary>
    [HttpPost]
    public async Task<ActionResult<CourseAssignmentEntity>> CreateAssignment(int courseId, [FromBody] CreateAssignmentRequest request)
    {
        var course = await _db.Courses.FindAsync(courseId);
        if (course == null)
            return NotFound();

        var duplicate = await _db.CourseAssignments
            .AnyAsync(a => a.CourseId == courseId && a.AssigneeType == request.AssigneeType && a.AssigneeId == request.AssigneeId);

        if (duplicate)
            return Conflict("This course is already assigned to the specified assignee.");

        var assignment = new CourseAssignmentEntity
        {
            CourseId = courseId,
            AssigneeType = request.AssigneeType,
            AssigneeId = request.AssigneeId,
            AssigneeName = request.AssigneeName,
            Type = request.Type,
            AssignedBy = request.AssignedBy,
            AssignedAt = DateTime.UtcNow,
        };

        _db.CourseAssignments.Add(assignment);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAssignments), new { courseId }, assignment);
    }

    /// <summary>Remove an assignment.</summary>
    [HttpDelete("{assignmentId:int}")]
    public async Task<ActionResult> DeleteAssignment(int courseId, int assignmentId)
    {
        var assignment = await _db.CourseAssignments
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.CourseId == courseId);

        if (assignment == null)
            return NotFound();

        _db.CourseAssignments.Remove(assignment);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public record CreateAssignmentRequest(
    AssigneeType AssigneeType,
    string AssigneeId,
    string? AssigneeName,
    AssignmentType Type,
    string AssignedBy);
