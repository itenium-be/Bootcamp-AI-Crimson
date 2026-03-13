using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class AnnotationControllerTests : DatabaseTestBase
{
    private ISkillForgeUser _user = null!;
    private AnnotationController _sut = null!;

    private const string LearnerId = "learner-1";
    private const string OtherLearnerId = "learner-2";
    private const string DisplayName = "Alice";

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<ISkillForgeUser>();
        _user.Id.Returns(LearnerId);
        _user.DisplayName.Returns(DisplayName);
        _user.IsBackOffice.Returns(false);
        _sut = new AnnotationController(Db, _user);
    }

    private async Task<(int courseId, int lessonId)> SeedLesson()
    {
        var course = new CourseEntity { Name = "Course", Status = CourseStatus.Published };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();
        var lesson = new LessonEntity { CourseId = course.Id, Title = "Lesson 1", SortOrder = 1 };
        Db.Lessons.Add(lesson);
        await Db.SaveChangesAsync();
        return (course.Id, lesson.Id);
    }

    private async Task<int> SeedAnnotation(int lessonId, string userId = LearnerId, string content = "My note")
    {
        var ann = new LessonAnnotationEntity
        {
            UserId = userId,
            DisplayName = userId == LearnerId ? DisplayName : "Bob",
            LessonId = lessonId,
            Content = content,
        };
        Db.LessonAnnotations.Add(ann);
        await Db.SaveChangesAsync();
        return ann.Id;
    }

    // ---- POST /api/lessons/{id}/annotations ----

    [Test]
    public async Task CreateAnnotation_WhenLessonNotFound_ReturnsNotFound()
    {
        var result = await _sut.CreateAnnotation(999, new CreateAnnotationRequest("My note", null));

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task CreateAnnotation_WhenRatingOutOfRange_ReturnsBadRequest()
    {
        var (_, lessonId) = await SeedLesson();

        var result = await _sut.CreateAnnotation(lessonId, new CreateAnnotationRequest("Note", 6));

        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateAnnotation_WithoutRating_CreatesEntry()
    {
        var (_, lessonId) = await SeedLesson();

        var result = await _sut.CreateAnnotation(lessonId, new CreateAnnotationRequest("My experience", null));

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var row = await Db.LessonAnnotations.SingleOrDefaultAsync(a => a.UserId == LearnerId && a.LessonId == lessonId);
        Assert.That(row, Is.Not.Null);
        Assert.That(row!.Content, Is.EqualTo("My experience"));
        Assert.That(row.Rating, Is.Null);
        Assert.That(row.DisplayName, Is.EqualTo(DisplayName));
    }

    [Test]
    public async Task CreateAnnotation_WithRating_StoresRating()
    {
        var (_, lessonId) = await SeedLesson();

        var result = await _sut.CreateAnnotation(lessonId, new CreateAnnotationRequest("Great content!", 5));

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var row = await Db.LessonAnnotations.SingleAsync(a => a.UserId == LearnerId && a.LessonId == lessonId);
        Assert.That(row.Rating, Is.EqualTo(5));
    }

    // ---- GET /api/lessons/{id}/annotations ----

    [Test]
    public async Task GetAnnotations_ReturnsListForLesson()
    {
        var (_, lessonId) = await SeedLesson();
        await SeedAnnotation(lessonId, LearnerId, "Note 1");
        await SeedAnnotation(lessonId, OtherLearnerId, "Note 2");

        var result = await _sut.GetAnnotations(lessonId);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dto = ok!.Value as AnnotationsPageDto;
        Assert.That(dto!.Items, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetAnnotations_ShowsDisplayName_NotUserId()
    {
        var (_, lessonId) = await SeedLesson();
        await SeedAnnotation(lessonId);

        var result = await _sut.GetAnnotations(lessonId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as AnnotationsPageDto;
        var item = dto!.Items[0];
        Assert.That(item.DisplayName, Is.EqualTo(DisplayName));
        Assert.That(item.DisplayName, Does.Not.Contain(LearnerId));
    }

    [Test]
    public async Task GetAnnotations_IsOwn_TrueForCurrentUser()
    {
        var (_, lessonId) = await SeedLesson();
        await SeedAnnotation(lessonId, LearnerId);
        await SeedAnnotation(lessonId, OtherLearnerId);

        var result = await _sut.GetAnnotations(lessonId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as AnnotationsPageDto;
        var own = dto!.Items.Single(i => i.IsOwn);
        Assert.That(own.DisplayName, Is.EqualTo(DisplayName));
    }

    [Test]
    public async Task GetAnnotations_Pagination_ReturnsCorrectPage()
    {
        var (_, lessonId) = await SeedLesson();
        for (var i = 0; i < 5; i++)
            await SeedAnnotation(lessonId, LearnerId, $"Note {i}");

        var result = await _sut.GetAnnotations(lessonId, page: 1, pageSize: 3);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as AnnotationsPageDto;
        Assert.That(dto!.Items, Has.Count.EqualTo(3));
        Assert.That(dto.TotalCount, Is.EqualTo(5));
    }

    // ---- PUT /api/annotations/{id} ----

    [Test]
    public async Task UpdateAnnotation_OwnAnnotation_UpdatesContent()
    {
        var (_, lessonId) = await SeedLesson();
        var annId = await SeedAnnotation(lessonId);

        var result = await _sut.UpdateAnnotation(annId, new UpdateAnnotationRequest("Updated note", 4));

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        var row = await Db.LessonAnnotations.FindAsync(annId);
        Assert.That(row!.Content, Is.EqualTo("Updated note"));
        Assert.That(row.Rating, Is.EqualTo(4));
    }

    [Test]
    public async Task UpdateAnnotation_OtherUsersAnnotation_ReturnsForbid()
    {
        var (_, lessonId) = await SeedLesson();
        var annId = await SeedAnnotation(lessonId, OtherLearnerId);

        var result = await _sut.UpdateAnnotation(annId, new UpdateAnnotationRequest("Hacked", null));

        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task UpdateAnnotation_WhenNotFound_ReturnsNotFound()
    {
        var result = await _sut.UpdateAnnotation(999, new UpdateAnnotationRequest("x", null));

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    // ---- DELETE /api/annotations/{id} ----

    [Test]
    public async Task DeleteAnnotation_OwnAnnotation_Deletes()
    {
        var (_, lessonId) = await SeedLesson();
        var annId = await SeedAnnotation(lessonId);

        var result = await _sut.DeleteAnnotation(annId);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        Assert.That(await Db.LessonAnnotations.FindAsync(annId), Is.Null);
    }

    [Test]
    public async Task DeleteAnnotation_OtherUsersAnnotation_ReturnsForbid()
    {
        var (_, lessonId) = await SeedLesson();
        var annId = await SeedAnnotation(lessonId, OtherLearnerId);

        var result = await _sut.DeleteAnnotation(annId);

        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task DeleteAnnotation_BackofficeCanDeleteAny()
    {
        _user.IsBackOffice.Returns(true);
        var (_, lessonId) = await SeedLesson();
        var annId = await SeedAnnotation(lessonId, OtherLearnerId);

        var result = await _sut.DeleteAnnotation(annId);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        Assert.That(await Db.LessonAnnotations.FindAsync(annId), Is.Null);
    }

    [Test]
    public async Task DeleteAnnotation_WhenNotFound_ReturnsNotFound()
    {
        var result = await _sut.DeleteAnnotation(999);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
}
