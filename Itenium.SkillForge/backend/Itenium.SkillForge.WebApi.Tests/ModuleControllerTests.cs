using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class ModuleControllerTests : DatabaseTestBase
{
    private ISkillForgeUser _user = null!;
    private ModuleController _sut = null!;

    private const string UserId = "manager-1";

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<ISkillForgeUser>();
        _user.Id.Returns(UserId);
        _user.IsBackOffice.Returns(true);
        _sut = new ModuleController(Db, _user);
    }

    private async Task<int> SeedModule(string name = "Module A", string? goal = null)
    {
        var module = new ModuleEntity { Name = name, Goal = goal, CreatedBy = UserId };
        Db.Modules.Add(module);
        await Db.SaveChangesAsync();
        return module.Id;
    }

    private async Task<int> SeedCourse(string name = "Course 1", int? moduleId = null, int moduleOrder = 0)
    {
        var course = new CourseEntity { Name = name, Status = CourseStatus.Published, ModuleId = moduleId, ModuleOrder = moduleOrder };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();
        return course.Id;
    }

    // --- GET /modules ---

    [Test]
    public async Task GetModules_ReturnsAllModules()
    {
        await SeedModule("Module A");
        await SeedModule("Module B");

        var result = await _sut.GetModules();

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var items = ok!.Value as IList<ModuleResponse>;
        Assert.That(items, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetModules_ReturnsEmptyList_WhenNoModules()
    {
        var result = await _sut.GetModules();

        var ok = result.Result as OkObjectResult;
        var items = ok!.Value as IList<ModuleResponse>;
        Assert.That(items, Is.Empty);
    }

    // --- POST /modules ---

    [Test]
    public async Task CreateModule_CreatesAndReturnsModule()
    {
        var result = await _sut.CreateModule(new ModuleRequest("Path to .NET", "Learn .NET", "Become a .NET dev"));

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dto = ok!.Value as ModuleResponse;
        Assert.That(dto!.Name, Is.EqualTo("Path to .NET"));
        Assert.That(await Db.Modules.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task CreateModule_StoresCreatedBy()
    {
        await _sut.CreateModule(new ModuleRequest("Module X", null, null));

        var module = await Db.Modules.SingleAsync();
        Assert.That(module.CreatedBy, Is.EqualTo(UserId));
    }

    // --- PUT /modules/{id} ---

    [Test]
    public async Task UpdateModule_UpdatesFields()
    {
        var id = await SeedModule("Old Name");

        var result = await _sut.UpdateModule(id, new ModuleRequest("New Name", "New Desc", "New Goal"));

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var module = await Db.Modules.FindAsync(id);
        Assert.That(module!.Name, Is.EqualTo("New Name"));
        Assert.That(module.Goal, Is.EqualTo("New Goal"));
    }

    [Test]
    public async Task UpdateModule_WhenNotFound_ReturnsNotFound()
    {
        var result = await _sut.UpdateModule(9999, new ModuleRequest("X", null, null));

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    // --- DELETE /modules/{id} ---

    [Test]
    public async Task DeleteModule_RemovesModule()
    {
        var id = await SeedModule();

        var result = await _sut.DeleteModule(id);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        Assert.That(await Db.Modules.FindAsync(id), Is.Null);
    }

    [Test]
    public async Task DeleteModule_UnassignsCoursesInModule()
    {
        var moduleId = await SeedModule();
        var courseId = await SeedCourse(moduleId: moduleId, moduleOrder: 1);

        await _sut.DeleteModule(moduleId);

        var course = await Db.Courses.FindAsync(courseId);
        Assert.That(course!.ModuleId, Is.Null);
    }

    [Test]
    public async Task DeleteModule_WhenNotFound_ReturnsNotFound()
    {
        var result = await _sut.DeleteModule(9999);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    // --- POST /modules/{id}/courses ---

    [Test]
    public async Task AddCourse_AssignsCourseToModule()
    {
        var moduleId = await SeedModule();
        var courseId = await SeedCourse();

        var result = await _sut.AddCourse(moduleId, new ModuleCourseRequest(courseId, 1));

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var course = await Db.Courses.FindAsync(courseId);
        Assert.That(course!.ModuleId, Is.EqualTo(moduleId));
        Assert.That(course.ModuleOrder, Is.EqualTo(1));
    }

    [Test]
    public async Task AddCourse_WhenModuleNotFound_ReturnsNotFound()
    {
        var courseId = await SeedCourse();

        var result = await _sut.AddCourse(9999, new ModuleCourseRequest(courseId, 1));

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task AddCourse_WhenCourseNotFound_ReturnsNotFound()
    {
        var moduleId = await SeedModule();

        var result = await _sut.AddCourse(moduleId, new ModuleCourseRequest(9999, 1));

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task AddCourse_WhenCourseAlreadyInAnotherModule_ReturnsBadRequest()
    {
        var module1 = await SeedModule("Module 1");
        var module2 = await SeedModule("Module 2");
        var courseId = await SeedCourse(moduleId: module1, moduleOrder: 1);

        var result = await _sut.AddCourse(module2, new ModuleCourseRequest(courseId, 1));

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    // --- DELETE /modules/{id}/courses/{courseId} ---

    [Test]
    public async Task RemoveCourse_UnassignsCourseFromModule()
    {
        var moduleId = await SeedModule();
        var courseId = await SeedCourse(moduleId: moduleId, moduleOrder: 1);

        var result = await _sut.RemoveCourse(moduleId, courseId);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var course = await Db.Courses.FindAsync(courseId);
        Assert.That(course!.ModuleId, Is.Null);
    }

    [Test]
    public async Task RemoveCourse_WhenNotInModule_ReturnsNotFound()
    {
        var moduleId = await SeedModule();
        var courseId = await SeedCourse(); // no module

        var result = await _sut.RemoveCourse(moduleId, courseId);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    // --- PUT /modules/{id}/courses/reorder ---

    [Test]
    public async Task ReorderCourses_UpdatesModuleOrder()
    {
        var moduleId = await SeedModule();
        var c1 = await SeedCourse("C1", moduleId: moduleId, moduleOrder: 1);
        var c2 = await SeedCourse("C2", moduleId: moduleId, moduleOrder: 2);
        var c3 = await SeedCourse("C3", moduleId: moduleId, moduleOrder: 3);

        var result = await _sut.ReorderCourses(moduleId, new ReorderCoursesRequest([c3, c1, c2]));

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var courses = await Db.Courses.Where(c => c.ModuleId == moduleId).ToListAsync();
        Assert.That(courses.Single(c => c.Id == c3).ModuleOrder, Is.EqualTo(1));
        Assert.That(courses.Single(c => c.Id == c1).ModuleOrder, Is.EqualTo(2));
        Assert.That(courses.Single(c => c.Id == c2).ModuleOrder, Is.EqualTo(3));
    }
}
