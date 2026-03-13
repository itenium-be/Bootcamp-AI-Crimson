using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

public record SubmitContentSuggestionRequest(
    string Title,
    string Description,
    string? Url,
    int? RelatedCourseId,
    string? Topic
);

public record ContentSuggestionDto(
    int Id,
    string Title,
    string Description,
    string? Url,
    int? RelatedCourseId,
    string? Topic,
    ContentSuggestionStatus Status,
    string? ReviewNote,
    DateTime SubmittedAt
);

[ApiController]
[Route("api")]
[Authorize]
public class ContentSuggestionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ISkillForgeUser _user;

    public ContentSuggestionsController(AppDbContext db, ISkillForgeUser user)
    {
        _db = db;
        _user = user;
    }

    [HttpPost("content-suggestions")]
    public async Task<ActionResult<ContentSuggestionDto>> Submit([FromBody] SubmitContentSuggestionRequest request)
    {
        var entity = new ContentSuggestionEntity
        {
            SubmittedBy = _user.UserId!,
            Title = request.Title,
            Description = request.Description,
            Url = request.Url,
            RelatedCourseId = request.RelatedCourseId,
            Topic = request.Topic,
            SubmittedAt = DateTime.UtcNow,
        };

        _db.ContentSuggestions.Add(entity);
        await _db.SaveChangesAsync();

        var dto = ToDto(entity);
        return CreatedAtAction(nameof(GetMySuggestions), dto);
    }

    [HttpGet("learners/me/content-suggestions")]
    public async Task<ActionResult<IList<ContentSuggestionDto>>> GetMySuggestions()
    {
        var suggestions = await _db.ContentSuggestions
            .AsNoTracking()
            .Where(s => s.SubmittedBy == _user.UserId)
            .OrderByDescending(s => s.SubmittedAt)
            .Select(s => ToDto(s))
            .ToListAsync();

        return Ok(suggestions);
    }

    private static ContentSuggestionDto ToDto(ContentSuggestionEntity e) =>
        new(e.Id, e.Title, e.Description, e.Url, e.RelatedCourseId, e.Topic, e.Status, e.ReviewNote, e.SubmittedAt);
}
