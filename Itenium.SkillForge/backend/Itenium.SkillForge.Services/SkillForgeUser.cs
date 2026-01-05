using Itenium.Forge.Security;
using Microsoft.AspNetCore.Http;

namespace Itenium.SkillForge.Services;

public class SkillForgeUser : CurrentUser, ISkillForgeUser
{
    public SkillForgeUser(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
    }

    public bool IsCentral => User?.IsInRole("central") ?? false;

    public IEnumerable<int> Organizations
    {
        get
        {
            if (User == null)
                return [];

            var organizations = User.FindAll("organization").Select(c => int.Parse(c.Value)).ToArray();
            return organizations;
        }
    }
}
