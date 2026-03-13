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

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ISkillForgeUser _user;

    public TeamController(AppDbContext db, ISkillForgeUser user)
    {
        _db = db;
        _user = user;
    }

    /// <summary>
    /// Get the teams the current user has access to.
    /// </summary>
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

    /// <summary>
    /// Get members of a team. Managers can only access their own teams; backoffice can access all.
    /// </summary>
    [HttpGet("{id:int}/members")]
    public async Task<ActionResult<IList<TeamMemberResponse>>> GetTeamMembers(int id)
    {
        if (!_user.IsBackOffice && !_user.Teams.Contains(id))
        {
            return Forbid();
        }

        var teamExists = await _db.Teams.AnyAsync(t => t.Id == id);
        if (!teamExists)
        {
            return NotFound();
        }

        var idStr = id.ToString(CultureInfo.InvariantCulture);
        var members = await _db.Set<ForgeUser>()
            .Where(u => _db.Set<IdentityUserClaim<string>>()
                .Any(c => c.UserId == u.Id && c.ClaimType == "team" && c.ClaimValue == idStr))
            .Select(u => new TeamMemberResponse(
                (u.FirstName + " " + u.LastName).Trim(),
                u.Email ?? string.Empty,
                null))
            .ToListAsync();

        return Ok(members);
    }
}
