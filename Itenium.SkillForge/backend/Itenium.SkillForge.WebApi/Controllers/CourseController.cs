using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CourseController : ControllerBase
{
    private readonly AppDbContext _db;

    public CourseController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get all courses.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IList<CourseEntity>>> GetCourses()
    {
        var courses = await _db.Courses.ToListAsync();
        return Ok(courses);
    }

    /// <summary>
    /// Get a course by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CourseEntity>> GetCourse(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        return Ok(course);
    }

    /// <summary>
    /// Create a new course. Starts in Draft status.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CourseEntity>> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var course = new CourseEntity
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Level = request.Level,
            EstimatedDuration = request.EstimatedDuration,
            Status = CourseStatus.Draft,
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
    }

    /// <summary>
    /// Update an existing course. Allowed in Draft and Published states.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CourseEntity>> UpdateCourse(int id, [FromBody] UpdateCourseRequest request)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        course.Name = request.Name;
        course.Description = request.Description;
        course.Category = request.Category;
        course.Level = request.Level;
        course.EstimatedDuration = request.EstimatedDuration;

        await _db.SaveChangesAsync();

        return Ok(course);
    }

    /// <summary>
    /// Publish a course — makes it visible to learners.
    /// </summary>
    [HttpPut("{id:int}/publish")]
    public async Task<ActionResult> PublishCourse(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        course.Status = CourseStatus.Published;
        await _db.SaveChangesAsync();

        return Ok(course);
    }

    /// <summary>
    /// Archive a course — hides it from learners but preserves history.
    /// </summary>
    [HttpPut("{id:int}/archive")]
    public async Task<ActionResult> ArchiveCourse(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        course.Status = CourseStatus.Archived;
        await _db.SaveChangesAsync();

        return Ok(course);
    }

    /// <summary>
    /// Delete a course. Only allowed for Draft courses with no learner progress.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteCourse(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        if (course.Status != CourseStatus.Draft)
        {
            return Conflict("Cannot delete a published or archived course. Archive it instead.");
        }

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
