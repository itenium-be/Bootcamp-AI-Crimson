using Itenium.SkillForge.Entities;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class QuizAnalyticsControllerTests : DatabaseTestBase
{
    private QuizAnalyticsController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _sut = new QuizAnalyticsController(Db);
    }

    private async Task<QuizEntity> CreateQuizWithQuestions(string name = "Test Quiz", double passScore = 60)
    {
        var quiz = new QuizEntity
        {
            Name = name,
            PassScore = passScore,
            Questions =
            [
                new QuestionEntity { Text = "Question 1", Order = 1 },
                new QuestionEntity { Text = "Question 2", Order = 2 },
            ]
        };
        Db.Quizzes.Add(quiz);
        await Db.SaveChangesAsync();
        return quiz;
    }

    [Test]
    public async Task GetAnalytics_WhenQuizNotFound_ReturnsNotFound()
    {
        var result = await _sut.GetAnalytics(999, null, null);
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task GetAnalytics_WithNoAttempts_ReturnsZeroStats()
    {
        var quiz = await CreateQuizWithQuestions();

        var result = await _sut.GetAnalytics(quiz.Id, null, null);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var analytics = okResult!.Value as QuizAnalyticsResponse;
        Assert.That(analytics!.AverageScore, Is.EqualTo(0));
        Assert.That(analytics.PassRate, Is.EqualTo(0));
        Assert.That(analytics.TotalAttempts, Is.EqualTo(0));
        Assert.That(analytics.UniqueLearners, Is.EqualTo(0));
        Assert.That(analytics.QuestionStats, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetAnalytics_WithAttempts_ReturnsCorrectAggregates()
    {
        var quiz = await CreateQuizWithQuestions(passScore: 60);
        var q1 = quiz.Questions[0];
        var q2 = quiz.Questions[1];

        Db.QuizAttempts.AddRange(
            new QuizAttemptEntity
            {
                QuizId = quiz.Id,
                UserId = "user1",
                UserName = "Alice",
                Score = 80,
                IsPassed = true,
                Responses =
                [
                    new QuestionResponseEntity { QuestionId = q1.Id, IsCorrect = true },
                    new QuestionResponseEntity { QuestionId = q2.Id, IsCorrect = false },
                ]
            },
            new QuizAttemptEntity
            {
                QuizId = quiz.Id,
                UserId = "user2",
                UserName = "Bob",
                Score = 40,
                IsPassed = false,
                Responses =
                [
                    new QuestionResponseEntity { QuestionId = q1.Id, IsCorrect = false },
                    new QuestionResponseEntity { QuestionId = q2.Id, IsCorrect = false },
                ]
            });
        await Db.SaveChangesAsync();

        var result = await _sut.GetAnalytics(quiz.Id, null, null);

        var okResult = result.Result as OkObjectResult;
        var analytics = okResult!.Value as QuizAnalyticsResponse;
        Assert.That(analytics!.AverageScore, Is.EqualTo(60).Within(0.01));
        Assert.That(analytics.PassRate, Is.EqualTo(50).Within(0.01));
        Assert.That(analytics.TotalAttempts, Is.EqualTo(2));
        Assert.That(analytics.UniqueLearners, Is.EqualTo(2));
    }

    [Test]
    public async Task GetAnalytics_SameUserMultipleAttempts_CountsUniqueLearnersOnce()
    {
        var quiz = await CreateQuizWithQuestions();

        Db.QuizAttempts.AddRange(
            new QuizAttemptEntity { QuizId = quiz.Id, UserId = "user1", UserName = "Alice", Score = 70, IsPassed = true },
            new QuizAttemptEntity { QuizId = quiz.Id, UserId = "user1", UserName = "Alice", Score = 80, IsPassed = true });
        await Db.SaveChangesAsync();

        var result = await _sut.GetAnalytics(quiz.Id, null, null);

        var analytics = (result.Result as OkObjectResult)!.Value as QuizAnalyticsResponse;
        Assert.That(analytics!.TotalAttempts, Is.EqualTo(2));
        Assert.That(analytics.UniqueLearners, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAnalytics_QuestionStats_SortedByMostMissed()
    {
        var quiz = await CreateQuizWithQuestions();
        var q1 = quiz.Questions[0]; // all wrong → 0% correct
        var q2 = quiz.Questions[1]; // all correct → 100% correct

        Db.QuizAttempts.Add(new QuizAttemptEntity
        {
            QuizId = quiz.Id,
            UserId = "user1",
            UserName = "Alice",
            Score = 50,
            IsPassed = false,
            Responses =
            [
                new QuestionResponseEntity { QuestionId = q1.Id, IsCorrect = false },
                new QuestionResponseEntity { QuestionId = q2.Id, IsCorrect = true },
            ]
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetAnalytics(quiz.Id, null, null);

        var analytics = (result.Result as OkObjectResult)!.Value as QuizAnalyticsResponse;
        var stats = analytics!.QuestionStats;
        Assert.That(stats[0].QuestionId, Is.EqualTo(q1.Id), "Most missed question should be first");
        Assert.That(stats[0].CorrectRate, Is.EqualTo(0).Within(0.01));
        Assert.That(stats[1].QuestionId, Is.EqualTo(q2.Id));
        Assert.That(stats[1].CorrectRate, Is.EqualTo(100).Within(0.01));
    }

    [Test]
    public async Task GetAnalytics_ScoreDistribution_BucketsCorrectly()
    {
        var quiz = await CreateQuizWithQuestions();

        Db.QuizAttempts.AddRange(
            new QuizAttemptEntity { QuizId = quiz.Id, UserId = "u1", UserName = "U1", Score = 5, IsPassed = false },
            new QuizAttemptEntity { QuizId = quiz.Id, UserId = "u2", UserName = "U2", Score = 55, IsPassed = false },
            new QuizAttemptEntity { QuizId = quiz.Id, UserId = "u3", UserName = "U3", Score = 95, IsPassed = true });
        await Db.SaveChangesAsync();

        var result = await _sut.GetAnalytics(quiz.Id, null, null);

        var analytics = (result.Result as OkObjectResult)!.Value as QuizAnalyticsResponse;
        Assert.That(analytics!.ScoreDistribution, Has.Count.EqualTo(10));
        Assert.That(analytics.ScoreDistribution.Single(b => b.Range == "0-10%").Count, Is.EqualTo(1));
        Assert.That(analytics.ScoreDistribution.Single(b => b.Range == "50-60%").Count, Is.EqualTo(1));
        Assert.That(analytics.ScoreDistribution.Single(b => b.Range == "90-100%").Count, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAnalytics_FilterByDateRange_OnlyIncludesMatchingAttempts()
    {
        var quiz = await CreateQuizWithQuestions();
        var baseDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        Db.QuizAttempts.AddRange(
            new QuizAttemptEntity { QuizId = quiz.Id, UserId = "u1", UserName = "U1", Score = 80, IsPassed = true, CompletedAt = baseDate },
            new QuizAttemptEntity { QuizId = quiz.Id, UserId = "u2", UserName = "U2", Score = 40, IsPassed = false, CompletedAt = baseDate.AddDays(10) });
        await Db.SaveChangesAsync();

        var result = await _sut.GetAnalytics(quiz.Id, baseDate.AddDays(-1), baseDate.AddDays(5));

        var analytics = (result.Result as OkObjectResult)!.Value as QuizAnalyticsResponse;
        Assert.That(analytics!.TotalAttempts, Is.EqualTo(1));
        Assert.That(analytics.AverageScore, Is.EqualTo(80).Within(0.01));
    }

    [Test]
    public async Task GetLearnerAnalytics_WhenQuizNotFound_ReturnsNotFound()
    {
        var result = await _sut.GetLearnerAnalytics(999, null);
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task GetLearnerAnalytics_ReturnsLatestAttemptPerLearner()
    {
        var quiz = await CreateQuizWithQuestions();

        Db.QuizAttempts.AddRange(
            new QuizAttemptEntity
            {
                QuizId = quiz.Id,
                UserId = "user1",
                UserName = "Alice",
                Score = 50,
                IsPassed = false,
                CompletedAt = DateTime.UtcNow.AddHours(-2)
            },
            new QuizAttemptEntity
            {
                QuizId = quiz.Id,
                UserId = "user1",
                UserName = "Alice",
                Score = 90,
                IsPassed = true,
                CompletedAt = DateTime.UtcNow
            },
            new QuizAttemptEntity
            {
                QuizId = quiz.Id,
                UserId = "user2",
                UserName = "Bob",
                Score = 30,
                IsPassed = false,
                CompletedAt = DateTime.UtcNow
            });
        await Db.SaveChangesAsync();

        var result = await _sut.GetLearnerAnalytics(quiz.Id, null);

        var okResult = result.Result as OkObjectResult;
        var learners = okResult!.Value as List<QuizLearnerAnalyticsItem>;
        Assert.That(learners, Has.Count.EqualTo(2));
        var alice = learners!.Single(l => l.UserId == "user1");
        Assert.That(alice.LatestScore, Is.EqualTo(90).Within(0.01));
        Assert.That(alice.IsPassed, Is.True);
    }

    [Test]
    public async Task GetLearnerAnalytics_FilterByTeam_OnlyReturnsTeamMembers()
    {
        var quiz = await CreateQuizWithQuestions();

        Db.QuizAttempts.AddRange(
            new QuizAttemptEntity { QuizId = quiz.Id, UserId = "u1", UserName = "Alice", Score = 80, IsPassed = true, TeamId = 1 },
            new QuizAttemptEntity { QuizId = quiz.Id, UserId = "u2", UserName = "Bob", Score = 60, IsPassed = true, TeamId = 2 },
            new QuizAttemptEntity { QuizId = quiz.Id, UserId = "u3", UserName = "Charlie", Score = 40, IsPassed = false, TeamId = null });
        await Db.SaveChangesAsync();

        var result = await _sut.GetLearnerAnalytics(quiz.Id, 1);

        var learners = (result.Result as OkObjectResult)!.Value as List<QuizLearnerAnalyticsItem>;
        Assert.That(learners, Has.Count.EqualTo(1));
        Assert.That(learners![0].UserName, Is.EqualTo("Alice"));
    }
}
