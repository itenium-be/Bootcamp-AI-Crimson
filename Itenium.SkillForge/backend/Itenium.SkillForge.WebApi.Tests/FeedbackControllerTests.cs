using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class FeedbackControllerTests : DatabaseTestBase
{
    private ISkillForgeUser _user = null!;
    private FeedbackController _sut = null!;

    private const string UserId = "user-1";

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<ISkillForgeUser>();
        _user.IsBackOffice.Returns(true);
        _sut = new FeedbackController(Db, _user);
    }

    private async Task<int> SeedCourse(string name = "Test Course")
    {
        var course = new CourseEntity { Name = name, Status = CourseStatus.Published };
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

    private async Task<int> SeedFeedback(int courseId, int rating = 4, string? comment = "Good", bool isFlagged = false, int? lessonId = null)
    {
        var fb = new CourseFeedbackEntity
        {
            UserId = UserId,
            CourseId = courseId,
            LessonId = lessonId,
            Rating = rating,
            Comment = comment,
            IsFlagged = isFlagged,
        };
        Db.CourseFeedbacks.Add(fb);
        await Db.SaveChangesAsync();
        return fb.Id;
    }

    // --- Submit feedback ---

    [Test]
    public async Task SubmitFeedback_WhenCourseNotFound_ReturnsNotFound()
    {
        _user.Id.Returns(UserId);

        var result = await _sut.SubmitFeedback(999, new SubmitFeedbackRequest(4, "Great!"));

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task SubmitFeedback_WhenRatingOutOfRange_ReturnsBadRequest()
    {
        var courseId = await SeedCourse();
        _user.Id.Returns(UserId);

        var result = await _sut.SubmitFeedback(courseId, new SubmitFeedbackRequest(6, "Oops"));

        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task SubmitFeedback_CreatesEntry()
    {
        var courseId = await SeedCourse();
        _user.Id.Returns(UserId);

        var result = await _sut.SubmitFeedback(courseId, new SubmitFeedbackRequest(5, "Excellent!"));

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var row = await Db.CourseFeedbacks.SingleOrDefaultAsync(f => f.UserId == UserId && f.CourseId == courseId);
        Assert.That(row, Is.Not.Null);
        Assert.That(row!.Rating, Is.EqualTo(5));
    }

    // --- GET /courses/{id}/feedback ---

    [Test]
    public async Task GetCourseFeedback_ReturnsFeedbackForCourse()
    {
        var courseId = await SeedCourse();
        var otherId = await SeedCourse("Other");
        await SeedFeedback(courseId, 4);
        await SeedFeedback(courseId, 2);
        await SeedFeedback(otherId, 5);

        var result = await _sut.GetCourseFeedback(courseId);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var data = ok!.Value as CourseFeedbackSummaryDto;
        Assert.That(data, Is.Not.Null);
        Assert.That(data!.Entries, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetCourseFeedback_FiltersByRating()
    {
        var courseId = await SeedCourse();
        await SeedFeedback(courseId, 1);
        await SeedFeedback(courseId, 5);

        var result = await _sut.GetCourseFeedback(courseId, minRating: 4);

        var ok = result.Result as OkObjectResult;
        var data = ok!.Value as CourseFeedbackSummaryDto;
        Assert.That(data!.Entries, Has.Count.EqualTo(1));
        Assert.That(data.Entries[0].Rating, Is.EqualTo(5));
    }

    [Test]
    public async Task GetCourseFeedback_ComputesAverageRating()
    {
        var courseId = await SeedCourse();
        await SeedFeedback(courseId, 4);
        await SeedFeedback(courseId, 2);

        var result = await _sut.GetCourseFeedback(courseId);

        var ok = result.Result as OkObjectResult;
        var data = ok!.Value as CourseFeedbackSummaryDto;
        Assert.That(data!.AverageRating, Is.EqualTo(3.0).Within(0.01));
    }

    [Test]
    public async Task GetCourseFeedback_DoesNotReturnUserName()
    {
        var courseId = await SeedCourse();
        await SeedFeedback(courseId, 3, "Some comment");

        var result = await _sut.GetCourseFeedback(courseId);

        var ok = result.Result as OkObjectResult;
        var data = ok!.Value as CourseFeedbackSummaryDto;
        Assert.That(data!.Entries[0].UserId, Is.Null.Or.Empty);
    }

    // --- GET /lessons/{id}/feedback ---

    [Test]
    public async Task GetLessonFeedback_ReturnsFeedbackForLesson()
    {
        var courseId = await SeedCourse();
        var lessonId = await SeedLesson(courseId);
        await SeedFeedback(courseId, 5, lessonId: lessonId);
        await SeedFeedback(courseId, 3); // no lessonId

        var result = await _sut.GetLessonFeedback(lessonId);

        var ok = result.Result as OkObjectResult;
        var data = ok!.Value as CourseFeedbackSummaryDto;
        Assert.That(data!.Entries, Has.Count.EqualTo(1));
    }

    // --- PUT /feedback/{id}/flag ---

    [Test]
    public async Task FlagFeedback_SetsFlaggedTrue()
    {
        var courseId = await SeedCourse();
        var fbId = await SeedFeedback(courseId);

        var result = await _sut.FlagFeedback(fbId);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var row = await Db.CourseFeedbacks.FindAsync(fbId);
        Assert.That(row!.IsFlagged, Is.True);
    }

    [Test]
    public async Task FlagFeedback_WhenNotFound_ReturnsNotFound()
    {
        var result = await _sut.FlagFeedback(9999);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task DismissFeedback_SetsDismissedTrue()
    {
        var courseId = await SeedCourse();
        var fbId = await SeedFeedback(courseId, isFlagged: true);

        var result = await _sut.DismissFeedback(fbId);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var row = await Db.CourseFeedbacks.FindAsync(fbId);
        Assert.That(row!.IsDismissed, Is.True);
    }

    // --- GET /reports/feedback-summary ---

    [Test]
    public async Task GetFeedbackSummary_RanksCoursesByAverageRating()
    {
        var course1 = await SeedCourse("Course A");
        var course2 = await SeedCourse("Course B");
        await SeedFeedback(course1, 2);
        await SeedFeedback(course1, 4); // avg 3
        await SeedFeedback(course2, 5); // avg 5

        var result = await _sut.GetFeedbackSummary();

        var ok = result.Result as OkObjectResult;
        var items = ok!.Value as IList<CourseFeedbackRankingDto>;
        Assert.That(items, Is.Not.Null);
        Assert.That(items![0].CourseId, Is.EqualTo(course2));
        Assert.That(items![1].CourseId, Is.EqualTo(course1));
    }

    [Test]
    public async Task GetFeedbackSummary_OnlyBackofficeCanAccess()
    {
        _user.IsBackOffice.Returns(false);

        var result = await _sut.GetFeedbackSummary();

        Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
    }
}
