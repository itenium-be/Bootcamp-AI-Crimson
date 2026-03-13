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
}
