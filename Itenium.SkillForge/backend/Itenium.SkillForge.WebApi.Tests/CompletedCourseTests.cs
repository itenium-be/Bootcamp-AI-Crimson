using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class CompletedCourseTests : DatabaseTestBase
{
    private ISkillForgeUser _user = null!;
    private LessonController _lessonController = null!;
    private EnrollmentController _enrollmentController = null!;

    private const string UserId = "learner-1";

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<ISkillForgeUser>();
        _user.UserId.Returns(UserId);
        _user.Id.Returns(UserId);
        _lessonController = new LessonController(Db, _user);
        _enrollmentController = new EnrollmentController(Db, _user);
    }

    private async Task<(int courseId, int[] lessonIds)> SeedCourseWithLessons(int lessonCount = 2)
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

        Db.Enrollments.Add(new EnrollmentEntity { UserId = UserId, CourseId = course.Id });
        await Db.SaveChangesAsync();

        return (course.Id, lessonIds);
    }

    [Test]
    public async Task SetStatus_Done_WhenAllLessonsDone_MarksEnrollmentCompleted()
    {
        var (courseId, lessonIds) = await SeedCourseWithLessons(2);

        await _lessonController.SetStatus(lessonIds[0], new SetLessonStatusRequest("done"));
        await _lessonController.SetStatus(lessonIds[1], new SetLessonStatusRequest("done"));

        var enrollment = await Db.Enrollments.SingleAsync(e => e.UserId == UserId && e.CourseId == courseId);
        Assert.That(enrollment.Status, Is.EqualTo(EnrollmentStatus.Completed));
        Assert.That(enrollment.CompletedAt, Is.Not.Null);
    }

    [Test]
    public async Task SetStatus_Done_WhenNotAllLessonsDone_EnrollmentStaysActive()
    {
        var (courseId, lessonIds) = await SeedCourseWithLessons(2);

        await _lessonController.SetStatus(lessonIds[0], new SetLessonStatusRequest("done"));

        var enrollment = await Db.Enrollments.SingleAsync(e => e.UserId == UserId && e.CourseId == courseId);
        Assert.That(enrollment.Status, Is.EqualTo(EnrollmentStatus.Active));
        Assert.That(enrollment.CompletedAt, Is.Null);
    }

    [Test]
    public async Task SetStatus_Done_WhenNoEnrollment_DoesNotThrow()
    {
        var course = new CourseEntity { Name = "Unenrolled", Status = CourseStatus.Published };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();
        var lesson = new LessonEntity { CourseId = course.Id, Title = "Lesson", SortOrder = 1 };
        Db.Lessons.Add(lesson);
        await Db.SaveChangesAsync();

        var result = await _lessonController.SetStatus(lesson.Id, new SetLessonStatusRequest("done"));

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task SetStatus_New_WhenPreviouslyCompleted_ResetsEnrollmentToActive()
    {
        var (courseId, lessonIds) = await SeedCourseWithLessons(1);

        await _lessonController.SetStatus(lessonIds[0], new SetLessonStatusRequest("done"));
        await _lessonController.SetStatus(lessonIds[0], new SetLessonStatusRequest("new"));

        var enrollment = await Db.Enrollments.SingleAsync(e => e.UserId == UserId && e.CourseId == courseId);
        Assert.That(enrollment.Status, Is.EqualTo(EnrollmentStatus.Active));
        Assert.That(enrollment.CompletedAt, Is.Null);
    }

    [Test]
    public async Task GetMyEnrollments_ReturnsCompletedAt()
    {
        var (courseId, lessonIds) = await SeedCourseWithLessons(1);

        await _lessonController.SetStatus(lessonIds[0], new SetLessonStatusRequest("done"));

        var result = await _enrollmentController.GetMyEnrollments();

        var ok = result.Result as OkObjectResult;
        var enrollments = ok!.Value as IList<EnrollmentResponse>;
        Assert.That(enrollments![0].CompletedAt, Is.Not.Null);
        Assert.That(enrollments[0].Status, Is.EqualTo("Completed"));
    }

    [Test]
    public async Task GetMyEnrollments_WithStatusFilter_ReturnsOnlyMatchingEnrollments()
    {
        var course1 = new CourseEntity { Name = "Active Course", Status = CourseStatus.Published };
        var course2 = new CourseEntity { Name = "Completed Course", Status = CourseStatus.Published };
        Db.Courses.AddRange(course1, course2);
        await Db.SaveChangesAsync();

        var lesson = new LessonEntity { CourseId = course2.Id, Title = "Only Lesson", SortOrder = 1 };
        Db.Lessons.Add(lesson);
        Db.Enrollments.AddRange(
            new EnrollmentEntity { UserId = UserId, CourseId = course1.Id },
            new EnrollmentEntity { UserId = UserId, CourseId = course2.Id });
        await Db.SaveChangesAsync();

        await _lessonController.SetStatus(lesson.Id, new SetLessonStatusRequest("done"));

        var result = await _enrollmentController.GetMyEnrollments("completed");

        var ok = result.Result as OkObjectResult;
        var enrollments = ok!.Value as IList<EnrollmentResponse>;
        Assert.That(enrollments, Has.Count.EqualTo(1));
        Assert.That(enrollments![0].CourseName, Is.EqualTo("Completed Course"));
    }

    [Test]
    public async Task GetMyEnrollments_WithActiveFilter_ReturnsOnlyActiveEnrollments()
    {
        var course1 = new CourseEntity { Name = "Active Course", Status = CourseStatus.Published };
        var course2 = new CourseEntity { Name = "Completed Course", Status = CourseStatus.Published };
        Db.Courses.AddRange(course1, course2);
        await Db.SaveChangesAsync();

        var lesson = new LessonEntity { CourseId = course2.Id, Title = "Only Lesson", SortOrder = 1 };
        Db.Lessons.Add(lesson);
        Db.Enrollments.AddRange(
            new EnrollmentEntity { UserId = UserId, CourseId = course1.Id },
            new EnrollmentEntity { UserId = UserId, CourseId = course2.Id });
        await Db.SaveChangesAsync();

        await _lessonController.SetStatus(lesson.Id, new SetLessonStatusRequest("done"));

        var result = await _enrollmentController.GetMyEnrollments("active");

        var ok = result.Result as OkObjectResult;
        var enrollments = ok!.Value as IList<EnrollmentResponse>;
        Assert.That(enrollments, Has.Count.EqualTo(1));
        Assert.That(enrollments![0].CourseName, Is.EqualTo("Active Course"));
    }

    [Test]
    public async Task GetMyEnrollments_IncludesModuleName()
    {
        var module = new ModuleEntity { Name = "Advanced .NET" };
        Db.Modules.Add(module);
        await Db.SaveChangesAsync();

        var course = new CourseEntity { Name = "C# Basics", Status = CourseStatus.Published, ModuleId = module.Id };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();

        Db.Enrollments.Add(new EnrollmentEntity { UserId = UserId, CourseId = course.Id });
        await Db.SaveChangesAsync();

        var result = await _enrollmentController.GetMyEnrollments();

        var ok = result.Result as OkObjectResult;
        var enrollments = ok!.Value as IList<EnrollmentResponse>;
        Assert.That(enrollments![0].ModuleName, Is.EqualTo("Advanced .NET"));
    }
}
