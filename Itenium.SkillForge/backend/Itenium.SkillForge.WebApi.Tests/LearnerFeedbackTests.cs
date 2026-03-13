using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class LearnerFeedbackTests : DatabaseTestBase
{
    private ISkillForgeUser _user = null!;
    private FeedbackController _sut = null!;

    private const string LearnerId = "learner-1";

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<ISkillForgeUser>();
        _user.Id.Returns(LearnerId);
        _user.IsBackOffice.Returns(false);
        _sut = new FeedbackController(Db, _user);
    }

    private async Task<int> SeedCourse()
    {
        var course = new CourseEntity { Name = "Course", Status = CourseStatus.Published };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();
        return course.Id;
    }

    private async Task<int> SeedLesson(int courseId)
    {
        var lesson = new LessonEntity { CourseId = courseId, Title = "Lesson 1", SortOrder = 1 };
        Db.Lessons.Add(lesson);
        await Db.SaveChangesAsync();
        return lesson.Id;
    }

    // ---- Course feedback: duplicate check ----

    [Test]
    public async Task SubmitCourseFeedback_WhenAlreadySubmitted_ReturnsConflict()
    {
        var courseId = await SeedCourse();
        Db.CourseFeedbacks.Add(new CourseFeedbackEntity
        {
            UserId = LearnerId,
            CourseId = courseId,
            Rating = 4,
        });
        await Db.SaveChangesAsync();

        var result = await _sut.SubmitFeedback(courseId, new SubmitFeedbackRequest(5, null));

        Assert.That(result.Result, Is.InstanceOf<ConflictObjectResult>());
    }

    // ---- PUT /api/courses/{id}/feedback ----

    [Test]
    public async Task UpdateCourseFeedback_WhenNotFound_ReturnsNotFound()
    {
        var courseId = await SeedCourse();

        var result = await _sut.UpdateCourseFeedback(courseId, new SubmitFeedbackRequest(3, null));

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdateCourseFeedback_WhenExists_UpdatesRatingAndComment()
    {
        var courseId = await SeedCourse();
        Db.CourseFeedbacks.Add(new CourseFeedbackEntity
        {
            UserId = LearnerId,
            CourseId = courseId,
            Rating = 3,
            Comment = "OK",
        });
        await Db.SaveChangesAsync();

        var result = await _sut.UpdateCourseFeedback(courseId, new SubmitFeedbackRequest(5, "Excellent!"));

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var row = await Db.CourseFeedbacks.SingleAsync(f => f.UserId == LearnerId && f.CourseId == courseId && f.LessonId == null);
        Assert.That(row.Rating, Is.EqualTo(5));
        Assert.That(row.Comment, Is.EqualTo("Excellent!"));
    }

    [Test]
    public async Task UpdateCourseFeedback_WhenRatingInvalid_ReturnsBadRequest()
    {
        var courseId = await SeedCourse();

        var result = await _sut.UpdateCourseFeedback(courseId, new SubmitFeedbackRequest(0, null));

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    // ---- POST /api/lessons/{id}/feedback ----

    [Test]
    public async Task SubmitLessonFeedback_WhenLessonNotFound_ReturnsNotFound()
    {
        var result = await _sut.SubmitLessonFeedback(999, new SubmitFeedbackRequest(4, null));

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task SubmitLessonFeedback_WhenRatingInvalid_ReturnsBadRequest()
    {
        var courseId = await SeedCourse();
        var lessonId = await SeedLesson(courseId);

        var result = await _sut.SubmitLessonFeedback(lessonId, new SubmitFeedbackRequest(6, null));

        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task SubmitLessonFeedback_CreatesEntry()
    {
        var courseId = await SeedCourse();
        var lessonId = await SeedLesson(courseId);

        var result = await _sut.SubmitLessonFeedback(lessonId, new SubmitFeedbackRequest(4, "Good lesson"));

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var row = await Db.CourseFeedbacks.SingleOrDefaultAsync(f => f.UserId == LearnerId && f.LessonId == lessonId);
        Assert.That(row, Is.Not.Null);
        Assert.That(row!.Rating, Is.EqualTo(4));
        Assert.That(row.Comment, Is.EqualTo("Good lesson"));
    }

    [Test]
    public async Task SubmitLessonFeedback_WhenAlreadySubmitted_ReturnsConflict()
    {
        var courseId = await SeedCourse();
        var lessonId = await SeedLesson(courseId);
        Db.CourseFeedbacks.Add(new CourseFeedbackEntity
        {
            UserId = LearnerId,
            CourseId = courseId,
            LessonId = lessonId,
            Rating = 3,
        });
        await Db.SaveChangesAsync();

        var result = await _sut.SubmitLessonFeedback(lessonId, new SubmitFeedbackRequest(5, null));

        Assert.That(result.Result, Is.InstanceOf<ConflictObjectResult>());
    }

    // ---- PUT /api/lessons/{id}/feedback ----

    [Test]
    public async Task UpdateLessonFeedback_WhenNotFound_ReturnsNotFound()
    {
        var courseId = await SeedCourse();
        var lessonId = await SeedLesson(courseId);

        var result = await _sut.UpdateLessonFeedback(lessonId, new SubmitFeedbackRequest(4, null));

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdateLessonFeedback_WhenExists_UpdatesEntry()
    {
        var courseId = await SeedCourse();
        var lessonId = await SeedLesson(courseId);
        Db.CourseFeedbacks.Add(new CourseFeedbackEntity
        {
            UserId = LearnerId,
            CourseId = courseId,
            LessonId = lessonId,
            Rating = 2,
            Comment = "Meh",
        });
        await Db.SaveChangesAsync();

        var result = await _sut.UpdateLessonFeedback(lessonId, new SubmitFeedbackRequest(5, "Great!"));

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var row = await Db.CourseFeedbacks.SingleAsync(f => f.UserId == LearnerId && f.LessonId == lessonId);
        Assert.That(row.Rating, Is.EqualTo(5));
        Assert.That(row.Comment, Is.EqualTo("Great!"));
    }

    // ---- GET my feedback ----

    [Test]
    public async Task GetMyCourseFeedback_WhenExists_ReturnsFeedback()
    {
        var courseId = await SeedCourse();
        Db.CourseFeedbacks.Add(new CourseFeedbackEntity
        {
            UserId = LearnerId,
            CourseId = courseId,
            Rating = 4,
            Comment = "Good",
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetMyCourseFeedback(courseId);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dto = ok!.Value as FeedbackEntryDto;
        Assert.That(dto!.Rating, Is.EqualTo(4));
    }

    [Test]
    public async Task GetMyCourseFeedback_WhenNotExists_ReturnsNotFound()
    {
        var courseId = await SeedCourse();

        var result = await _sut.GetMyCourseFeedback(courseId);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task GetMyLessonFeedback_WhenExists_ReturnsFeedback()
    {
        var courseId = await SeedCourse();
        var lessonId = await SeedLesson(courseId);
        Db.CourseFeedbacks.Add(new CourseFeedbackEntity
        {
            UserId = LearnerId,
            CourseId = courseId,
            LessonId = lessonId,
            Rating = 3,
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetMyLessonFeedback(lessonId);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dto = ok!.Value as FeedbackEntryDto;
        Assert.That(dto!.Rating, Is.EqualTo(3));
    }

    [Test]
    public async Task GetMyLessonFeedback_WhenNotExists_ReturnsNotFound()
    {
        var courseId = await SeedCourse();
        var lessonId = await SeedLesson(courseId);

        var result = await _sut.GetMyLessonFeedback(lessonId);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }
}
