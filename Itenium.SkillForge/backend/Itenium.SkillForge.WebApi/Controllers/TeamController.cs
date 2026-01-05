using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<ActionResult<UserTeamsResponse>> GetUserTeams()
    {
        List<TeamEntity> teams;

        if (_user.IsBackOffice)
        {
            teams = await _db.Teams.ToListAsync();
        }
        else
        {
            var teamIds = _user.Teams.ToList();
            teams = await _db.Teams
                .Where(t => teamIds.Contains(t.Id))
                .ToListAsync();
        }

        return Ok(new UserTeamsResponse(_user.IsBackOffice, teams));
    }
}

public record UserTeamsResponse(bool BackOffice, List<TeamEntity> Teams);
