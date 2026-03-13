using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class ReportsControllerTests : DatabaseTestBase
{
    private ISkillForgeUser _user = null!;
    private ReportsController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<ISkillForgeUser>();
        _user.IsBackOffice.Returns(true);
        _sut = new ReportsController(Db, _user);
    }

    private async Task<CourseEntity> SeedCourse(string name = "Test Course", CourseStatus status = CourseStatus.Published)
    {
        var course = new CourseEntity { Name = name, Status = status };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();
        return course;
    }

    private async Task SeedEnrollment(string userId, int courseId, EnrollmentStatus status = EnrollmentStatus.Active, DateTime? enrolledAt = null)
    {
        Db.Enrollments.Add(new EnrollmentEntity
        {
            UserId = userId,
            CourseId = courseId,
            Status = status,
            EnrolledAt = enrolledAt ?? DateTime.UtcNow,
        });
        await Db.SaveChangesAsync();
    }

    // --- Authorization ---

    [Test]
    public async Task GetSummary_WhenNotBackOffice_ReturnsForbid()
    {
        _user.IsBackOffice.Returns(false);

        var result = await _sut.GetSummary();

        Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task GetCourseUsage_WhenNotBackOffice_ReturnsForbid()
    {
        _user.IsBackOffice.Returns(false);

        var result = await _sut.GetCourseUsage(null, null, null);

        Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
    }

    // --- GetSummary ---

    [Test]
    public async Task GetSummary_WithNoData_ReturnsZeros()
    {
        var result = await _sut.GetSummary();

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dto = ok!.Value as ReportSummaryDto;
        Assert.That(dto!.ActiveLearners, Is.EqualTo(0));
        Assert.That(dto.CompletionsThisMonth, Is.EqualTo(0));
        Assert.That(dto.TotalEnrollments, Is.EqualTo(0));
    }

    [Test]
    public async Task GetSummary_CountsActiveLearners()
    {
        var course = await SeedCourse();
        await SeedEnrollment("user-1", course.Id, EnrollmentStatus.Active);
        await SeedEnrollment("user-2", course.Id, EnrollmentStatus.Active);
        await SeedEnrollment("user-3", course.Id, EnrollmentStatus.Completed);

        var result = await _sut.GetSummary();

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as ReportSummaryDto;
        Assert.That(dto!.ActiveLearners, Is.EqualTo(2));
    }

    [Test]
    public async Task GetSummary_CountsCompletionsThisMonth()
    {
        var course = await SeedCourse();
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        await SeedEnrollment("user-1", course.Id, EnrollmentStatus.Completed, monthStart.AddDays(1));
        await SeedEnrollment("user-2", course.Id, EnrollmentStatus.Completed, monthStart.AddMonths(-1));

        var result = await _sut.GetSummary();

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as ReportSummaryDto;
        Assert.That(dto!.CompletionsThisMonth, Is.EqualTo(1));
    }

    [Test]
    public async Task GetSummary_CountsTotalEnrollments()
    {
        var course = await SeedCourse();
        await SeedEnrollment("user-1", course.Id);
        await SeedEnrollment("user-2", course.Id);

        var result = await _sut.GetSummary();

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as ReportSummaryDto;
        Assert.That(dto!.TotalEnrollments, Is.EqualTo(2));
    }

    // --- GetCourseUsage ---

    [Test]
    public async Task GetCourseUsage_ReturnsPerCourseStats()
    {
        var course = await SeedCourse("Course A");
        await SeedEnrollment("user-1", course.Id, EnrollmentStatus.Active);
        await SeedEnrollment("user-2", course.Id, EnrollmentStatus.Completed);

        var result = await _sut.GetCourseUsage(null, null, null);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var list = ok!.Value as IList<CourseUsageDto>;
        Assert.That(list, Has.Count.EqualTo(1));
        var dto = list![0];
        Assert.That(dto.CourseName, Is.EqualTo("Course A"));
        Assert.That(dto.TotalEnrollments, Is.EqualTo(2));
        Assert.That(dto.Completions, Is.EqualTo(1));
        Assert.That(dto.CompletionRate, Is.EqualTo(50.0).Within(0.01));
    }

    [Test]
    public async Task GetCourseUsage_FilterByCourseId()
    {
        var courseA = await SeedCourse("Course A");
        var courseB = await SeedCourse("Course B");
        await SeedEnrollment("user-1", courseA.Id);
        await SeedEnrollment("user-2", courseB.Id);

        var result = await _sut.GetCourseUsage(null, null, courseA.Id);

        var ok = result.Result as OkObjectResult;
        var list = ok!.Value as IList<CourseUsageDto>;
        Assert.That(list, Has.Count.EqualTo(1));
        Assert.That(list![0].CourseId, Is.EqualTo(courseA.Id));
    }

    [Test]
    public async Task GetCourseUsage_FilterByDateRange()
    {
        var course = await SeedCourse();
        var past = DateTime.UtcNow.AddDays(-30);
        var recent = DateTime.UtcNow.AddDays(-5);
        await SeedEnrollment("user-1", course.Id, enrolledAt: past);
        await SeedEnrollment("user-2", course.Id, enrolledAt: recent);

        var from = DateTime.UtcNow.AddDays(-10);
        var result = await _sut.GetCourseUsage(from, null, null);

        var ok = result.Result as OkObjectResult;
        var list = ok!.Value as IList<CourseUsageDto>;
        Assert.That(list, Has.Count.EqualTo(1));
        Assert.That(list![0].TotalEnrollments, Is.EqualTo(1));
    }

    [Test]
    public async Task GetCourseUsage_OrderedByEnrollmentCountDescending()
    {
        var courseA = await SeedCourse("A");
        var courseB = await SeedCourse("B");
        await SeedEnrollment("user-1", courseB.Id);
        await SeedEnrollment("user-2", courseB.Id);
        await SeedEnrollment("user-3", courseA.Id);

        var result = await _sut.GetCourseUsage(null, null, null);

        var ok = result.Result as OkObjectResult;
        var list = ok!.Value as IList<CourseUsageDto>;
        Assert.That(list![0].CourseId, Is.EqualTo(courseB.Id));
    }
}
