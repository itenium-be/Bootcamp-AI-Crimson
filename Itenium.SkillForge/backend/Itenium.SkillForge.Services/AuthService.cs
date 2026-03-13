using Itenium.Forge.Security.OpenIddict;
using Microsoft.AspNetCore.Identity;

namespace Itenium.SkillForge.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ForgeUser> _userManager;
    private readonly SignInManager<ForgeUser> _signInManager;

    public AuthService(UserManager<ForgeUser> userManager, SignInManager<ForgeUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<string?> GetUsernameAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return null;

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        return result.Succeeded ? user.UserName : null;
    }
}
