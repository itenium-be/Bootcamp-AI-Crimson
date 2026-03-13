using System.Globalization;
using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class TeamMembersControllerTests : DatabaseTestBase
{
    private ISkillForgeUser _user = null!;
    private TeamController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<ISkillForgeUser>();
        _sut = new TeamController(Db, _user);
    }

    [Test]
    public async Task GetTeamMembers_WhenTeamNotFound_ReturnsNotFound()
    {
        _user.IsBackOffice.Returns(true);

        var result = await _sut.GetTeamMembers(999);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task GetTeamMembers_WhenManagerHasNoAccess_ReturnsForbid()
    {
        var team = new TeamEntity { Name = "Java" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        _user.IsBackOffice.Returns(false);
        _user.Teams.Returns(Array.Empty<int>());

        var result = await _sut.GetTeamMembers(team.Id);

        Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task GetTeamMembers_WhenTeamHasNoMembers_ReturnsEmpty()
    {
        var team = new TeamEntity { Name = "Java" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        _user.IsBackOffice.Returns(true);

        var members = GetMembers(await _sut.GetTeamMembers(team.Id));

        Assert.That(members, Is.Empty);
    }

    [Test]
    public async Task GetTeamMembers_WhenTeamHasMembers_ReturnsMembersWithNameAndEmail()
    {
        var team = new TeamEntity { Name = "Java" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        await AddUserToTeamAsync("Alice", "Smith", "alice@test.local", team.Id);
        await AddUserToTeamAsync("Bob", "Jones", "bob@test.local", team.Id);

        _user.IsBackOffice.Returns(true);

        var members = GetMembers(await _sut.GetTeamMembers(team.Id));

        Assert.That(members, Has.Count.EqualTo(2));
        Assert.That(members.Select(m => m.Email), Contains.Item("alice@test.local"));
        Assert.That(members.Select(m => m.Email), Contains.Item("bob@test.local"));
    }

    [Test]
    public async Task GetTeamMembers_WhenManagerHasAccess_ReturnsMembers()
    {
        var team = new TeamEntity { Name = "Java" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        await AddUserToTeamAsync("Alice", "Smith", "alice@test.local", team.Id);

        _user.IsBackOffice.Returns(false);
        _user.Teams.Returns(new[] { team.Id });

        var members = GetMembers(await _sut.GetTeamMembers(team.Id));

        Assert.That(members, Has.Count.EqualTo(1));
        Assert.That(members[0].Name, Is.EqualTo("Alice Smith"));
        Assert.That(members[0].Email, Is.EqualTo("alice@test.local"));
    }

    [Test]
    public async Task GetTeamMembers_DoesNotReturnMembersFromOtherTeams()
    {
        var javaTeam = new TeamEntity { Name = "Java" };
        var dotnetTeam = new TeamEntity { Name = ".NET" };
        Db.Teams.AddRange(javaTeam, dotnetTeam);
        await Db.SaveChangesAsync();

        await AddUserToTeamAsync("Alice", "Smith", "alice@test.local", javaTeam.Id);
        await AddUserToTeamAsync("Bob", "Jones", "bob@test.local", dotnetTeam.Id);

        _user.IsBackOffice.Returns(true);

        var members = GetMembers(await _sut.GetTeamMembers(javaTeam.Id));

        Assert.That(members, Has.Count.EqualTo(1));
        Assert.That(members[0].Email, Is.EqualTo("alice@test.local"));
    }

    [Test]
    public async Task GetTeamMembers_BackOfficeCanAccessAnyTeam()
    {
        var team = new TeamEntity { Name = "Java" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        await AddUserToTeamAsync("Alice", "Smith", "alice@test.local", team.Id);

        _user.IsBackOffice.Returns(true);
        _user.Teams.Returns(Array.Empty<int>());

        var members = GetMembers(await _sut.GetTeamMembers(team.Id));

        Assert.That(members, Has.Count.EqualTo(1));
    }

    private static IList<TeamMemberResponse> GetMembers(ActionResult<IList<TeamMemberResponse>> result)
    {
        return (IList<TeamMemberResponse>)((OkObjectResult)result.Result!).Value!;
    }

    private async Task AddUserToTeamAsync(string firstName, string lastName, string email, int teamId)
    {
        var user = new ForgeUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            SecurityStamp = Guid.NewGuid().ToString(),
        };
        Db.Set<ForgeUser>().Add(user);
        Db.Set<IdentityUserClaim<string>>().Add(new IdentityUserClaim<string>
        {
            UserId = user.Id,
            ClaimType = "team",
            ClaimValue = teamId.ToString(CultureInfo.InvariantCulture),
        });
        await Db.SaveChangesAsync();
    }
}
