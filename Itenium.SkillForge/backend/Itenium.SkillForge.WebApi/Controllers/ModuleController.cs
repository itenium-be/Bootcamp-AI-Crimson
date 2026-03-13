using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

public record ModuleRequest(string Name, string? Description, string? Goal);

public record ModuleCourseRequest(int CourseId, int Order);

public record ReorderCoursesRequest(IList<int> OrderedCourseIds);

public record ModuleCourseDto(int CourseId, string CourseName, int Order);

public record ModuleResponse(int Id, string Name, string? Description, string? Goal, IList<ModuleCourseDto> Courses);

[ApiController]
[Route("api/modules")]
[Authorize]
public class ModuleController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ISkillForgeUser _user;

    public ModuleController(AppDbContext db, ISkillForgeUser user)
    {
        _db = db;
        _user = user;
    }

    [HttpGet]
    public async Task<ActionResult<IList<ModuleResponse>>> GetModules()
    {
        var modules = await _db.Modules.AsNoTracking().ToListAsync();
        var coursesByModule = (await _db.Courses
            .AsNoTracking()
            .Where(c => c.ModuleId != null)
            .ToListAsync())
            .GroupBy(c => c.ModuleId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.ModuleOrder).ToList());

        var result = modules.Select(m =>
        {
            var courses = coursesByModule.TryGetValue(m.Id, out var list)
                ? list.Select(c => new ModuleCourseDto(c.Id, c.Name, c.ModuleOrder)).ToList()
                : new List<ModuleCourseDto>();
            return new ModuleResponse(m.Id, m.Name, m.Description, m.Goal, courses);
        }).ToList();

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ModuleResponse>> CreateModule([FromBody] ModuleRequest request)
    {
        var module = new ModuleEntity
        {
            Name = request.Name,
            Description = request.Description,
            Goal = request.Goal,
            CreatedBy = _user.Id,
        };

        _db.Modules.Add(module);
        await _db.SaveChangesAsync();

        return Ok(new ModuleResponse(module.Id, module.Name, module.Description, module.Goal, []));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateModule(int id, [FromBody] ModuleRequest request)
    {
        var module = await _db.Modules.FindAsync(id);
        if (module is null) return NotFound();

        module.Name = request.Name;
        module.Description = request.Description;
        module.Goal = request.Goal;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteModule(int id)
    {
        var module = await _db.Modules.FindAsync(id);
        if (module is null) return NotFound();

        // Unassign all courses in this module
        var courses = await _db.Courses.Where(c => c.ModuleId == id).ToListAsync();
        foreach (var course in courses)
        {
            course.ModuleId = null;
            course.ModuleOrder = 0;
        }

        _db.Modules.Remove(module);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id:int}/courses")]
    public async Task<IActionResult> AddCourse(int id, [FromBody] ModuleCourseRequest request)
    {
        var moduleExists = await _db.Modules.AnyAsync(m => m.Id == id);
        if (!moduleExists) return NotFound();

        var course = await _db.Courses.FindAsync(request.CourseId);
        if (course is null) return NotFound();

        if (course.ModuleId != null && course.ModuleId != id)
            return BadRequest("Course is already assigned to another module.");

        course.ModuleId = id;
        course.ModuleOrder = request.Order;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}/courses/{courseId:int}")]
    public async Task<IActionResult> RemoveCourse(int id, int courseId)
    {
        var course = await _db.Courses.FindAsync(courseId);
        if (course is null || course.ModuleId != id) return NotFound();

        course.ModuleId = null;
        course.ModuleOrder = 0;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id:int}/courses/reorder")]
    public async Task<IActionResult> ReorderCourses(int id, [FromBody] ReorderCoursesRequest request)
    {
        var courses = await _db.Courses.Where(c => c.ModuleId == id).ToListAsync();

        for (var i = 0; i < request.OrderedCourseIds.Count; i++)
        {
            var course = courses.FirstOrDefault(c => c.Id == request.OrderedCourseIds[i]);
            if (course != null)
                course.ModuleOrder = i + 1;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
