using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class LessonProgressTests : DatabaseTestBase
{
    private ISkillForgeUser _user = null!;
    private LessonController _sut = null!;

    private const string LearnerId = "learner-1";
    private const string OtherLearnerId = "learner-2";

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<ISkillForgeUser>();
        _user.UserId.Returns(LearnerId);
        _user.IsBackOffice.Returns(false);
        _sut = new LessonController(Db, _user);
    }

    private async Task<int> SeedLesson()
    {
        var course = new CourseEntity { Name = "Course", Status = CourseStatus.Published };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();
        var lesson = new LessonEntity { CourseId = course.Id, Title = "Lesson 1", SortOrder = 1 };
        Db.Lessons.Add(lesson);
        await Db.SaveChangesAsync();
        return lesson.Id;
    }

    private async Task SeedProgress(string userId, int lessonId)
    {
        Db.LessonProgresses.Add(new LessonProgressEntity { UserId = userId, LessonId = lessonId });
        await Db.SaveChangesAsync();
    }

    // --- SetStatus records progress when done ---

    [Test]
    public async Task SetStatus_WhenDone_RecordsProgress()
    {
        var lessonId = await SeedLesson();

        await _sut.SetStatus(lessonId, new SetLessonStatusRequest("done"));

        var progress = await Db.LessonProgresses
            .SingleOrDefaultAsync(p => p.UserId == LearnerId && p.LessonId == lessonId);
        Assert.That(progress, Is.Not.Null);
    }

    [Test]
    public async Task SetStatus_WhenDoneCalledTwice_DoesNotDuplicateProgress()
    {
        var lessonId = await SeedLesson();

        await _sut.SetStatus(lessonId, new SetLessonStatusRequest("done"));
        await _sut.SetStatus(lessonId, new SetLessonStatusRequest("done"));

        var count = await Db.LessonProgresses
            .CountAsync(p => p.UserId == LearnerId && p.LessonId == lessonId);
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public async Task SetStatus_WhenLater_DoesNotRecordProgress()
    {
        var lessonId = await SeedLesson();

        await _sut.SetStatus(lessonId, new SetLessonStatusRequest("later"));

        var count = await Db.LessonProgresses.CountAsync(p => p.LessonId == lessonId);
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public async Task SetStatus_WhenNew_DoesNotRemoveProgress()
    {
        var lessonId = await SeedLesson();
        await SeedProgress(LearnerId, lessonId);

        await _sut.SetStatus(lessonId, new SetLessonStatusRequest("new"));

        var progress = await Db.LessonProgresses
            .SingleOrDefaultAsync(p => p.UserId == LearnerId && p.LessonId == lessonId);
        Assert.That(progress, Is.Not.Null); // progress is preserved even when status reset
    }

    // --- GET /lessons/{id}/progress-summary ---

    [Test]
    public async Task GetProgressSummary_ReturnsCompletionCount()
    {
        var lessonId = await SeedLesson();
        await SeedProgress(LearnerId, lessonId);
        await SeedProgress(OtherLearnerId, lessonId);

        var result = await _sut.GetProgressSummary(lessonId);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dto = ok!.Value as LessonProgressSummaryDto;
        Assert.That(dto!.CompletedCount, Is.EqualTo(2));
    }

    [Test]
    public async Task GetProgressSummary_WhenNoCompletions_ReturnsZero()
    {
        var lessonId = await SeedLesson();

        var result = await _sut.GetProgressSummary(lessonId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as LessonProgressSummaryDto;
        Assert.That(dto!.CompletedCount, Is.EqualTo(0));
    }

    // --- DELETE /lessons/{id}/progress ---

    [Test]
    public async Task ResetProgress_RemovesAllLearnerProgress()
    {
        _user.IsBackOffice.Returns(true);
        var lessonId = await SeedLesson();
        await SeedProgress(LearnerId, lessonId);
        await SeedProgress(OtherLearnerId, lessonId);

        var result = await _sut.ResetProgress(lessonId);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        Assert.That(await Db.LessonProgresses.CountAsync(p => p.LessonId == lessonId), Is.EqualTo(0));
    }

    [Test]
    public async Task ResetProgress_WhenNotBackOffice_ReturnsForbid()
    {
        _user.IsBackOffice.Returns(false);
        var lessonId = await SeedLesson();

        var result = await _sut.ResetProgress(lessonId);

        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task ResetProgress_DoesNotAffectOtherLessons()
    {
        _user.IsBackOffice.Returns(true);
        var lesson1 = await SeedLesson();
        var lesson2 = await SeedLesson();
        await SeedProgress(LearnerId, lesson1);
        await SeedProgress(LearnerId, lesson2);

        await _sut.ResetProgress(lesson1);

        Assert.That(await Db.LessonProgresses.CountAsync(p => p.LessonId == lesson2), Is.EqualTo(1));
    }
}
