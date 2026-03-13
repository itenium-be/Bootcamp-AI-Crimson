using Itenium.SkillForge.Entities;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class CourseStatusControllerTests : DatabaseTestBase
{
    private CourseController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _sut = new CourseController(Db);
    }

    [Test]
    public async Task CreateCourse_SetsStatusToDraft()
    {
        var request = new CreateCourseRequest("New Course", null, null, null);

        var result = await _sut.CreateCourse(request);

        var created = (result.Result as CreatedAtActionResult)!.Value as CourseEntity;
        Assert.That(created!.Status, Is.EqualTo(CourseStatus.Draft));
    }

    [Test]
    public async Task CreateCourse_WithEstimatedDuration_SavesDuration()
    {
        var request = new CreateCourseRequest("New Course", null, null, null, EstimatedDuration: 120);

        var result = await _sut.CreateCourse(request);

        var created = (result.Result as CreatedAtActionResult)!.Value as CourseEntity;
        Assert.That(created!.EstimatedDuration, Is.EqualTo(120));
    }

    [Test]
    public async Task UpdateCourse_WithEstimatedDuration_UpdatesDuration()
    {
        var course = new CourseEntity { Name = "Course", EstimatedDuration = 60 };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();

        var result = await _sut.UpdateCourse(course.Id, new UpdateCourseRequest("Course", null, null, null, EstimatedDuration: 90));

        var updated = (result.Result as OkObjectResult)!.Value as CourseEntity;
        Assert.That(updated!.EstimatedDuration, Is.EqualTo(90));
    }

    [Test]
    public async Task PublishCourse_WhenDraft_PublishesCourse()
    {
        var course = new CourseEntity { Name = "Draft Course", Status = CourseStatus.Draft };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();

        var result = await _sut.PublishCourse(course.Id);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var published = okResult!.Value as CourseEntity;
        Assert.That(published!.Status, Is.EqualTo(CourseStatus.Published));
    }

    [Test]
    public async Task PublishCourse_WhenArchived_Republishes()
    {
        var course = new CourseEntity { Name = "Archived Course", Status = CourseStatus.Archived };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();

        var result = await _sut.PublishCourse(course.Id);

        var published = (result as OkObjectResult)!.Value as CourseEntity;
        Assert.That(published!.Status, Is.EqualTo(CourseStatus.Published));
    }

    [Test]
    public async Task PublishCourse_WhenNotFound_ReturnsNotFound()
    {
        var result = await _sut.PublishCourse(999);
        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task ArchiveCourse_WhenPublished_ArchivesCourse()
    {
        var course = new CourseEntity { Name = "Published Course", Status = CourseStatus.Published };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();

        var result = await _sut.ArchiveCourse(course.Id);

        var archived = (result as OkObjectResult)!.Value as CourseEntity;
        Assert.That(archived!.Status, Is.EqualTo(CourseStatus.Archived));
    }

    [Test]
    public async Task ArchiveCourse_WhenDraft_ArchivesCourse()
    {
        var course = new CourseEntity { Name = "Draft Course", Status = CourseStatus.Draft };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();

        var result = await _sut.ArchiveCourse(course.Id);

        var archived = (result as OkObjectResult)!.Value as CourseEntity;
        Assert.That(archived!.Status, Is.EqualTo(CourseStatus.Archived));
    }

    [Test]
    public async Task ArchiveCourse_WhenNotFound_ReturnsNotFound()
    {
        var result = await _sut.ArchiveCourse(999);
        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task DeleteCourse_WhenPublished_ReturnsConflict()
    {
        var course = new CourseEntity { Name = "Published Course", Status = CourseStatus.Published };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();

        var result = await _sut.DeleteCourse(course.Id);

        Assert.That(result, Is.TypeOf<ConflictObjectResult>());
    }

    [Test]
    public async Task DeleteCourse_WhenArchived_ReturnsConflict()
    {
        var course = new CourseEntity { Name = "Archived Course", Status = CourseStatus.Archived };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();

        var result = await _sut.DeleteCourse(course.Id);

        Assert.That(result, Is.TypeOf<ConflictObjectResult>());
    }

    [Test]
    public async Task DeleteCourse_WhenDraft_DeletesCourse()
    {
        var course = new CourseEntity { Name = "Draft Course", Status = CourseStatus.Draft };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();

        var result = await _sut.DeleteCourse(course.Id);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        Assert.That(await Db.Courses.FindAsync(course.Id), Is.Null);
    }
}
