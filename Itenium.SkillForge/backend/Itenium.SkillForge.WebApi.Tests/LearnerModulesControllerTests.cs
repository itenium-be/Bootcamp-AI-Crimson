using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class LearnerModulesControllerTests : DatabaseTestBase
{
    private ISkillForgeUser _user = null!;
    private LearnerModulesController _sut = null!;

    private const string UserId = "learner-1";

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<ISkillForgeUser>();
        _user.Id.Returns(UserId);
        _sut = new LearnerModulesController(Db, _user);
    }

    private async Task<(ModuleEntity module, CourseEntity course, LessonEntity lesson1, LessonEntity lesson2)> SeedModuleWithCourse()
    {
        var module = new ModuleEntity { Name = "Module A" };
        Db.Modules.Add(module);
        await Db.SaveChangesAsync();

        var course = new CourseEntity { Name = "Course A", Status = CourseStatus.Published, ModuleId = module.Id, ModuleOrder = 1 };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();

        var lesson1 = new LessonEntity { CourseId = course.Id, Title = "Lesson 1", SortOrder = 1 };
        var lesson2 = new LessonEntity { CourseId = course.Id, Title = "Lesson 2", SortOrder = 2 };
        Db.Lessons.AddRange(lesson1, lesson2);
        await Db.SaveChangesAsync();

        return (module, course, lesson1, lesson2);
    }

    [Test]
    public async Task GetMyModules_WithNoModules_ReturnsEmptyList()
    {
        var result = await _sut.GetMyModules();

        var ok = result.Result as OkObjectResult;
        var modules = ok!.Value as IList<LearnerModuleResponse>;
        Assert.That(modules, Is.Empty);
    }

    [Test]
    public async Task GetMyModules_WithModuleButNoCourses_ReturnsModuleWithZeroProgress()
    {
        var module = new ModuleEntity { Name = "Empty Module" };
        Db.Modules.Add(module);
        await Db.SaveChangesAsync();

        var result = await _sut.GetMyModules();

        var ok = result.Result as OkObjectResult;
        var modules = ok!.Value as IList<LearnerModuleResponse>;
        Assert.That(modules, Has.Count.EqualTo(1));
        Assert.That(modules![0].CompletionPercent, Is.EqualTo(0));
        Assert.That(modules![0].Courses, Is.Empty);
    }

    [Test]
    public async Task GetMyModules_WithNoLessonsCompleted_ReturnsZeroCompletion()
    {
        await SeedModuleWithCourse();

        var result = await _sut.GetMyModules();

        var ok = result.Result as OkObjectResult;
        var modules = ok!.Value as IList<LearnerModuleResponse>;
        Assert.That(modules, Has.Count.EqualTo(1));
        var course = modules![0].Courses[0];
        Assert.That(course.CompletionPercent, Is.EqualTo(0));
        Assert.That(modules[0].CompletionPercent, Is.EqualTo(0));
    }

    [Test]
    public async Task GetMyModules_WithOneLessonCompleted_ReturnsPartialCompletion()
    {
        var (_, course, lesson1, _) = await SeedModuleWithCourse();

        Db.LessonStatuses.Add(new LessonStatusEntity
        {
            UserId = UserId,
            LessonId = lesson1.Id,
            Status = LessonStatusValue.Done,
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetMyModules();

        var ok = result.Result as OkObjectResult;
        var modules = ok!.Value as IList<LearnerModuleResponse>;
        var courseDto = modules![0].Courses[0];
        Assert.That(courseDto.CompletedLessons, Is.EqualTo(1));
        Assert.That(courseDto.TotalLessons, Is.EqualTo(2));
        Assert.That(courseDto.CompletionPercent, Is.EqualTo(50));
    }

    [Test]
    public async Task GetMyModules_WithAllLessonsCompleted_Returns100Completion()
    {
        var (_, _, lesson1, lesson2) = await SeedModuleWithCourse();

        Db.LessonStatuses.AddRange(
            new LessonStatusEntity { UserId = UserId, LessonId = lesson1.Id, Status = LessonStatusValue.Done },
            new LessonStatusEntity { UserId = UserId, LessonId = lesson2.Id, Status = LessonStatusValue.Done }
        );
        await Db.SaveChangesAsync();

        var result = await _sut.GetMyModules();

        var ok = result.Result as OkObjectResult;
        var modules = ok!.Value as IList<LearnerModuleResponse>;
        Assert.That(modules![0].Courses[0].CompletionPercent, Is.EqualTo(100));
        Assert.That(modules![0].CompletionPercent, Is.EqualTo(100));
    }

    [Test]
    public async Task GetMyModules_DoesNotCountLaterStatusAsCompleted()
    {
        var (_, _, lesson1, _) = await SeedModuleWithCourse();

        Db.LessonStatuses.Add(new LessonStatusEntity
        {
            UserId = UserId,
            LessonId = lesson1.Id,
            Status = LessonStatusValue.Later,
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetMyModules();

        var ok = result.Result as OkObjectResult;
        var modules = ok!.Value as IList<LearnerModuleResponse>;
        Assert.That(modules![0].Courses[0].CompletionPercent, Is.EqualTo(0));
    }

    [Test]
    public async Task GetMyModules_OnlyCountsCurrentUserProgress()
    {
        var (_, _, lesson1, _) = await SeedModuleWithCourse();

        Db.LessonStatuses.Add(new LessonStatusEntity
        {
            UserId = "other-user",
            LessonId = lesson1.Id,
            Status = LessonStatusValue.Done,
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetMyModules();

        var ok = result.Result as OkObjectResult;
        var modules = ok!.Value as IList<LearnerModuleResponse>;
        Assert.That(modules![0].Courses[0].CompletionPercent, Is.EqualTo(0));
    }

    [Test]
    public async Task GetMyModules_ModuleCompletionIsAverageOfCourseCompletions()
    {
        var module = new ModuleEntity { Name = "Module Multi" };
        Db.Modules.Add(module);
        await Db.SaveChangesAsync();

        var courseA = new CourseEntity { Name = "A", Status = CourseStatus.Published, ModuleId = module.Id, ModuleOrder = 1 };
        var courseB = new CourseEntity { Name = "B", Status = CourseStatus.Published, ModuleId = module.Id, ModuleOrder = 2 };
        Db.Courses.AddRange(courseA, courseB);
        await Db.SaveChangesAsync();

        var lessonA = new LessonEntity { CourseId = courseA.Id, Title = "LA1", SortOrder = 1 };
        var lessonB = new LessonEntity { CourseId = courseB.Id, Title = "LB1", SortOrder = 1 };
        Db.Lessons.AddRange(lessonA, lessonB);
        await Db.SaveChangesAsync();

        // Complete all lessons in course A (100%), none in course B (0%) => average 50%
        Db.LessonStatuses.Add(new LessonStatusEntity { UserId = UserId, LessonId = lessonA.Id, Status = LessonStatusValue.Done });
        await Db.SaveChangesAsync();

        var result = await _sut.GetMyModules();

        var ok = result.Result as OkObjectResult;
        var modules = ok!.Value as IList<LearnerModuleResponse>;
        Assert.That(modules![0].CompletionPercent, Is.EqualTo(50));
    }

    [Test]
    public async Task GetMyModules_MarksMandatoryCoursesForUser()
    {
        var (_, course, _, _) = await SeedModuleWithCourse();

        Db.CourseAssignments.Add(new CourseAssignmentEntity
        {
            CourseId = course.Id,
            AssigneeType = AssigneeType.User,
            AssigneeId = UserId,
            Type = AssignmentType.Mandatory,
            AssignedBy = "admin",
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetMyModules();

        var ok = result.Result as OkObjectResult;
        var modules = ok!.Value as IList<LearnerModuleResponse>;
        Assert.That(modules![0].Courses[0].IsMandatory, Is.True);
    }

    [Test]
    public async Task GetMyModules_OptionalCoursesAreNotMandatory()
    {
        var (_, course, _, _) = await SeedModuleWithCourse();

        Db.CourseAssignments.Add(new CourseAssignmentEntity
        {
            CourseId = course.Id,
            AssigneeType = AssigneeType.User,
            AssigneeId = UserId,
            Type = AssignmentType.Optional,
            AssignedBy = "admin",
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetMyModules();

        var ok = result.Result as OkObjectResult;
        var modules = ok!.Value as IList<LearnerModuleResponse>;
        Assert.That(modules![0].Courses[0].IsMandatory, Is.False);
    }

    [Test]
    public async Task GetModuleProgress_WhenNotFound_ReturnsNotFound()
    {
        var result = await _sut.GetModuleProgress(999);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task GetModuleProgress_ReturnsDetailedProgress()
    {
        var (module, _, lesson1, lesson2) = await SeedModuleWithCourse();

        Db.LessonStatuses.Add(new LessonStatusEntity { UserId = UserId, LessonId = lesson1.Id, Status = LessonStatusValue.Done });
        await Db.SaveChangesAsync();

        var result = await _sut.GetModuleProgress(module.Id);

        var ok = result.Result as OkObjectResult;
        var response = ok!.Value as LearnerModuleResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Id, Is.EqualTo(module.Id));
        Assert.That(response.Courses, Has.Count.EqualTo(1));
        Assert.That(response.Courses[0].CompletedLessons, Is.EqualTo(1));
        Assert.That(response.Courses[0].TotalLessons, Is.EqualTo(2));
        Assert.That(response.Courses[0].CompletionPercent, Is.EqualTo(50));
    }
}
