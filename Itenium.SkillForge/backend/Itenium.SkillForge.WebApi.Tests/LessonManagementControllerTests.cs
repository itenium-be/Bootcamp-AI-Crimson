using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class LessonManagementControllerTests : DatabaseTestBase
{
    private ISkillForgeUser _user = null!;
    private LessonController _sut = null!;

    private const string ManagerId = "manager-1";

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<ISkillForgeUser>();
        _user.UserId.Returns(ManagerId);
        _user.IsManager.Returns(true);
        _sut = new LessonController(Db, _user);
    }

    private async Task<int> SeedCourse(string name = "Test Course")
    {
        var course = new CourseEntity { Name = name, Status = CourseStatus.Draft };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();
        return course.Id;
    }

    private async Task<int> SeedLesson(int courseId, string title = "Test Lesson", int sortOrder = 1, int? estimatedDuration = null)
    {
        var lesson = new LessonEntity { CourseId = courseId, Title = title, SortOrder = sortOrder, EstimatedDuration = estimatedDuration };
        Db.Lessons.Add(lesson);
        await Db.SaveChangesAsync();
        return lesson.Id;
    }

    // --- GET /api/courses/{courseId}/lessons ---

    [Test]
    public async Task GetCourseLessons_WhenNotManager_ReturnsForbid()
    {
        _user.IsManager.Returns(false);
        var courseId = await SeedCourse();

        var result = await _sut.GetCourseLessons(courseId);

        Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task GetCourseLessons_ReturnsLessonsForCourse()
    {
        var courseId = await SeedCourse();
        await SeedLesson(courseId, "Lesson 1", 1);
        await SeedLesson(courseId, "Lesson 2", 2);
        var otherCourseId = await SeedCourse("Other Course");
        await SeedLesson(otherCourseId, "Other Lesson", 1);

        var result = await _sut.GetCourseLessons(courseId);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var lessons = ok!.Value as IList<LessonDto>;
        Assert.That(lessons, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetCourseLessons_ReturnsLessonsOrderedBySortOrder()
    {
        var courseId = await SeedCourse();
        await SeedLesson(courseId, "Lesson B", 2);
        await SeedLesson(courseId, "Lesson A", 1);

        var result = await _sut.GetCourseLessons(courseId);

        var ok = result.Result as OkObjectResult;
        var lessons = ok!.Value as IList<LessonDto>;
        Assert.That(lessons![0].Title, Is.EqualTo("Lesson A"));
        Assert.That(lessons![1].Title, Is.EqualTo("Lesson B"));
    }

    [Test]
    public async Task GetCourseLessons_IncludesEstimatedDuration()
    {
        var courseId = await SeedCourse();
        await SeedLesson(courseId, "Lesson With Duration", 1, estimatedDuration: 45);

        var result = await _sut.GetCourseLessons(courseId);

        var ok = result.Result as OkObjectResult;
        var lessons = ok!.Value as IList<LessonDto>;
        Assert.That(lessons![0].EstimatedDuration, Is.EqualTo(45));
    }

    // --- POST /api/courses/{courseId}/lessons ---

    [Test]
    public async Task CreateLesson_WhenNotManager_ReturnsForbid()
    {
        _user.IsManager.Returns(false);
        var courseId = await SeedCourse();

        var result = await _sut.CreateLesson(courseId, new CreateLessonRequest("New Lesson", null, 1));

        Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task CreateLesson_CreatesLessonAndReturns201()
    {
        var courseId = await SeedCourse();

        var result = await _sut.CreateLesson(courseId, new CreateLessonRequest("New Lesson", 30, 1));

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var created = (result.Result as CreatedAtActionResult)!;
        var dto = created.Value as LessonDto;
        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.Title, Is.EqualTo("New Lesson"));
        Assert.That(dto.EstimatedDuration, Is.EqualTo(30));
        Assert.That(dto.SortOrder, Is.EqualTo(1));
    }

    [Test]
    public async Task CreateLesson_PersistsToDatabase()
    {
        var courseId = await SeedCourse();

        await _sut.CreateLesson(courseId, new CreateLessonRequest("Persisted Lesson", null, 2));

        var lesson = await Db.Lessons.SingleOrDefaultAsync(l => l.Title == "Persisted Lesson");
        Assert.That(lesson, Is.Not.Null);
        Assert.That(lesson!.CourseId, Is.EqualTo(courseId));
    }

    // --- PUT /api/lessons/{id} ---

    [Test]
    public async Task UpdateLesson_WhenNotManager_ReturnsForbid()
    {
        _user.IsManager.Returns(false);
        var courseId = await SeedCourse();
        var lessonId = await SeedLesson(courseId);

        var result = await _sut.UpdateLesson(lessonId, new UpdateLessonRequest("Updated", null, 1));

        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task UpdateLesson_WhenNotFound_ReturnsNotFound()
    {
        var result = await _sut.UpdateLesson(9999, new UpdateLessonRequest("Updated", null, 1));

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdateLesson_UpdatesAndReturnsNoContent()
    {
        var courseId = await SeedCourse();
        var lessonId = await SeedLesson(courseId, "Old Title");

        var result = await _sut.UpdateLesson(lessonId, new UpdateLessonRequest("New Title", 60, 3));

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var lesson = await Db.Lessons.FindAsync(lessonId);
        Assert.That(lesson!.Title, Is.EqualTo("New Title"));
        Assert.That(lesson.EstimatedDuration, Is.EqualTo(60));
        Assert.That(lesson.SortOrder, Is.EqualTo(3));
    }

    // --- DELETE /api/lessons/{id} ---

    [Test]
    public async Task DeleteLesson_WhenNotManager_ReturnsForbid()
    {
        _user.IsManager.Returns(false);
        var courseId = await SeedCourse();
        var lessonId = await SeedLesson(courseId);

        var result = await _sut.DeleteLesson(lessonId);

        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task DeleteLesson_WhenNotFound_ReturnsNotFound()
    {
        var result = await _sut.DeleteLesson(9999);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task DeleteLesson_WithNoCompletions_ReturnsNoContent()
    {
        var courseId = await SeedCourse();
        var lessonId = await SeedLesson(courseId);

        var result = await _sut.DeleteLesson(lessonId);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var lesson = await Db.Lessons.FindAsync(lessonId);
        Assert.That(lesson, Is.Null);
    }

    [Test]
    public async Task DeleteLesson_WithCompletions_ReturnsConflict()
    {
        var courseId = await SeedCourse();
        var lessonId = await SeedLesson(courseId);
        Db.LessonStatuses.Add(new LessonStatusEntity { UserId = "learner-1", LessonId = lessonId, Status = LessonStatusValue.Done });
        await Db.SaveChangesAsync();

        var result = await _sut.DeleteLesson(lessonId);

        Assert.That(result, Is.InstanceOf<ConflictResult>());
        var lesson = await Db.Lessons.FindAsync(lessonId);
        Assert.That(lesson, Is.Not.Null);
    }

    [Test]
    public async Task DeleteLesson_WithOnlyLaterStatus_ReturnsNoContent()
    {
        var courseId = await SeedCourse();
        var lessonId = await SeedLesson(courseId);
        Db.LessonStatuses.Add(new LessonStatusEntity { UserId = "learner-1", LessonId = lessonId, Status = LessonStatusValue.Later });
        await Db.SaveChangesAsync();

        var result = await _sut.DeleteLesson(lessonId);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    // --- PUT /api/courses/{courseId}/lessons/reorder ---

    [Test]
    public async Task ReorderLessons_WhenNotManager_ReturnsForbid()
    {
        _user.IsManager.Returns(false);
        var courseId = await SeedCourse();

        var result = await _sut.ReorderLessons(courseId, new ReorderLessonsRequest([1, 2]));

        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task ReorderLessons_UpdatesSortOrders()
    {
        var courseId = await SeedCourse();
        var lesson1Id = await SeedLesson(courseId, "Lesson 1", 1);
        var lesson2Id = await SeedLesson(courseId, "Lesson 2", 2);
        var lesson3Id = await SeedLesson(courseId, "Lesson 3", 3);

        var result = await _sut.ReorderLessons(courseId, new ReorderLessonsRequest([lesson3Id, lesson1Id, lesson2Id]));

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var lesson3 = await Db.Lessons.FindAsync(lesson3Id);
        var lesson1 = await Db.Lessons.FindAsync(lesson1Id);
        var lesson2 = await Db.Lessons.FindAsync(lesson2Id);
        Assert.That(lesson3!.SortOrder, Is.EqualTo(1));
        Assert.That(lesson1!.SortOrder, Is.EqualTo(2));
        Assert.That(lesson2!.SortOrder, Is.EqualTo(3));
    }

    // --- GET /api/lessons/{id} ---

    [Test]
    public async Task GetLesson_WhenNotFound_ReturnsNotFound()
    {
        var result = await _sut.GetLesson(9999);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task GetLesson_WhenFound_ReturnsLesson()
    {
        var courseId = await SeedCourse();
        var lessonId = await SeedLesson(courseId, "Find Me", 1, 20);

        var result = await _sut.GetLesson(lessonId);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dto = ok!.Value as LessonDto;
        Assert.That(dto!.Title, Is.EqualTo("Find Me"));
        Assert.That(dto.EstimatedDuration, Is.EqualTo(20));
    }
}
