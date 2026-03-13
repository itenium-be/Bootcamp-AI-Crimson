using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class PasswordResetControllerTests
{
    private UserManager<ForgeUser> _userManager = null!;
    private IEmailSender _emailSender = null!;
    private PasswordResetController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _userManager = Substitute.For<UserManager<ForgeUser>>(
            Substitute.For<IUserStore<ForgeUser>>(), null, null, null, null, null, null, null, null);
        _emailSender = Substitute.For<IEmailSender>();
        _sut = new PasswordResetController(_userManager, _emailSender);
    }

    [TearDown]
    public void TearDown()
    {
        _userManager.Dispose();
    }

    // --- Request reset ---

    [Test]
    public async Task RequestReset_WhenEmailNotFound_ReturnsOkWithoutSendingEmail()
    {
        _userManager.FindByEmailAsync("unknown@test.local").Returns((ForgeUser?)null);

        var result = await _sut.RequestReset(new PasswordResetRequest("unknown@test.local"));

        Assert.That(result, Is.InstanceOf<OkResult>());
        await _emailSender.DidNotReceive().SendPasswordResetEmailAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public async Task RequestReset_WhenEmailFound_ReturnsOkAndSendsEmail()
    {
        var user = new ForgeUser { Email = "user@test.local", PasswordHash = "hash" };
        _userManager.FindByEmailAsync("user@test.local").Returns(user);
        _userManager.GeneratePasswordResetTokenAsync(user).Returns("reset-token-123");

        var result = await _sut.RequestReset(new PasswordResetRequest("user@test.local"));

        Assert.That(result, Is.InstanceOf<OkResult>());
        await _emailSender.Received(1).SendPasswordResetEmailAsync("user@test.local", "reset-token-123");
    }

    [Test]
    public async Task RequestReset_WhenSsoUser_ReturnsOkWithoutSendingEmail()
    {
        // SSO users have no PasswordHash
        var user = new ForgeUser { Email = "sso@test.local", PasswordHash = null };
        _userManager.FindByEmailAsync("sso@test.local").Returns(user);

        var result = await _sut.RequestReset(new PasswordResetRequest("sso@test.local"));

        Assert.That(result, Is.InstanceOf<OkResult>());
        await _emailSender.DidNotReceive().SendPasswordResetEmailAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    // --- Confirm reset ---

    [Test]
    public async Task ConfirmReset_WhenEmailNotFound_ReturnsBadRequestWithInvalidToken()
    {
        _userManager.FindByEmailAsync("unknown@test.local").Returns((ForgeUser?)null);

        var result = await _sut.ConfirmReset(new PasswordResetConfirmRequest("unknown@test.local", "token", "NewPass123!"));

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var body = ((BadRequestObjectResult)result).Value as PasswordResetErrorResponse;
        Assert.That(body?.Error, Is.EqualTo("InvalidToken"));
    }

    [Test]
    public async Task ConfirmReset_WhenSsoUser_ReturnsBadRequestWithSsoError()
    {
        var user = new ForgeUser { Email = "sso@test.local", PasswordHash = null };
        _userManager.FindByEmailAsync("sso@test.local").Returns(user);

        var result = await _sut.ConfirmReset(new PasswordResetConfirmRequest("sso@test.local", "token", "NewPass123!"));

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var body = ((BadRequestObjectResult)result).Value as PasswordResetErrorResponse;
        Assert.That(body?.Error, Is.EqualTo("SsoUser"));
    }

    [Test]
    public async Task ConfirmReset_WhenTokenInvalid_ReturnsBadRequestWithInvalidToken()
    {
        var user = new ForgeUser { Email = "user@test.local", PasswordHash = "hash" };
        _userManager.FindByEmailAsync("user@test.local").Returns(user);
        _userManager.ResetPasswordAsync(user, "bad-token", "NewPass123!")
            .Returns(IdentityResult.Failed(new IdentityError { Code = "InvalidToken", Description = "Invalid token." }));

        var result = await _sut.ConfirmReset(new PasswordResetConfirmRequest("user@test.local", "bad-token", "NewPass123!"));

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var body = ((BadRequestObjectResult)result).Value as PasswordResetErrorResponse;
        Assert.That(body?.Error, Is.EqualTo("InvalidToken"));
    }

    [Test]
    public async Task ConfirmReset_WhenValid_ReturnsOk()
    {
        var user = new ForgeUser { Email = "user@test.local", PasswordHash = "hash" };
        _userManager.FindByEmailAsync("user@test.local").Returns(user);
        _userManager.ResetPasswordAsync(user, "valid-token", "NewPass123!")
            .Returns(IdentityResult.Success);

        var result = await _sut.ConfirmReset(new PasswordResetConfirmRequest("user@test.local", "valid-token", "NewPass123!"));

        Assert.That(result, Is.InstanceOf<OkResult>());
    }
}
