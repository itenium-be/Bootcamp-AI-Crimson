using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class ContentSuggestionsControllerTests : DatabaseTestBase
{
    private ISkillForgeUser _user = null!;
    private ContentSuggestionsController _sut = null!;

    private const int TeamId = 42;
    private const string ManagerId = "manager-1";

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<ISkillForgeUser>();
        _user.Id.Returns(ManagerId);
        _user.Teams.Returns(new List<int> { TeamId });
        _sut = new ContentSuggestionsController(Db, _user);
    }

    private async Task<ContentSuggestionEntity> SeedSuggestion(
        int? teamId = TeamId,
        ContentSuggestionStatus status = ContentSuggestionStatus.Pending,
        string submittedBy = "learner-1")
    {
        var suggestion = new ContentSuggestionEntity
        {
            SubmittedBy = submittedBy,
            SubmitterName = "Learner One",
            TeamId = teamId,
            Title = "Great Resource",
            Description = "A very useful link",
            Url = "https://example.com",
            Status = status,
        };
        Db.ContentSuggestions.Add(suggestion);
        await Db.SaveChangesAsync();
        return suggestion;
    }

    // ── GET ──────────────────────────────────────────────────────────────

    [Test]
    public async Task GetSuggestions_WhenManagerOfTeam_ReturnsPendingSuggestions()
    {
        await SeedSuggestion(teamId: TeamId, status: ContentSuggestionStatus.Pending);

        var result = await _sut.GetSuggestions(TeamId, "pending");

        var ok = result.Result as OkObjectResult;
        var list = ok!.Value as IList<ContentSuggestionResponse>;
        Assert.That(list, Has.Count.EqualTo(1));
        Assert.That(list![0].Title, Is.EqualTo("Great Resource"));
    }

    [Test]
    public async Task GetSuggestions_WhenManagerOfDifferentTeam_ReturnsForbid()
    {
        _user.Teams.Returns(new List<int> { 99 });

        var result = await _sut.GetSuggestions(TeamId, "pending");

        Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task GetSuggestions_FiltersByStatus_ReturnsOnlyMatchingStatus()
    {
        await SeedSuggestion(status: ContentSuggestionStatus.Pending);
        await SeedSuggestion(status: ContentSuggestionStatus.Approved);
        await SeedSuggestion(status: ContentSuggestionStatus.Rejected);

        var result = await _sut.GetSuggestions(TeamId, "pending");

        var ok = result.Result as OkObjectResult;
        var list = ok!.Value as IList<ContentSuggestionResponse>;
        Assert.That(list, Has.Count.EqualTo(1));
        Assert.That(list![0].Status, Is.EqualTo("pending"));
    }

    [Test]
    public async Task GetSuggestions_WithNoStatusFilter_ReturnsAllStatuses()
    {
        await SeedSuggestion(status: ContentSuggestionStatus.Pending);
        await SeedSuggestion(status: ContentSuggestionStatus.Approved);

        var result = await _sut.GetSuggestions(TeamId, null);

        var ok = result.Result as OkObjectResult;
        var list = ok!.Value as IList<ContentSuggestionResponse>;
        Assert.That(list, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetSuggestions_BackOffice_CanSeeAllTeams()
    {
        _user.IsBackOffice.Returns(true);
        _user.Teams.Returns(new List<int>());
        await SeedSuggestion(teamId: TeamId);

        var result = await _sut.GetSuggestions(TeamId, "pending");

        var ok = result.Result as OkObjectResult;
        var list = ok!.Value as IList<ContentSuggestionResponse>;
        Assert.That(list, Has.Count.EqualTo(1));
    }

    // ── APPROVE ──────────────────────────────────────────────────────────

    [Test]
    public async Task Approve_WhenSuggestionExists_UpdatesStatusToApproved()
    {
        var suggestion = await SeedSuggestion();

        var result = await _sut.Approve(suggestion.Id, new ReviewRequest(null));

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var updated = await Db.ContentSuggestions.FindAsync(suggestion.Id);
        Assert.That(updated!.Status, Is.EqualTo(ContentSuggestionStatus.Approved));
        Assert.That(updated.ReviewedBy, Is.EqualTo(ManagerId));
        Assert.That(updated.ReviewedAt, Is.Not.Null);
    }

    [Test]
    public async Task Approve_WithNote_StoresNote()
    {
        var suggestion = await SeedSuggestion();

        await _sut.Approve(suggestion.Id, new ReviewRequest("Well done!"));

        var updated = await Db.ContentSuggestions.FindAsync(suggestion.Id);
        Assert.That(updated!.ReviewNote, Is.EqualTo("Well done!"));
    }

    [Test]
    public async Task Approve_WhenNotFound_ReturnsNotFound()
    {
        var result = await _sut.Approve(999, new ReviewRequest(null));

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Approve_WhenManagerOfDifferentTeam_ReturnsForbid()
    {
        _user.Teams.Returns(new List<int> { 99 });
        var suggestion = await SeedSuggestion(teamId: TeamId);

        var result = await _sut.Approve(suggestion.Id, new ReviewRequest(null));

        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }

    // ── REJECT ───────────────────────────────────────────────────────────

    [Test]
    public async Task Reject_WhenSuggestionExists_UpdatesStatusToRejected()
    {
        var suggestion = await SeedSuggestion();

        var result = await _sut.Reject(suggestion.Id, new ReviewRequest("Not relevant"));

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var updated = await Db.ContentSuggestions.FindAsync(suggestion.Id);
        Assert.That(updated!.Status, Is.EqualTo(ContentSuggestionStatus.Rejected));
        Assert.That(updated.ReviewNote, Is.EqualTo("Not relevant"));
    }

    [Test]
    public async Task Reject_WhenNotFound_ReturnsNotFound()
    {
        var result = await _sut.Reject(999, new ReviewRequest(null));

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Reject_WhenManagerOfDifferentTeam_ReturnsForbid()
    {
        _user.Teams.Returns(new List<int> { 99 });
        var suggestion = await SeedSuggestion(teamId: TeamId);

        var result = await _sut.Reject(suggestion.Id, new ReviewRequest(null));

        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }
}
