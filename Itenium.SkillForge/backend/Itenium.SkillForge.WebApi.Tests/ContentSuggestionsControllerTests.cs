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

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<ISkillForgeUser>();
        _user.UserId.Returns("user-1");
        _sut = new ContentSuggestionsController(Db, _user);
    }

    // --- POST /api/content-suggestions ---

    [Test]
    public async Task Submit_WithValidData_ReturnsCreated()
    {
        var request = new SubmitContentSuggestionRequest("Great Article", "A very useful resource", null, null, null);

        var result = await _sut.Submit(request);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
    }

    [Test]
    public async Task Submit_PersistsSuggestionWithCurrentUser()
    {
        var request = new SubmitContentSuggestionRequest("My Title", "My Description", "https://example.com", 42, "AI");

        await _sut.Submit(request);

        var saved = Db.ContentSuggestions.Single();
        Assert.That(saved.SubmittedBy, Is.EqualTo("user-1"));
        Assert.That(saved.Title, Is.EqualTo("My Title"));
        Assert.That(saved.Description, Is.EqualTo("My Description"));
        Assert.That(saved.Url, Is.EqualTo("https://example.com"));
        Assert.That(saved.RelatedCourseId, Is.EqualTo(42));
        Assert.That(saved.Topic, Is.EqualTo("AI"));
        Assert.That(saved.Status, Is.EqualTo(ContentSuggestionStatus.Pending));
    }

    [Test]
    public async Task Submit_SetsSubmittedAtToNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var request = new SubmitContentSuggestionRequest("Title", "Description", null, null, null);

        await _sut.Submit(request);

        var saved = Db.ContentSuggestions.Single();
        Assert.That(saved.SubmittedAt, Is.GreaterThan(before));
    }

    // --- GET /api/learners/me/content-suggestions ---

    [Test]
    public async Task GetMySuggestions_ReturnsOnlyCurrentUserSuggestions()
    {
        Db.ContentSuggestions.Add(new ContentSuggestionEntity { SubmittedBy = "user-1", Title = "Mine", Description = "Desc" });
        Db.ContentSuggestions.Add(new ContentSuggestionEntity { SubmittedBy = "user-2", Title = "Not Mine", Description = "Desc" });
        await Db.SaveChangesAsync();

        var result = await _sut.GetMySuggestions();

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var list = ok!.Value as IList<ContentSuggestionDto>;
        Assert.That(list, Has.Count.EqualTo(1));
        Assert.That(list![0].Title, Is.EqualTo("Mine"));
    }

    [Test]
    public async Task GetMySuggestions_ReturnsStatusInResponse()
    {
        Db.ContentSuggestions.Add(new ContentSuggestionEntity
        {
            SubmittedBy = "user-1",
            Title = "Title",
            Description = "Desc",
            Status = ContentSuggestionStatus.Approved,
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetMySuggestions();

        var ok = result.Result as OkObjectResult;
        var list = ok!.Value as IList<ContentSuggestionDto>;
        Assert.That(list![0].Status, Is.EqualTo(ContentSuggestionStatus.Approved));
    }

    [Test]
    public async Task GetMySuggestions_OrderedBySubmittedAtDescending()
    {
        var old = DateTime.UtcNow.AddDays(-5);
        var recent = DateTime.UtcNow.AddDays(-1);
        Db.ContentSuggestions.Add(new ContentSuggestionEntity { SubmittedBy = "user-1", Title = "Old", Description = "D", SubmittedAt = old });
        Db.ContentSuggestions.Add(new ContentSuggestionEntity { SubmittedBy = "user-1", Title = "Recent", Description = "D", SubmittedAt = recent });
        await Db.SaveChangesAsync();

        var result = await _sut.GetMySuggestions();

        var ok = result.Result as OkObjectResult;
        var list = ok!.Value as IList<ContentSuggestionDto>;
        Assert.That(list![0].Title, Is.EqualTo("Recent"));
    }
}
