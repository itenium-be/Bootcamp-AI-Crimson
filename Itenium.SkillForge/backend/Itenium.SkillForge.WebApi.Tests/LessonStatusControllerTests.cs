using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class LessonStatusControllerTests : DatabaseTestBase
{
    private ISkillForgeUser _user = null!;
    private LessonController _sut = null!;

    private const string UserId = "user-1";
    private const string OtherUserId = "user-2";

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<ISkillForgeUser>();
        _user.UserId.Returns(UserId);
        _sut = new LessonController(Db, _user);
    }

    private async Task<int> SeedLesson()
    {
        var lesson = new LessonEntity { CourseId = 1, Title = "Test Lesson", SortOrder = 1 };
        Db.Lessons.Add(lesson);
        await Db.SaveChangesAsync();
        return lesson.Id;
    }

    // GET status

    [Test]
    public async Task GetStatus_WhenNoRow_ReturnsNew()
    {
        var lessonId = await SeedLesson();

        var result = await _sut.GetStatus(lessonId);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo("new"));
    }

    [Test]
    public async Task GetStatus_WhenRowExists_ReturnsSavedStatus()
    {
        var lessonId = await SeedLesson();
        Db.LessonStatuses.Add(new LessonStatusEntity { UserId = UserId, LessonId = lessonId, Status = LessonStatusValue.Done });
        await Db.SaveChangesAsync();

        var result = await _sut.GetStatus(lessonId);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok!.Value, Is.EqualTo("done"));
    }

    [Test]
    public async Task GetStatus_OnlyReturnsCurrentUserStatus()
    {
        var lessonId = await SeedLesson();
        Db.LessonStatuses.Add(new LessonStatusEntity { UserId = OtherUserId, LessonId = lessonId, Status = LessonStatusValue.Done });
        await Db.SaveChangesAsync();

        var result = await _sut.GetStatus(lessonId);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok!.Value, Is.EqualTo("new"));
    }

    // PUT status - set done

    [Test]
    public async Task SetStatus_Done_CreatesRow()
    {
        var lessonId = await SeedLesson();

        var result = await _sut.SetStatus(lessonId, new SetLessonStatusRequest("done"));

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var row = await Db.LessonStatuses.SingleOrDefaultAsync(s => s.UserId == UserId && s.LessonId == lessonId);
        Assert.That(row, Is.Not.Null);
        Assert.That(row!.Status, Is.EqualTo(LessonStatusValue.Done));
    }

    [Test]
    public async Task SetStatus_Later_CreatesRow()
    {
        var lessonId = await SeedLesson();

        var result = await _sut.SetStatus(lessonId, new SetLessonStatusRequest("later"));

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var row = await Db.LessonStatuses.SingleOrDefaultAsync(s => s.UserId == UserId && s.LessonId == lessonId);
        Assert.That(row!.Status, Is.EqualTo(LessonStatusValue.Later));
    }

    [Test]
    public async Task SetStatus_Done_UpdatesExistingRow()
    {
        var lessonId = await SeedLesson();
        Db.LessonStatuses.Add(new LessonStatusEntity { UserId = UserId, LessonId = lessonId, Status = LessonStatusValue.Later });
        await Db.SaveChangesAsync();

        await _sut.SetStatus(lessonId, new SetLessonStatusRequest("done"));

        var row = await Db.LessonStatuses.SingleAsync(s => s.UserId == UserId && s.LessonId == lessonId);
        Assert.That(row.Status, Is.EqualTo(LessonStatusValue.Done));
    }

    [Test]
    public async Task SetStatus_New_DeletesRow()
    {
        var lessonId = await SeedLesson();
        Db.LessonStatuses.Add(new LessonStatusEntity { UserId = UserId, LessonId = lessonId, Status = LessonStatusValue.Done });
        await Db.SaveChangesAsync();

        var result = await _sut.SetStatus(lessonId, new SetLessonStatusRequest("new"));

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var row = await Db.LessonStatuses.SingleOrDefaultAsync(s => s.UserId == UserId && s.LessonId == lessonId);
        Assert.That(row, Is.Null);
    }

    [Test]
    public async Task SetStatus_New_WhenNoRow_ReturnsNoContent()
    {
        var lessonId = await SeedLesson();

        var result = await _sut.SetStatus(lessonId, new SetLessonStatusRequest("new"));

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task SetStatus_InvalidStatus_ReturnsBadRequest()
    {
        var lessonId = await SeedLesson();

        var result = await _sut.SetStatus(lessonId, new SetLessonStatusRequest("invalid"));

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task SetStatus_LessonNotFound_ReturnsNotFound()
    {
        var result = await _sut.SetStatus(9999, new SetLessonStatusRequest("done"));

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    // Done contributes to completion %

    [Test]
    public async Task GetLessons_ReturnsCourseIdLessonsWithUserStatus()
    {
        var lesson1 = new LessonEntity { CourseId = 1, Title = "Lesson 1", SortOrder = 1 };
        var lesson2 = new LessonEntity { CourseId = 1, Title = "Lesson 2", SortOrder = 2 };
        var lesson3 = new LessonEntity { CourseId = 2, Title = "Other course", SortOrder = 1 };
        Db.Lessons.AddRange(lesson1, lesson2, lesson3);
        await Db.SaveChangesAsync();
        Db.LessonStatuses.Add(new LessonStatusEntity { UserId = UserId, LessonId = lesson1.Id, Status = LessonStatusValue.Done });
        await Db.SaveChangesAsync();

        var result = await _sut.GetLessons(1);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var lessons = ok!.Value as IList<LessonWithStatusDto>;
        Assert.That(lessons, Has.Count.EqualTo(2));
        Assert.That(lessons!.First(l => l.Id == lesson1.Id).Status, Is.EqualTo("done"));
        Assert.That(lessons!.First(l => l.Id == lesson2.Id).Status, Is.EqualTo("new"));
    }
}
