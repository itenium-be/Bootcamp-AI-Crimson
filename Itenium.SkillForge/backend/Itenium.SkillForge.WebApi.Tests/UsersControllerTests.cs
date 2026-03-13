using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class UsersControllerTests
{
    private IUserRepository _userRepository = null!;
    private ISkillForgeUser _user = null!;
    private UsersController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _user = Substitute.For<ISkillForgeUser>();
        _sut = new UsersController(_userRepository, _user);
    }

    [Test]
    public async Task GetUsers_WhenNotBackoffice_ReturnsForbid()
    {
        _user.IsBackOffice.Returns(false);

        var result = await _sut.GetUsers();

        Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task GetUsers_WhenBackoffice_ReturnsAllUsers()
    {
        _user.IsBackOffice.Returns(true);
        _userRepository.GetAllUsersWithRolesAsync().Returns(new List<UserResponse>
        {
            new("1", "Alice Smith", "alice@test.com", "learner", true),
            new("2", "Bob Jones", "bob@test.com", "team_manager", true),
            new("3", "Carol Admin", "carol@test.com", "backoffice", true),
        });

        var result = await _sut.GetUsers();

        var ok = result.Result as OkObjectResult;
        var users = ok!.Value as IList<UserResponse>;
        Assert.That(users, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task GetUsers_WhenBackoffice_ReturnsUserWithCorrectFields()
    {
        _user.IsBackOffice.Returns(true);
        _userRepository.GetAllUsersWithRolesAsync().Returns(new List<UserResponse>
        {
            new("1", "Alice Smith", "alice@test.com", "learner", true),
        });

        var result = await _sut.GetUsers();

        var ok = result.Result as OkObjectResult;
        var users = ok!.Value as IList<UserResponse>;
        var user = users![0];
        Assert.That(user.Name, Is.EqualTo("Alice Smith"));
        Assert.That(user.Email, Is.EqualTo("alice@test.com"));
        Assert.That(user.Role, Is.EqualTo("learner"));
        Assert.That(user.IsActive, Is.True);
    }

    [Test]
    public async Task GetUsers_WhenBackoffice_ReturnsEmptyList()
    {
        _user.IsBackOffice.Returns(true);
        _userRepository.GetAllUsersWithRolesAsync().Returns(new List<UserResponse>());

        var result = await _sut.GetUsers();

        var ok = result.Result as OkObjectResult;
        var users = ok!.Value as IList<UserResponse>;
        Assert.That(users, Is.Empty);
    }

    // --- GetUser by ID ---

    [Test]
    public async Task GetUser_WhenNotBackoffice_ReturnsForbid()
    {
        _user.IsBackOffice.Returns(false);

        var result = await _sut.GetUser("1");

        Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task GetUser_WhenNotFound_ReturnsNotFound()
    {
        _user.IsBackOffice.Returns(true);
        _userRepository.GetUserByIdAsync("999").Returns((UserResponse?)null);

        var result = await _sut.GetUser("999");

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task GetUser_WhenFound_ReturnsUser()
    {
        _user.IsBackOffice.Returns(true);
        _userRepository.GetUserByIdAsync("1").Returns(new UserResponse("1", "Alice Smith", "alice@test.com", "learner", true));

        var result = await _sut.GetUser("1");

        var ok = result.Result as OkObjectResult;
        var user = ok!.Value as UserResponse;
        Assert.That(user!.Name, Is.EqualTo("Alice Smith"));
    }

    // --- ChangeRole ---

    [Test]
    public async Task ChangeRole_WhenNotBackoffice_ReturnsForbid()
    {
        _user.IsBackOffice.Returns(false);

        var result = await _sut.ChangeRole("1", new ChangeRoleRequest("team_manager"));

        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task ChangeRole_WhenUserNotFound_ReturnsNotFound()
    {
        _user.IsBackOffice.Returns(true);
        _userRepository.ChangeRoleAsync("999", "learner").Returns(false);

        var result = await _sut.ChangeRole("999", new ChangeRoleRequest("learner"));

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task ChangeRole_WhenFound_ReturnsNoContent()
    {
        _user.IsBackOffice.Returns(true);
        _userRepository.ChangeRoleAsync("1", "team_manager").Returns(true);

        var result = await _sut.ChangeRole("1", new ChangeRoleRequest("team_manager"));

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    // --- DeactivateUser ---

    [Test]
    public async Task DeactivateUser_WhenNotBackoffice_ReturnsForbid()
    {
        _user.IsBackOffice.Returns(false);

        var result = await _sut.DeactivateUser("1");

        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task DeactivateUser_WhenUserNotFound_ReturnsNotFound()
    {
        _user.IsBackOffice.Returns(true);
        _userRepository.DeactivateAsync("999").Returns(false);

        var result = await _sut.DeactivateUser("999");

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task DeactivateUser_WhenFound_ReturnsNoContent()
    {
        _user.IsBackOffice.Returns(true);
        _userRepository.DeactivateAsync("1").Returns(true);

        var result = await _sut.DeactivateUser("1");

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    // --- ActivateUser ---

    [Test]
    public async Task ActivateUser_WhenNotBackoffice_ReturnsForbid()
    {
        _user.IsBackOffice.Returns(false);

        var result = await _sut.ActivateUser("1");

        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task ActivateUser_WhenFound_ReturnsNoContent()
    {
        _user.IsBackOffice.Returns(true);
        _userRepository.ActivateAsync("1").Returns(true);

        var result = await _sut.ActivateUser("1");

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }
}
