using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

public record ContentBlockRequest(string Type, string Content, int Order);

public record ReorderRequest(int[] OrderedIds);

[ApiController]
[Route("api/lessons/{lessonId:int}/content-blocks")]
[Authorize]
public class ContentBlockController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ISkillForgeUser _user;

    public ContentBlockController(AppDbContext db, ISkillForgeUser user)
    {
        _db = db;
        _user = user;
    }

    private bool CanManage => _user.IsBackOffice || _user.IsManager;

    [HttpGet]
    public async Task<ActionResult<List<ContentBlockEntity>>> GetContentBlocks(int lessonId)
    {
        var blocks = await _db.ContentBlocks
            .AsNoTracking()
            .Where(b => b.LessonId == lessonId)
            .OrderBy(b => b.Order)
            .ToListAsync();

        return Ok(blocks);
    }

    [HttpPost]
    public async Task<ActionResult<ContentBlockEntity>> AddContentBlock(int lessonId, [FromBody] ContentBlockRequest request)
    {
        if (!CanManage) return Forbid();

        var lessonExists = await _db.Lessons.AnyAsync(l => l.Id == lessonId);
        if (!lessonExists) return NotFound();

        var block = new ContentBlockEntity
        {
            LessonId = lessonId,
            Type = request.Type,
            Content = request.Content,
            Order = request.Order,
        };

        _db.ContentBlocks.Add(block);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetContentBlocks), new { lessonId }, block);
    }

    [HttpPut("{blockId:int}")]
    public async Task<ActionResult<ContentBlockEntity>> UpdateContentBlock(int lessonId, int blockId, [FromBody] ContentBlockRequest request)
    {
        if (!CanManage) return Forbid();

        var block = await _db.ContentBlocks.SingleOrDefaultAsync(b => b.Id == blockId && b.LessonId == lessonId);
        if (block is null) return NotFound();

        block.Type = request.Type;
        block.Content = request.Content;
        block.Order = request.Order;

        await _db.SaveChangesAsync();
        return Ok(block);
    }

    [HttpDelete("{blockId:int}")]
    public async Task<IActionResult> DeleteContentBlock(int lessonId, int blockId)
    {
        if (!CanManage) return Forbid();

        var block = await _db.ContentBlocks.SingleOrDefaultAsync(b => b.Id == blockId && b.LessonId == lessonId);
        if (block is null) return NotFound();

        _db.ContentBlocks.Remove(block);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> ReorderContentBlocks(int lessonId, [FromBody] ReorderRequest request)
    {
        if (!CanManage) return Forbid();

        var blocks = await _db.ContentBlocks
            .Where(b => b.LessonId == lessonId)
            .ToListAsync();

        for (var i = 0; i < request.OrderedIds.Length; i++)
        {
            var block = blocks.SingleOrDefault(b => b.Id == request.OrderedIds[i]);
            if (block is not null)
            {
                block.Order = i + 1;
            }
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
