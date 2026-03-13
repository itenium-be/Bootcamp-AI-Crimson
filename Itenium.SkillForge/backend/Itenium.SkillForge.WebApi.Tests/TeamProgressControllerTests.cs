using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class TeamProgressControllerTests : DatabaseTestBase
{
    private IUserRepository _userRepository = null!;
    private ISkillForgeUser _user = null!;
    private TeamController _sut = null!;

    private const int TeamId = 1;
    private const string MemberId1 = "member-1";
    private const string MemberId2 = "member-2";

    [SetUp]
    public void Setup()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _user = Substitute.For<ISkillForgeUser>();
        _sut = new TeamController(Db, _user, _userRepository);

        _user.IsBackOffice.Returns(false);
        _user.Teams.Returns(new List<int> { TeamId });
    }

    private async Task<int> SeedCourse(string name = "Course")
    {
        var course = new CourseEntity { Name = name, Status = CourseStatus.Published };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();
        return course.Id;
    }

    private async Task<int> SeedLesson(int courseId, string title = "Lesson")
    {
        var lesson = new LessonEntity { CourseId = courseId, Title = title, SortOrder = 1 };
        Db.Lessons.Add(lesson);
        await Db.SaveChangesAsync();
        return lesson.Id;
    }

    private async Task SeedEnrollment(string userId, int courseId)
    {
        Db.Enrollments.Add(new EnrollmentEntity { UserId = userId, CourseId = courseId });
        await Db.SaveChangesAsync();
    }

    private async Task SeedProgress(string userId, int lessonId)
    {
        Db.LessonProgresses.Add(new LessonProgressEntity { UserId = userId, LessonId = lessonId });
        await Db.SaveChangesAsync();
    }

    private async Task SeedMandatoryTeamAssignment(int courseId)
    {
        Db.CourseAssignments.Add(new CourseAssignmentEntity
        {
            CourseId = courseId,
            AssigneeType = AssigneeType.Team,
            AssigneeId = TeamId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Type = AssignmentType.Mandatory,
            AssignedBy = "admin",
        });
        await Db.SaveChangesAsync();
    }

    private void SetupMembers(params string[] memberIds)
    {
        var members = memberIds
            .Select((id, i) => new UserResponse(id, $"User {i + 1}", $"user{i + 1}@test.com", "learner", true))
            .ToList();
        _userRepository.GetTeamMembersAsync(TeamId).Returns(members);
    }

    // --- GET /api/team/{id}/progress ---

    [Test]
    public async Task GetTeamProgress_WhenNotManagerOfTeam_ReturnsForbid()
    {
        _user.Teams.Returns(new List<int> { 99 });

        var result = await _sut.GetTeamProgress(TeamId);

        Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task GetTeamProgress_WhenBackOffice_ReturnsOk()
    {
        _user.IsBackOffice.Returns(true);
        _user.Teams.Returns(new List<int>());
        SetupMembers();

        var result = await _sut.GetTeamProgress(TeamId);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetTeamProgress_NoMembers_ReturnsEmptyList()
    {
        SetupMembers();

        var result = await _sut.GetTeamProgress(TeamId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as TeamProgressDto;
        Assert.That(dto!.Members, Is.Empty);
    }

    [Test]
    public async Task GetTeamProgress_AllLessonsCompleted_CourseIsCompleted()
    {
        SetupMembers(MemberId1);
        var courseId = await SeedCourse();
        var lessonId = await SeedLesson(courseId);
        await SeedEnrollment(MemberId1, courseId);
        await SeedProgress(MemberId1, lessonId);

        var result = await _sut.GetTeamProgress(TeamId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as TeamProgressDto;
        var member = dto!.Members.Single();
        Assert.That(member.EnrolledCourses, Is.EqualTo(1));
        Assert.That(member.CompletedCourses, Is.EqualTo(1));
        Assert.That(member.OverallPercent, Is.EqualTo(100.0));
    }

    [Test]
    public async Task GetTeamProgress_PartialLessons_NotCompleted()
    {
        SetupMembers(MemberId1);
        var courseId = await SeedCourse();
        await SeedLesson(courseId);
        var lesson2Id = await SeedLesson(courseId);
        await SeedEnrollment(MemberId1, courseId);
        await SeedProgress(MemberId1, lesson2Id);

        var result = await _sut.GetTeamProgress(TeamId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as TeamProgressDto;
        var member = dto!.Members.Single();
        Assert.That(member.CompletedCourses, Is.EqualTo(0));
        Assert.That(member.OverallPercent, Is.EqualTo(50.0));
    }

    [Test]
    public async Task GetTeamProgress_NoEnrollments_ZeroStats()
    {
        SetupMembers(MemberId1);

        var result = await _sut.GetTeamProgress(TeamId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as TeamProgressDto;
        var member = dto!.Members.Single();
        Assert.That(member.EnrolledCourses, Is.EqualTo(0));
        Assert.That(member.CompletedCourses, Is.EqualTo(0));
        Assert.That(member.OverallPercent, Is.EqualTo(0.0));
    }

    [Test]
    public async Task GetTeamProgress_MandatoryNotCompleted_IsOverdue()
    {
        SetupMembers(MemberId1);
        var courseId = await SeedCourse();
        await SeedLesson(courseId);
        await SeedEnrollment(MemberId1, courseId);
        await SeedMandatoryTeamAssignment(courseId);

        var result = await _sut.GetTeamProgress(TeamId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as TeamProgressDto;
        var course = dto!.Members.Single().Courses.Single();
        Assert.That(course.IsMandatory, Is.True);
        Assert.That(course.IsOverdue, Is.True);
    }

    [Test]
    public async Task GetTeamProgress_MandatoryCompleted_IsNotOverdue()
    {
        SetupMembers(MemberId1);
        var courseId = await SeedCourse();
        var lessonId = await SeedLesson(courseId);
        await SeedEnrollment(MemberId1, courseId);
        await SeedProgress(MemberId1, lessonId);
        await SeedMandatoryTeamAssignment(courseId);

        var result = await _sut.GetTeamProgress(TeamId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as TeamProgressDto;
        var course = dto!.Members.Single().Courses.Single();
        Assert.That(course.IsMandatory, Is.True);
        Assert.That(course.IsOverdue, Is.False);
    }

    [Test]
    public async Task GetTeamProgress_MultipleMembers_EachHasOwnStats()
    {
        SetupMembers(MemberId1, MemberId2);
        var courseId = await SeedCourse();
        var lessonId = await SeedLesson(courseId);
        await SeedEnrollment(MemberId1, courseId);
        await SeedEnrollment(MemberId2, courseId);
        await SeedProgress(MemberId1, lessonId);
        // MemberId2 has not completed

        var result = await _sut.GetTeamProgress(TeamId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as TeamProgressDto;
        Assert.That(dto!.Members, Has.Count.EqualTo(2));
        var m1 = dto.Members.Single(m => m.UserId == MemberId1);
        var m2 = dto.Members.Single(m => m.UserId == MemberId2);
        Assert.That(m1.CompletedCourses, Is.EqualTo(1));
        Assert.That(m2.CompletedCourses, Is.EqualTo(0));
    }

    // --- GET /api/team/{id}/courses/{courseId}/progress ---

    [Test]
    public async Task GetCourseProgress_WhenNotManagerOfTeam_ReturnsForbid()
    {
        _user.Teams.Returns(new List<int> { 99 });

        var result = await _sut.GetCourseProgress(TeamId, 1);

        Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task GetCourseProgress_NotEnrolled_ReturnsNotStarted()
    {
        SetupMembers(MemberId1);
        var courseId = await SeedCourse();

        var result = await _sut.GetCourseProgress(TeamId, courseId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as CourseMemberProgressDto;
        var member = dto!.Members.Single();
        Assert.That(member.Status, Is.EqualTo("NotStarted"));
        Assert.That(member.CompletedLessons, Is.EqualTo(0));
    }

    [Test]
    public async Task GetCourseProgress_AllLessonsDone_ReturnsCompleted()
    {
        SetupMembers(MemberId1);
        var courseId = await SeedCourse();
        var lessonId = await SeedLesson(courseId);
        await SeedEnrollment(MemberId1, courseId);
        await SeedProgress(MemberId1, lessonId);

        var result = await _sut.GetCourseProgress(TeamId, courseId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as CourseMemberProgressDto;
        var member = dto!.Members.Single();
        Assert.That(member.Status, Is.EqualTo("Completed"));
        Assert.That(member.CompletedLessons, Is.EqualTo(1));
        Assert.That(member.PercentComplete, Is.EqualTo(100.0));
    }

    [Test]
    public async Task GetCourseProgress_SomeLessonsDone_ReturnsInProgress()
    {
        SetupMembers(MemberId1);
        var courseId = await SeedCourse();
        await SeedLesson(courseId);
        var lesson2Id = await SeedLesson(courseId);
        await SeedEnrollment(MemberId1, courseId);
        await SeedProgress(MemberId1, lesson2Id);

        var result = await _sut.GetCourseProgress(TeamId, courseId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as CourseMemberProgressDto;
        var member = dto!.Members.Single();
        Assert.That(member.Status, Is.EqualTo("InProgress"));
        Assert.That(member.CompletedLessons, Is.EqualTo(1));
        Assert.That(member.PercentComplete, Is.EqualTo(50.0));
    }

    [Test]
    public async Task GetCourseProgress_EnrolledNoProgress_ReturnsInProgress()
    {
        SetupMembers(MemberId1);
        var courseId = await SeedCourse();
        await SeedLesson(courseId);
        await SeedEnrollment(MemberId1, courseId);

        var result = await _sut.GetCourseProgress(TeamId, courseId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as CourseMemberProgressDto;
        var member = dto!.Members.Single();
        Assert.That(member.Status, Is.EqualTo("InProgress"));
    }

    [Test]
    public async Task GetCourseProgress_MandatoryNotCompleted_IsOverdue()
    {
        SetupMembers(MemberId1);
        var courseId = await SeedCourse();
        await SeedLesson(courseId);
        await SeedEnrollment(MemberId1, courseId);
        await SeedMandatoryTeamAssignment(courseId);

        var result = await _sut.GetCourseProgress(TeamId, courseId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as CourseMemberProgressDto;
        var member = dto!.Members.Single();
        Assert.That(member.IsMandatory, Is.True);
        Assert.That(member.IsOverdue, Is.True);
    }

    [Test]
    public async Task GetCourseProgress_ReturnsCourseName()
    {
        SetupMembers(MemberId1);
        var courseId = await SeedCourse("Advanced C#");

        var result = await _sut.GetCourseProgress(TeamId, courseId);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as CourseMemberProgressDto;
        Assert.That(dto!.CourseName, Is.EqualTo("Advanced C#"));
        Assert.That(dto.TotalLessons, Is.EqualTo(0));
    }
}
