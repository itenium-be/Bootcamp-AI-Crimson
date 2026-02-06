using Itenium.SkillForge.Entities;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class CourseControllerTests : DatabaseTestBase
{
    private CourseController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _sut = new CourseController(Db);
    }

    [Test]
    public async Task GetCourses_ReturnsAllCourses()
    {
        Db.Courses.AddRange(
            new CourseEntity { Name = "C# Basics" },
            new CourseEntity { Name = "Advanced .NET" });
        await Db.SaveChangesAsync();

        var result = await _sut.GetCourses();

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var courses = okResult!.Value as List<CourseEntity>;
        Assert.That(courses, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetCourses_WhenNoCourses_ReturnsEmptyList()
    {
        var result = await _sut.GetCourses();

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var courses = okResult!.Value as List<CourseEntity>;
        Assert.That(courses, Is.Empty);
    }

    [Test]
    public async Task GetCourse_WhenExists_ReturnsCourse()
    {
        var course = new CourseEntity { Name = "C# Basics", Description = "Learn C#" };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();

        var result = await _sut.GetCourse(course.Id);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returnedCourse = okResult!.Value as CourseEntity;
        Assert.That(returnedCourse!.Name, Is.EqualTo("C# Basics"));
        Assert.That(returnedCourse.Description, Is.EqualTo("Learn C#"));
    }

    [Test]
    public async Task GetCourse_WhenNotExists_ReturnsNotFound()
    {
        var result = await _sut.GetCourse(999);
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task CreateCourse_AddsCourseAndReturnsCreated()
    {
        var request = new CreateCourseRequest("New Course", "Description", "Programming", "Beginner");

        var result = await _sut.CreateCourse(request);

        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        var course = createdResult!.Value as CourseEntity;
        Assert.That(course!.Name, Is.EqualTo("New Course"));
        Assert.That(course.Description, Is.EqualTo("Description"));
        Assert.That(course.Category, Is.EqualTo("Programming"));
        Assert.That(course.Level, Is.EqualTo("Beginner"));

        var savedCourse = await Db.Courses.FindAsync(course.Id);
        Assert.That(savedCourse, Is.Not.Null);
        Assert.That(savedCourse!.Name, Is.EqualTo("New Course"));
    }

    [Test]
    public async Task UpdateCourse_WhenExists_UpdatesAndReturnsOk()
    {
        var course = new CourseEntity { Name = "Old Name", Description = "Old Desc" };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();
        var request = new UpdateCourseRequest("New Name", "New Desc", "New Category", "Advanced");

        var result = await _sut.UpdateCourse(course.Id, request);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var updatedCourse = okResult!.Value as CourseEntity;
        Assert.That(updatedCourse!.Name, Is.EqualTo("New Name"));
        Assert.That(updatedCourse.Description, Is.EqualTo("New Desc"));
        Assert.That(updatedCourse.Category, Is.EqualTo("New Category"));
        Assert.That(updatedCourse.Level, Is.EqualTo("Advanced"));
    }

    [Test]
    public async Task UpdateCourse_WhenNotExists_ReturnsNotFound()
    {
        var request = new UpdateCourseRequest("Name", "Desc", null, null);
        var result = await _sut.UpdateCourse(999, request);
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task DeleteCourse_WhenExists_RemovesAndReturnsNoContent()
    {
        var course = new CourseEntity { Name = "To Delete" };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();

        var result = await _sut.DeleteCourse(course.Id);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        var deletedCourse = await Db.Courses.FindAsync(course.Id);
        Assert.That(deletedCourse, Is.Null);
    }

    [Test]
    public async Task DeleteCourse_WhenNotExists_ReturnsNotFound()
    {
        var result = await _sut.DeleteCourse(999);
        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }
}
