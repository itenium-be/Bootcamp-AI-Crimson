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

    public async Task<UserResponse?> GetUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        var isActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;

        return new UserResponse(
            user.Id,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Email ?? string.Empty,
            roles.FirstOrDefault() ?? "learner",
            isActive,
            user.LockoutEnd
        );
    }

    public async Task<bool> ChangeRoleAsync(string id, string role)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return false;

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

        await _userManager.AddToRoleAsync(user, role);
        return true;
    }

    public async Task<bool> DeactivateAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return false;

        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        return true;
    }

    public async Task<bool> ActivateAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return false;

        await _userManager.SetLockoutEndDateAsync(user, null);
        return true;
    }
}
