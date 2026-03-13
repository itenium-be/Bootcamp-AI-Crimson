using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

public record CreateAnnotationRequest(string Content, int? Rating);
public record UpdateAnnotationRequest(string Content, int? Rating);
public record AnnotationDto(int Id, string DisplayName, string Content, int? Rating, DateTime CreatedAt, DateTime UpdatedAt, bool IsOwn);
public record AnnotationsPageDto(IList<AnnotationDto> Items, int TotalCount);

[ApiController]
[Route("api")]
[Authorize]
public class AnnotationController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ISkillForgeUser _user;

    public AnnotationController(AppDbContext db, ISkillForgeUser user)
    {
        _db = db;
        _user = user;
    }

    /// <summary>
    /// Create an annotation for a lesson.
    /// </summary>
    [HttpPost("lessons/{lessonId:int}/annotations")]
    public async Task<ActionResult<AnnotationDto>> CreateAnnotation(int lessonId, [FromBody] CreateAnnotationRequest request)
    {
        var lessonExists = await _db.Lessons.AnyAsync(l => l.Id == lessonId);
        if (!lessonExists)
            return NotFound();

        if (request.Rating.HasValue && (request.Rating < 1 || request.Rating > 5))
            return BadRequest("Rating must be between 1 and 5.");

        var annotation = new LessonAnnotationEntity
        {
            UserId = _user.Id!,
            DisplayName = _user.DisplayName,
            LessonId = lessonId,
            Content = request.Content,
            Rating = request.Rating,
        };

        _db.LessonAnnotations.Add(annotation);
        await _db.SaveChangesAsync();

        return Ok(ToDto(annotation));
    }

    /// <summary>
    /// Get annotations for a lesson (paginated).
    /// </summary>
    [HttpGet("lessons/{lessonId:int}/annotations")]
    public async Task<ActionResult<AnnotationsPageDto>> GetAnnotations(
        int lessonId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.LessonAnnotations
            .AsNoTracking()
            .Where(a => a.LessonId == lessonId)
            .OrderByDescending(a => a.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(ToDto).ToList();
        return Ok(new AnnotationsPageDto(dtos, total));
    }

    /// <summary>
    /// Update own annotation.
    /// </summary>
    [HttpPut("annotations/{id:int}")]
    public async Task<IActionResult> UpdateAnnotation(int id, [FromBody] UpdateAnnotationRequest request)
    {
        var annotation = await _db.LessonAnnotations.FindAsync(id);
        if (annotation == null)
            return NotFound();

        if (annotation.UserId != _user.Id)
            return Forbid();

        if (request.Rating.HasValue && (request.Rating < 1 || request.Rating > 5))
            return BadRequest("Rating must be between 1 and 5.");

        annotation.Content = request.Content;
        annotation.Rating = request.Rating;
        annotation.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Delete own annotation, or any annotation if backoffice.
    /// </summary>
    [HttpDelete("annotations/{id:int}")]
    public async Task<IActionResult> DeleteAnnotation(int id)
    {
        var annotation = await _db.LessonAnnotations.FindAsync(id);
        if (annotation == null)
            return NotFound();

        if (annotation.UserId != _user.Id && !_user.IsBackOffice)
            return Forbid();

        _db.LessonAnnotations.Remove(annotation);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private AnnotationDto ToDto(LessonAnnotationEntity a) =>
        new(a.Id, a.DisplayName ?? "Learner", a.Content, a.Rating, a.CreatedAt, a.UpdatedAt, a.UserId == _user.Id);
}
