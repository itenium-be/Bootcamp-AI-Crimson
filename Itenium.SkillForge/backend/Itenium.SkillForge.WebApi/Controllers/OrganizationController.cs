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
public class OrganizationController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ISkillForgeUser _user;

    public OrganizationController(AppDbContext db, ISkillForgeUser user)
    {
        _db = db;
        _user = user;
    }

    /// <summary>
    /// Get the organizations the current user has access to.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<UserOrganizationsResponse>> GetUserOrganizations()
    {
        List<OrganizationEntity> organizations;

        if (_user.IsCentral)
        {
            organizations = await _db.Organizations.ToListAsync();
        }
        else
        {
            var organizationIds = _user.Organizations.ToList();
            organizations = await _db.Organizations
                .Where(o => organizationIds.Contains(o.Id))
                .ToListAsync();
        }

        return Ok(new UserOrganizationsResponse(_user.IsCentral, organizations));
    }
}

public record UserOrganizationsResponse(bool Central, List<OrganizationEntity> Organizations);
