using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Services;
using Microsoft.AspNetCore.Identity;

namespace Itenium.SkillForge.WebApi;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ForgeUser> _userManager;

    public UserRepository(UserManager<ForgeUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IList<UserResponse>> GetAllUsersWithRolesAsync()
    {
        var users = _userManager.Users.ToList();
        var result = new List<UserResponse>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var isActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;

            result.Add(new UserResponse(
                user.Id,
                $"{user.FirstName} {user.LastName}".Trim(),
                user.Email ?? string.Empty,
                roles.FirstOrDefault() ?? "learner",
                isActive
            ));
        }

        return result;
    }
}
