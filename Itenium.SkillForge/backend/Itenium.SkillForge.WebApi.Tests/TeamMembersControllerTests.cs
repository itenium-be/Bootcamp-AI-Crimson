using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class TeamMembersControllerTests : DatabaseTestBase
{
    private IUserRepository _userRepository = null!;
    private ISkillForgeUser _user = null!;
    private TeamController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _user = Substitute.For<ISkillForgeUser>();
        _sut = new TeamController(Db, _user, _userRepository);
    }

    [Test]
    public async Task GetTeamMembers_WhenNotBackoffice_ReturnsForbid()
    {
        _user.IsBackOffice.Returns(false);

        var result = await _sut.GetTeamMembers(1);

        Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task GetTeamMembers_WhenBackoffice_ReturnsMembers()
    {
        _user.IsBackOffice.Returns(true);
        _userRepository.GetTeamMembersAsync(1).Returns(new List<UserResponse>
        {
            new("u1", "Alice Smith", "alice@test.com", "learner", true),
            new("u2", "Bob Jones", "bob@test.com", "learner", true),
        });

        var result = await _sut.GetTeamMembers(1);

        var ok = result.Result as OkObjectResult;
        var members = ok!.Value as IList<UserResponse>;
        Assert.That(members, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetAvailableLearners_WhenNotBackoffice_ReturnsForbid()
    {
        _user.IsBackOffice.Returns(false);

        var result = await _sut.GetAvailableLearners(1);

        Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task GetAvailableLearners_WhenBackoffice_ReturnsActiveLearners()
    {
        _user.IsBackOffice.Returns(true);
        _userRepository.GetActiveLearnersAsync().Returns(new List<UserResponse>
        {
            new("u1", "Alice", "alice@test.com", "learner", true),
        });

        var result = await _sut.GetAvailableLearners(1);

        var ok = result.Result as OkObjectResult;
        var learners = ok!.Value as IList<UserResponse>;
        Assert.That(learners, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task AddTeamMember_WhenNotBackoffice_ReturnsForbid()
    {
        _user.IsBackOffice.Returns(false);

        var result = await _sut.AddTeamMember(1, new AddTeamMemberRequest("u1"));

        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task AddTeamMember_WhenUserNotFound_ReturnsNotFound()
    {
        _user.IsBackOffice.Returns(true);
        _userRepository.AddTeamMemberAsync(1, "u999").Returns(false);

        var result = await _sut.AddTeamMember(1, new AddTeamMemberRequest("u999"));

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task AddTeamMember_WhenFound_ReturnsNoContent()
    {
        _user.IsBackOffice.Returns(true);
        _userRepository.AddTeamMemberAsync(1, "u1").Returns(true);

        var result = await _sut.AddTeamMember(1, new AddTeamMemberRequest("u1"));

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task RemoveTeamMember_WhenNotBackoffice_ReturnsForbid()
    {
        _user.IsBackOffice.Returns(false);

        var result = await _sut.RemoveTeamMember(1, "u1");

        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task RemoveTeamMember_WhenFound_ReturnsNoContent()
    {
        _user.IsBackOffice.Returns(true);
        _userRepository.RemoveTeamMemberAsync(1, "u1").Returns(true);

        var result = await _sut.RemoveTeamMember(1, "u1");

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task RemoveTeamMember_WhenNotFound_ReturnsNotFound()
    {
        _user.IsBackOffice.Returns(true);
        _userRepository.RemoveTeamMemberAsync(1, "u999").Returns(false);

        var result = await _sut.RemoveTeamMember(1, "u999");

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
}
