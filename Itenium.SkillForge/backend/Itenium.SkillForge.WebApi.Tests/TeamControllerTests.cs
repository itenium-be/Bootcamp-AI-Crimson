using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class TeamControllerTests
{
    private AppDbContext _db = null!;
    private ISkillForgeUser _user = null!;
    private TeamController _sut = null!;

    [SetUp]
    public async Task Setup()
    {
        _db = new AppDbContext(PostgresFixture.CreateDbContextOptions());
        _user = Substitute.For<ISkillForgeUser>();
        _sut = new TeamController(_db, _user);

        _db.Teams.RemoveRange(_db.Teams);
        await _db.SaveChangesAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [Test]
    public async Task GetUserTeams_WhenBackOffice_ReturnsAllTeams()
    {
        _db.Teams.AddRange(
            new TeamEntity { Name = "Java" },
            new TeamEntity { Name = ".NET" },
            new TeamEntity { Name = "QA" }
        );
        await _db.SaveChangesAsync();
        _user.IsBackOffice.Returns(true);

        var result = await _sut.GetUserTeams();

        var teams = result.Value!;
        Assert.That(teams, Has.Count.EqualTo(3));
        Assert.That(teams.Select(t => t.Name), Contains.Item("Java"));
        Assert.That(teams.Select(t => t.Name), Contains.Item(".NET"));
        Assert.That(teams.Select(t => t.Name), Contains.Item("QA"));
    }

    [Test]
    public async Task GetUserTeams_WhenNotBackOffice_ReturnsOnlyUserTeams()
    {
        var javaTeam = new TeamEntity { Name = "Java" };
        var dotnetTeam = new TeamEntity { Name = ".NET" };
        var qaTeam = new TeamEntity { Name = "QA" };
        _db.Teams.AddRange(javaTeam, dotnetTeam, qaTeam);
        await _db.SaveChangesAsync();
        _user.IsBackOffice.Returns(false);
        _user.Teams.Returns(new[] { javaTeam.Id, qaTeam.Id });

        var result = await _sut.GetUserTeams();

        var teams = result.Value!;
        Assert.That(teams, Has.Count.EqualTo(2));
        Assert.That(teams.Select(t => t.Name), Contains.Item("Java"));
        Assert.That(teams.Select(t => t.Name), Contains.Item("QA"));
        Assert.That(teams.Select(t => t.Name), Does.Not.Contain(".NET"));
    }

    [Test]
    public async Task GetUserTeams_WhenNotBackOfficeAndNoTeams_ReturnsEmpty()
    {
        _db.Teams.AddRange(
            new TeamEntity { Name = "Java" },
            new TeamEntity { Name = ".NET" }
        );
        await _db.SaveChangesAsync();
        _user.IsBackOffice.Returns(false);
        _user.Teams.Returns(Array.Empty<int>());

        var result = await _sut.GetUserTeams();

        var teams = result.Value!;
        Assert.That(teams, Is.Empty);
    }

    [Test]
    public async Task GetUserTeams_WhenUserHasNonExistentTeamId_IgnoresIt()
    {
        var javaTeam = new TeamEntity { Name = "Java" };
        _db.Teams.Add(javaTeam);
        await _db.SaveChangesAsync();
        _user.IsBackOffice.Returns(false);
        _user.Teams.Returns(new[] { javaTeam.Id, 999 });

        var result = await _sut.GetUserTeams();

        var teams = result.Value!;
        Assert.That(teams, Has.Count.EqualTo(1));
        Assert.That(teams.Select(t => t.Name), Contains.Item("Java"));
    }
}
