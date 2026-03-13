using Itenium.SkillForge.Entities;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class CourseAssignmentsControllerTests : DatabaseTestBase
{
    private CourseAssignmentsController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _sut = new CourseAssignmentsController(Db);
    }

    private async Task<CourseEntity> CreateCourse(string name = "Test Course")
    {
        var course = new CourseEntity { Name = name };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();
        return course;
    }

    [Test]
    public async Task GetAssignments_ReturnsAssignmentsForCourse()
    {
        var course = await CreateCourse();
        Db.CourseAssignments.Add(new CourseAssignmentEntity
        {
            CourseId = course.Id,
            AssigneeType = AssigneeType.Team,
            AssigneeId = "1",
            AssigneeName = "Java Team",
            Type = AssignmentType.Mandatory,
            AssignedBy = "admin",
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetAssignments(course.Id);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var assignments = ok!.Value as List<CourseAssignmentEntity>;
        Assert.That(assignments, Has.Count.EqualTo(1));
        Assert.That(assignments![0].AssigneeName, Is.EqualTo("Java Team"));
    }

    [Test]
    public async Task GetAssignments_WhenCourseNotFound_ReturnsNotFound()
    {
        var result = await _sut.GetAssignments(999);
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task GetAssignments_DoesNotReturnAssignmentsFromOtherCourses()
    {
        var course1 = await CreateCourse("Course 1");
        var course2 = await CreateCourse("Course 2");
        Db.CourseAssignments.Add(new CourseAssignmentEntity
        {
            CourseId = course2.Id,
            AssigneeType = AssigneeType.Team,
            AssigneeId = "1",
            Type = AssignmentType.Optional,
            AssignedBy = "admin",
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetAssignments(course1.Id);

        var ok = result.Result as OkObjectResult;
        var assignments = ok!.Value as List<CourseAssignmentEntity>;
        Assert.That(assignments, Is.Empty);
    }

    [Test]
    public async Task CreateAssignment_AddsAssignmentAndReturnsCreated()
    {
        var course = await CreateCourse();
        var request = new CreateAssignmentRequest(AssigneeType.Team, "1", "Java Team", AssignmentType.Mandatory, "manager1");

        var result = await _sut.CreateAssignment(course.Id, request);

        var created = result.Result as CreatedAtActionResult;
        Assert.That(created, Is.Not.Null);
        var assignment = created!.Value as CourseAssignmentEntity;
        Assert.That(assignment!.CourseId, Is.EqualTo(course.Id));
        Assert.That(assignment.AssigneeType, Is.EqualTo(AssigneeType.Team));
        Assert.That(assignment.AssigneeId, Is.EqualTo("1"));
        Assert.That(assignment.Type, Is.EqualTo(AssignmentType.Mandatory));
        Assert.That(assignment.AssignedBy, Is.EqualTo("manager1"));
    }

    [Test]
    public async Task CreateAssignment_WhenCourseNotFound_ReturnsNotFound()
    {
        var request = new CreateAssignmentRequest(AssigneeType.Team, "1", "Java Team", AssignmentType.Mandatory, "manager1");
        var result = await _sut.CreateAssignment(999, request);
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task CreateAssignment_WhenDuplicate_ReturnsConflict()
    {
        var course = await CreateCourse();
        Db.CourseAssignments.Add(new CourseAssignmentEntity
        {
            CourseId = course.Id,
            AssigneeType = AssigneeType.Team,
            AssigneeId = "1",
            Type = AssignmentType.Mandatory,
            AssignedBy = "admin",
        });
        await Db.SaveChangesAsync();

        var request = new CreateAssignmentRequest(AssigneeType.Team, "1", "Java Team", AssignmentType.Optional, "manager1");
        var result = await _sut.CreateAssignment(course.Id, request);

        Assert.That(result.Result, Is.TypeOf<ConflictObjectResult>());
    }

    [Test]
    public async Task DeleteAssignment_RemovesAssignmentAndReturnsNoContent()
    {
        var course = await CreateCourse();
        var assignment = new CourseAssignmentEntity
        {
            CourseId = course.Id,
            AssigneeType = AssigneeType.User,
            AssigneeId = "user-123",
            Type = AssignmentType.Optional,
            AssignedBy = "admin",
        };
        Db.CourseAssignments.Add(assignment);
        await Db.SaveChangesAsync();

        var result = await _sut.DeleteAssignment(course.Id, assignment.Id);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        Assert.That(await Db.CourseAssignments.FindAsync(assignment.Id), Is.Null);
    }

    [Test]
    public async Task DeleteAssignment_WhenNotFound_ReturnsNotFound()
    {
        var course = await CreateCourse();
        var result = await _sut.DeleteAssignment(course.Id, 999);
        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task DeleteAssignment_WhenAssignmentBelongsToDifferentCourse_ReturnsNotFound()
    {
        var course1 = await CreateCourse("Course 1");
        var course2 = await CreateCourse("Course 2");
        var assignment = new CourseAssignmentEntity
        {
            CourseId = course2.Id,
            AssigneeType = AssigneeType.Team,
            AssigneeId = "1",
            Type = AssignmentType.Mandatory,
            AssignedBy = "admin",
        };
        Db.CourseAssignments.Add(assignment);
        await Db.SaveChangesAsync();

        var result = await _sut.DeleteAssignment(course1.Id, assignment.Id);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }
}
