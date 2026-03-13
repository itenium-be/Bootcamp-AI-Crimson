using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class EnrollmentControllerTests : DatabaseTestBase
{
    private ISkillForgeUser _user = null!;
    private EnrollmentController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<ISkillForgeUser>();
        _sut = new EnrollmentController(Db, _user);
    }

    [Test]
    public async Task Enroll_WhenCourseNotFound_ReturnsNotFound()
    {
        _user.Id.Returns("user-1");

        var result = await _sut.Enroll(999);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Enroll_WhenCourseNotPublished_ReturnsBadRequest()
    {
        var course = new CourseEntity { Name = "Draft Course", Status = CourseStatus.Draft };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();
        _user.Id.Returns("user-1");

        var result = await _sut.Enroll(course.Id);

        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Enroll_WhenPublishedCourse_CreatesEnrollment()
    {
        var course = new CourseEntity { Name = "Published Course", Status = CourseStatus.Published };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();
        _user.Id.Returns("user-1");

        var result = await _sut.Enroll(course.Id);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var enrollment = Db.Enrollments.FirstOrDefault(e => e.UserId == "user-1" && e.CourseId == course.Id);
        Assert.That(enrollment, Is.Not.Null);
        Assert.That(enrollment!.Status, Is.EqualTo(EnrollmentStatus.Active));
    }

    [Test]
    public async Task Enroll_WhenAlreadyEnrolled_IsIdempotent()
    {
        var course = new CourseEntity { Name = "Published Course", Status = CourseStatus.Published };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();
        _user.Id.Returns("user-1");

        await _sut.Enroll(course.Id);
        var result = await _sut.Enroll(course.Id);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        Assert.That(Db.Enrollments.Count(e => e.UserId == "user-1" && e.CourseId == course.Id), Is.EqualTo(1));
    }

    [Test]
    public async Task GetMyEnrollments_ReturnsOnlyCurrentUserEnrollments()
    {
        var course1 = new CourseEntity { Name = "Course A", Status = CourseStatus.Published };
        var course2 = new CourseEntity { Name = "Course B", Status = CourseStatus.Published };
        Db.Courses.AddRange(course1, course2);
        await Db.SaveChangesAsync();

        Db.Enrollments.AddRange(
            new EnrollmentEntity { UserId = "user-1", CourseId = course1.Id },
            new EnrollmentEntity { UserId = "user-2", CourseId = course2.Id });
        await Db.SaveChangesAsync();

        _user.Id.Returns("user-1");

        var result = await _sut.GetMyEnrollments();

        var ok = result.Result as OkObjectResult;
        var enrollments = ok!.Value as IList<EnrollmentResponse>;
        Assert.That(enrollments, Has.Count.EqualTo(1));
        Assert.That(enrollments![0].CourseName, Is.EqualTo("Course A"));
    }

    [Test]
    public async Task GetMyEnrollments_WhenNoEnrollments_ReturnsEmpty()
    {
        _user.Id.Returns("user-1");

        var result = await _sut.GetMyEnrollments();

        var ok = result.Result as OkObjectResult;
        var enrollments = ok!.Value as IList<EnrollmentResponse>;
        Assert.That(enrollments, Is.Empty);
    }
}
