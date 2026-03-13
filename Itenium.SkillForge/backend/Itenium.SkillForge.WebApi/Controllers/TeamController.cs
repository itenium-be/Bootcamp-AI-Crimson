using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

public record AddTeamMemberRequest(string UserId);

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

    /// <summary>Get members of a team. BackOffice only.</summary>
    [HttpGet("{id}/members")]
    public async Task<ActionResult<IList<UserResponse>>> GetTeamMembers(int id)
    {
        if (!_user.IsBackOffice) return Forbid();
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
}
