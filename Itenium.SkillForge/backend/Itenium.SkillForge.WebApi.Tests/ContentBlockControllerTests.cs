using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class ContentBlockControllerTests : DatabaseTestBase
{
    private ISkillForgeUser _user = null!;
    private ContentBlockController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<ISkillForgeUser>();
        _sut = new ContentBlockController(Db, _user);
    }

    private async Task<LessonEntity> CreateLesson()
    {
        var course = new CourseEntity { Name = "Test Course" };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();

        var lesson = new LessonEntity { CourseId = course.Id, Title = "Test Lesson", SortOrder = 1 };
        Db.Lessons.Add(lesson);
        await Db.SaveChangesAsync();
        return lesson;
    }

    // --- GET ---

    [Test]
    public async Task GetContentBlocks_WhenLessonExists_ReturnsBlocks()
    {
        var lesson = await CreateLesson();
        Db.ContentBlocks.AddRange(
            new ContentBlockEntity { LessonId = lesson.Id, Type = "text", Content = "{\"markdown\":\"Hello\"}", Order = 1 },
            new ContentBlockEntity { LessonId = lesson.Id, Type = "link", Content = "{\"url\":\"https://example.com\",\"label\":\"Docs\"}", Order = 2 });
        await Db.SaveChangesAsync();

        var result = await _sut.GetContentBlocks(lesson.Id);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var blocks = ok!.Value as List<ContentBlockEntity>;
        Assert.That(blocks, Has.Count.EqualTo(2));
        Assert.That(blocks![0].Order, Is.EqualTo(1));
    }

    [Test]
    public async Task GetContentBlocks_WhenNoBlocks_ReturnsEmptyList()
    {
        var lesson = await CreateLesson();

        var result = await _sut.GetContentBlocks(lesson.Id);

        var ok = result.Result as OkObjectResult;
        var blocks = ok!.Value as List<ContentBlockEntity>;
        Assert.That(blocks, Is.Empty);
    }

    // --- POST ---

    [Test]
    public async Task AddContentBlock_WhenNotManager_ReturnsForbid()
    {
        _user.IsBackOffice.Returns(false);
        _user.IsManager.Returns(false);
        var lesson = await CreateLesson();

        var result = await _sut.AddContentBlock(lesson.Id, new ContentBlockRequest("text", "{\"markdown\":\"Hi\"}", 1));

        Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task AddContentBlock_WhenLessonNotFound_ReturnsNotFound()
    {
        _user.IsManager.Returns(true);

        var result = await _sut.AddContentBlock(999, new ContentBlockRequest("text", "{\"markdown\":\"Hi\"}", 1));

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task AddContentBlock_WhenValid_ReturnsCreatedBlock()
    {
        _user.IsManager.Returns(true);
        var lesson = await CreateLesson();

        var result = await _sut.AddContentBlock(lesson.Id, new ContentBlockRequest("youtube", "{\"url\":\"https://youtube.com/watch?v=abc\"}", 1));

        var created = result.Result as CreatedAtActionResult;
        Assert.That(created, Is.Not.Null);
        var block = created!.Value as ContentBlockEntity;
        Assert.That(block!.Type, Is.EqualTo("youtube"));
        Assert.That(block.LessonId, Is.EqualTo(lesson.Id));

        var saved = await Db.ContentBlocks.FindAsync(block.Id);
        Assert.That(saved, Is.Not.Null);
    }

    // --- PUT ---

    [Test]
    public async Task UpdateContentBlock_WhenNotManager_ReturnsForbid()
    {
        _user.IsBackOffice.Returns(false);
        _user.IsManager.Returns(false);
        var lesson = await CreateLesson();

        var result = await _sut.UpdateContentBlock(lesson.Id, 1, new ContentBlockRequest("text", "{}", 1));

        Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task UpdateContentBlock_WhenNotFound_ReturnsNotFound()
    {
        _user.IsManager.Returns(true);
        var lesson = await CreateLesson();

        var result = await _sut.UpdateContentBlock(lesson.Id, 999, new ContentBlockRequest("text", "{}", 1));

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdateContentBlock_WhenValid_UpdatesAndReturnsBlock()
    {
        _user.IsManager.Returns(true);
        var lesson = await CreateLesson();
        var block = new ContentBlockEntity { LessonId = lesson.Id, Type = "text", Content = "{\"markdown\":\"Old\"}", Order = 1 };
        Db.ContentBlocks.Add(block);
        await Db.SaveChangesAsync();

        var result = await _sut.UpdateContentBlock(lesson.Id, block.Id, new ContentBlockRequest("text", "{\"markdown\":\"New\"}", 2));

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var updated = ok!.Value as ContentBlockEntity;
        Assert.That(updated!.Content, Is.EqualTo("{\"markdown\":\"New\"}"));
        Assert.That(updated.Order, Is.EqualTo(2));
    }

    // --- DELETE ---

    [Test]
    public async Task DeleteContentBlock_WhenNotManager_ReturnsForbid()
    {
        _user.IsBackOffice.Returns(false);
        _user.IsManager.Returns(false);
        var lesson = await CreateLesson();

        var result = await _sut.DeleteContentBlock(lesson.Id, 1);

        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task DeleteContentBlock_WhenNotFound_ReturnsNotFound()
    {
        _user.IsManager.Returns(true);
        var lesson = await CreateLesson();

        var result = await _sut.DeleteContentBlock(lesson.Id, 999);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task DeleteContentBlock_WhenFound_RemovesAndReturnsNoContent()
    {
        _user.IsManager.Returns(true);
        var lesson = await CreateLesson();
        var block = new ContentBlockEntity { LessonId = lesson.Id, Type = "image", Content = "{\"url\":\"https://example.com/img.png\"}", Order = 1 };
        Db.ContentBlocks.Add(block);
        await Db.SaveChangesAsync();

        var result = await _sut.DeleteContentBlock(lesson.Id, block.Id);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var deleted = await Db.ContentBlocks.FindAsync(block.Id);
        Assert.That(deleted, Is.Null);
    }

    // --- REORDER ---

    [Test]
    public async Task ReorderContentBlocks_WhenNotManager_ReturnsForbid()
    {
        _user.IsBackOffice.Returns(false);
        _user.IsManager.Returns(false);
        var lesson = await CreateLesson();

        var result = await _sut.ReorderContentBlocks(lesson.Id, new ReorderRequest([1, 2]));

        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task ReorderContentBlocks_WhenValid_UpdatesOrder()
    {
        _user.IsManager.Returns(true);
        var lesson = await CreateLesson();
        var b1 = new ContentBlockEntity { LessonId = lesson.Id, Type = "text", Content = "{}", Order = 1 };
        var b2 = new ContentBlockEntity { LessonId = lesson.Id, Type = "link", Content = "{}", Order = 2 };
        Db.ContentBlocks.AddRange(b1, b2);
        await Db.SaveChangesAsync();

        var result = await _sut.ReorderContentBlocks(lesson.Id, new ReorderRequest([b2.Id, b1.Id]));

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var updated1 = await Db.ContentBlocks.FindAsync(b1.Id);
        var updated2 = await Db.ContentBlocks.FindAsync(b2.Id);
        Assert.That(updated2!.Order, Is.EqualTo(1));
        Assert.That(updated1!.Order, Is.EqualTo(2));
    }
}
