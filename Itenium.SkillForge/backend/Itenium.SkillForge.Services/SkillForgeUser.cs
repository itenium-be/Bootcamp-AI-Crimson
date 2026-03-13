using System.Globalization;
using Itenium.Forge.Security;
using Microsoft.AspNetCore.Http;

namespace Itenium.SkillForge.Services;

public class SkillForgeUser : CurrentUser, ISkillForgeUser
{
    public SkillForgeUser(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }

    public string? Id => User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    public bool IsBackOffice => User?.IsInRole("backoffice") ?? false;

    public bool IsManager => (User?.IsInRole("team_manager") ?? false) || IsBackOffice;

    public string? DisplayName =>
        User?.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value
        ?? User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
        ?? User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value?.Split('@')[0];

    public ICollection<int> Teams
    {
        get
        {
            if (User == null)
            {
                return [];
            }

            var teams = User.FindAll("team").Select(c => int.Parse(c.Value, CultureInfo.InvariantCulture)).ToArray();
            return teams;
        }
    }
}
