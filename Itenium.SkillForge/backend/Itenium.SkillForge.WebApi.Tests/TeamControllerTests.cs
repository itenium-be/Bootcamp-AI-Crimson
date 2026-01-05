using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class TeamControllerTests
{
    private AppDbContext _db = null!;
    private ISkillForgeUser _user = null!;
    private TeamController _sut = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _user = Substitute.For<ISkillForgeUser>();
        _sut = new TeamController(_db, _user);
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
            new TeamEntity { Id = 1, Name = "Java" },
            new TeamEntity { Id = 2, Name = ".NET" },
            new TeamEntity { Id = 3, Name = "QA" }
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
        _db.Teams.AddRange(
            new TeamEntity { Id = 1, Name = "Java" },
            new TeamEntity { Id = 2, Name = ".NET" },
            new TeamEntity { Id = 3, Name = "QA" }
        );
        await _db.SaveChangesAsync();
        _user.IsBackOffice.Returns(false);
        _user.Teams.Returns(new[] { 1, 3 });

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
            new TeamEntity { Id = 1, Name = "Java" },
            new TeamEntity { Id = 2, Name = ".NET" }
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
        _db.Teams.Add(new TeamEntity { Id = 1, Name = "Java" });
        await _db.SaveChangesAsync();
        _user.IsBackOffice.Returns(false);
        _user.Teams.Returns(new[] { 1, 999 });

        var result = await _sut.GetUserTeams();

        var teams = result.Value!;
        Assert.That(teams, Has.Count.EqualTo(1));
        Assert.That(teams.Select(t => t.Name), Contains.Item("Java"));
    }
}
