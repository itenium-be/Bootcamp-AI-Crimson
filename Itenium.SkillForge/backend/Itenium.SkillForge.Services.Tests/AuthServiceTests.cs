using Itenium.Forge.Security.OpenIddict;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Itenium.SkillForge.Services.Tests;

[TestFixture]
public class AuthServiceTests
{
    private UserManager<ForgeUser> _userManager = null!;
    private SignInManager<ForgeUser> _signInManager = null!;
    private AuthService _sut = null!;

    [TearDown]
    public void TearDown()
    {
        _userManager?.Dispose();
    }

    [SetUp]
    public void Setup()
    {
        var userStore = Substitute.For<IUserStore<ForgeUser>>();
        _userManager = Substitute.For<UserManager<ForgeUser>>(
            userStore, null, null, null, null, null, null, null, null);

        _signInManager = Substitute.For<SignInManager<ForgeUser>>(
            _userManager,
            Substitute.For<IHttpContextAccessor>(),
            Substitute.For<IUserClaimsPrincipalFactory<ForgeUser>>(),
            null, null, null, null);

        _sut = new AuthService(_userManager, _signInManager);
    }

    [Test]
    public async Task GetUsernameAsync_WhenUserNotFound_ReturnsNull()
    {
        _userManager.FindByEmailAsync(Arg.Any<string>())
            .Returns(Task.FromResult<ForgeUser?>(null));

        var result = await _sut.GetUsernameAsync("notexist@test.local", "password");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetUsernameAsync_WhenPasswordInvalid_ReturnsNull()
    {
        var user = new ForgeUser { UserName = "testuser" };
        _userManager.FindByEmailAsync("user@test.local")
            .Returns(Task.FromResult<ForgeUser?>(user));
        _signInManager.CheckPasswordSignInAsync(user, "wrongpassword", false)
            .Returns(SignInResult.Failed);

        var result = await _sut.GetUsernameAsync("user@test.local", "wrongpassword");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetUsernameAsync_WhenCredentialsValid_ReturnsUsername()
    {
        var user = new ForgeUser { UserName = "testuser" };
        _userManager.FindByEmailAsync("user@test.local")
            .Returns(Task.FromResult<ForgeUser?>(user));
        _signInManager.CheckPasswordSignInAsync(user, "ValidPassword123!", false)
            .Returns(SignInResult.Success);

        var result = await _sut.GetUsernameAsync("user@test.local", "ValidPassword123!");

        Assert.That(result, Is.EqualTo("testuser"));
    }
}
