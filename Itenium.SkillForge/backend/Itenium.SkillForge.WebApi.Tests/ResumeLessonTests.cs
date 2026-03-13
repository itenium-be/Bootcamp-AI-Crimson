using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class ResumeLessonTests : DatabaseTestBase
{
    private ISkillForgeUser _user = null!;
    private EnrollmentController _sut = null!;

    private const string LearnerId = "learner-1";

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<ISkillForgeUser>();
        _user.Id.Returns(LearnerId);
        _sut = new EnrollmentController(Db, _user);
    }

    private async Task<(int courseId, int[] lessonIds)> SeedCourseWithLessons(int lessonCount = 3)
    {
        var course = new CourseEntity { Name = "Test Course", Status = CourseStatus.Published };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();

        var lessonIds = new int[lessonCount];
        for (var i = 0; i < lessonCount; i++)
        {
            var lesson = new LessonEntity { CourseId = course.Id, Title = $"Lesson {i + 1}", SortOrder = i + 1 };
            Db.Lessons.Add(lesson);
            await Db.SaveChangesAsync();
            lessonIds[i] = lesson.Id;
        }

        return (course.Id, lessonIds);
    }

    private async Task EnrollUser(string userId, int courseId, int? lastVisitedLessonId = null)
    {
        var enrollment = new EnrollmentEntity
        {
            UserId = userId,
            CourseId = courseId,
            LastVisitedLessonId = lastVisitedLessonId,
        };
        Db.Enrollments.Add(enrollment);
        await Db.SaveChangesAsync();
    }

    private async Task SetLessonStatus(string userId, int lessonId, LessonStatusValue status)
    {
        Db.LessonStatuses.Add(new LessonStatusEntity
        {
            UserId = userId,
            LessonId = lessonId,
            Status = status,
        });
        await Db.SaveChangesAsync();
    }

    // ---- Resume ----

    [Test]
    public async Task Resume_WhenNotEnrolled_ReturnsNotFound()
    {
        var (courseId, _) = await SeedCourseWithLessons();

        var result = await _sut.Resume(courseId);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Resume_WhenEnrolledAndNoLessons_ReturnsAllComplete()
    {
        var course = new CourseEntity { Name = "Empty Course", Status = CourseStatus.Published };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();
        await EnrollUser(LearnerId, course.Id);

        var result = await _sut.Resume(course.Id);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dto = ok!.Value as ResumeResponse;
        Assert.That(dto!.IsComplete, Is.True);
        Assert.That(dto.LessonId, Is.Null);
    }

    [Test]
    public async Task Resume_WhenEnrolledAndNoStatuses_ReturnsFirstLesson()
    {
        var (courseId, lessonIds) = await SeedCourseWithLessons();
        await EnrollUser(LearnerId, courseId);

        var result = await _sut.Resume(courseId);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dto = ok!.Value as ResumeResponse;
        Assert.That(dto!.IsComplete, Is.False);
        Assert.That(dto.LessonId, Is.EqualTo(lessonIds[0]));
    }

    [Test]
    public async Task Resume_WhenFirstLessonDone_ReturnsNextNotDoneLesson()
    {
        var (courseId, lessonIds) = await SeedCourseWithLessons();
        await EnrollUser(LearnerId, courseId);
        await SetLessonStatus(LearnerId, lessonIds[0], LessonStatusValue.Done);

        var result = await _sut.Resume(courseId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as ResumeResponse;
        Assert.That(dto!.LessonId, Is.EqualTo(lessonIds[1]));
        Assert.That(dto.IsComplete, Is.False);
    }

    [Test]
    public async Task Resume_WhenStatusIsLater_CountsAsNotDone()
    {
        var (courseId, lessonIds) = await SeedCourseWithLessons(2);
        await EnrollUser(LearnerId, courseId);
        await SetLessonStatus(LearnerId, lessonIds[0], LessonStatusValue.Done);
        await SetLessonStatus(LearnerId, lessonIds[1], LessonStatusValue.Later);

        var result = await _sut.Resume(courseId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as ResumeResponse;
        Assert.That(dto!.LessonId, Is.EqualTo(lessonIds[1]));
        Assert.That(dto.IsComplete, Is.False);
    }

    [Test]
    public async Task Resume_WhenAllLessonsDone_ReturnsAllCompleteTrue()
    {
        var (courseId, lessonIds) = await SeedCourseWithLessons(2);
        await EnrollUser(LearnerId, courseId);
        foreach (var id in lessonIds)
            await SetLessonStatus(LearnerId, id, LessonStatusValue.Done);

        var result = await _sut.Resume(courseId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as ResumeResponse;
        Assert.That(dto!.IsComplete, Is.True);
    }

    [Test]
    public async Task Resume_WhenAllDoneAndLastVisitedSet_ReturnsLastVisitedLesson()
    {
        var (courseId, lessonIds) = await SeedCourseWithLessons(2);
        await EnrollUser(LearnerId, courseId, lastVisitedLessonId: lessonIds[1]);
        foreach (var id in lessonIds)
            await SetLessonStatus(LearnerId, id, LessonStatusValue.Done);

        var result = await _sut.Resume(courseId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as ResumeResponse;
        Assert.That(dto!.IsComplete, Is.True);
        Assert.That(dto.LessonId, Is.EqualTo(lessonIds[1]));
    }

    // ---- TrackLastVisited ----

    [Test]
    public async Task TrackLastVisited_WhenNotEnrolled_ReturnsNotFound()
    {
        var (courseId, lessonIds) = await SeedCourseWithLessons(1);

        var result = await _sut.TrackLastVisited(courseId, lessonIds[0]);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task TrackLastVisited_WhenEnrolled_UpdatesLastVisitedLessonId()
    {
        var (courseId, lessonIds) = await SeedCourseWithLessons(2);
        await EnrollUser(LearnerId, courseId);

        await _sut.TrackLastVisited(courseId, lessonIds[1]);

        var enrollment = Db.Enrollments.First(e => e.UserId == LearnerId && e.CourseId == courseId);
        Assert.That(enrollment.LastVisitedLessonId, Is.EqualTo(lessonIds[1]));
    }
}
