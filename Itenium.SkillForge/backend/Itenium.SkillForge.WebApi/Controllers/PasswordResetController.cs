using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Itenium.SkillForge.WebApi.Controllers;

public record PasswordResetErrorResponse(string Error);

[ApiController]
[Route("api/auth/password-reset")]
[AllowAnonymous]
public class PasswordResetController : ControllerBase
{
    private readonly UserManager<ForgeUser> _userManager;
    private readonly IEmailSender _emailSender;

    public PasswordResetController(UserManager<ForgeUser> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    /// <summary>
    /// Request a password reset email. Always returns 200 to avoid user enumeration.
    /// SSO users (no local password) are silently skipped.
    /// </summary>
    [HttpPost("request")]
    public async Task<IActionResult> RequestReset([FromBody] PasswordResetRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user != null && user.PasswordHash != null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailSender.SendPasswordResetEmailAsync(user.Email!, token);
        }

        return Ok();
    }

    /// <summary>
    /// Confirm a password reset using the token from the email.
    /// Returns 400 with error "InvalidToken" if token is invalid/expired, "SsoUser" if SSO account.
    /// </summary>
    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmReset([FromBody] PasswordResetConfirmRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return BadRequest(new PasswordResetErrorResponse("InvalidToken"));
        }

        if (user.PasswordHash == null)
        {
            return BadRequest(new PasswordResetErrorResponse("SsoUser"));
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault()?.Code ?? "InvalidToken";
            return BadRequest(new PasswordResetErrorResponse(error));
        }

        return Ok();
    }
}
