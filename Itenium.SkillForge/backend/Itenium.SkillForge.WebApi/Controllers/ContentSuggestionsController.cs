using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

public record SubmitContentSuggestionRequest(
    string Title,
    string? Description,
    string? Url,
    int? RelatedCourseId,
    string? Topic
);

public record ContentSuggestionDto(
    int Id,
    string Title,
    string? Description,
    string? Url,
    int? RelatedCourseId,
    string? Topic,
    ContentSuggestionStatus Status,
    string? ReviewNote,
    DateTime SubmittedAt
);

public record ReviewRequest(string? Note);

public record ContentSuggestionResponse(
    int Id,
    string SubmittedBy,
    string? SubmitterName,
    int? TeamId,
    string Title,
    string? Description,
    string? Url,
    int? RelatedCourseId,
    string? Topic,
    string Status,
    string? ReviewedBy,
    DateTime? ReviewedAt,
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

    /// <summary>
    /// Returns content suggestions for a team, optionally filtered by status.
    /// Team manager can only see suggestions from their own teams.
    /// </summary>
    [HttpGet("content-suggestions")]
    public async Task<ActionResult<IList<ContentSuggestionResponse>>> GetSuggestions(
        [FromQuery] int teamId,
        [FromQuery] string? status = null)
    {
        if (!_user.IsBackOffice && !_user.Teams.Contains(teamId))
            return Forbid();

        var query = _db.ContentSuggestions
            .AsNoTracking()
            .Where(s => s.TeamId == teamId);

        if (!string.IsNullOrEmpty(status))
        {
            if (TryParseStatus(status, out var statusValue))
                query = query.Where(s => s.Status == statusValue);
        }

        var suggestions = await query
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();

        return Ok(suggestions.Select(ToResponse).ToList());
    }

    /// <summary>
    /// Approve a content suggestion.
    /// </summary>
    [HttpPut("content-suggestions/{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ReviewRequest request)
    {
        var suggestion = await _db.ContentSuggestions.FindAsync(id);
        if (suggestion == null) return NotFound();

        if (!_user.IsBackOffice && (!suggestion.TeamId.HasValue || !_user.Teams.Contains(suggestion.TeamId.Value)))
            return Forbid();

        suggestion.Status = ContentSuggestionStatus.Approved;
        suggestion.ReviewedBy = _user.Id;
        suggestion.ReviewedAt = DateTime.UtcNow;
        suggestion.ReviewNote = request.Note;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Reject a content suggestion.
    /// </summary>
    [HttpPut("content-suggestions/{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ReviewRequest request)
    {
        var suggestion = await _db.ContentSuggestions.FindAsync(id);
        if (suggestion == null) return NotFound();

        if (!_user.IsBackOffice && (!suggestion.TeamId.HasValue || !_user.Teams.Contains(suggestion.TeamId.Value)))
            return Forbid();

        suggestion.Status = ContentSuggestionStatus.Rejected;
        suggestion.ReviewedBy = _user.Id;
        suggestion.ReviewedAt = DateTime.UtcNow;
        suggestion.ReviewNote = request.Note;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static bool TryParseStatus(string status, out ContentSuggestionStatus value)
    {
        value = default;
        return status.ToLowerInvariant() switch
        {
            "pending" => (value = ContentSuggestionStatus.Pending) == value,
            "approved" => (value = ContentSuggestionStatus.Approved) == value,
            "rejected" => (value = ContentSuggestionStatus.Rejected) == value,
            _ => false,
        };
    }

    private static ContentSuggestionDto ToDto(ContentSuggestionEntity e) =>
        new(e.Id, e.Title, e.Description, e.Url, e.RelatedCourseId, e.Topic, e.Status, e.ReviewNote, e.SubmittedAt);

    private static ContentSuggestionResponse ToResponse(ContentSuggestionEntity s) => new(
        s.Id,
        s.SubmittedBy,
        s.SubmitterName,
        s.TeamId,
        s.Title,
        s.Description,
        s.Url,
        s.RelatedCourseId,
        s.Topic,
        s.Status.ToString().ToLowerInvariant(),
        s.ReviewedBy,
        s.ReviewedAt,
        s.ReviewNote,
        s.SubmittedAt
    );
}
