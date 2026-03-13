using Itenium.SkillForge.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

[ApiController]
[Route("api/quizzes/{quizId:int}/analytics")]
[Authorize]
public class QuizAnalyticsController : ControllerBase
{
    private readonly AppDbContext _db;

    public QuizAnalyticsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get aggregated analytics for a quiz.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<QuizAnalyticsResponse>> GetAnalytics(
        int quizId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var quiz = await _db.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz == null)
        {
            return NotFound();
        }

        var attemptsQuery = _db.QuizAttempts
            .Where(a => a.QuizId == quizId);

        if (dateFrom.HasValue)
        {
            attemptsQuery = attemptsQuery.Where(a => a.CompletedAt >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            attemptsQuery = attemptsQuery.Where(a => a.CompletedAt <= dateTo.Value);
        }

        var attempts = await attemptsQuery
            .Include(a => a.Responses)
            .ToListAsync();

        var totalAttempts = attempts.Count;
        var uniqueLearners = attempts.Select(a => a.UserId).Distinct(StringComparer.Ordinal).Count();
        var averageScore = totalAttempts > 0 ? attempts.Average(a => a.Score) : 0;
        var passRate = totalAttempts > 0 ? attempts.Count(a => a.IsPassed) * 100.0 / totalAttempts : 0;

        var questionStats = quiz.Questions.Select(q =>
        {
            var responses = attempts.SelectMany(a => a.Responses).Where(r => r.QuestionId == q.Id).ToList();
            var correctRate = responses.Count > 0 ? responses.Count(r => r.IsCorrect) * 100.0 / responses.Count : 0;
            return new QuestionStatDto(q.Id, q.Text, correctRate);
        })
        .OrderBy(qs => qs.CorrectRate)
        .ToList();

        var scoreDistribution = BuildScoreDistribution(attempts.Select(a => a.Score).ToList());

        return Ok(new QuizAnalyticsResponse(averageScore, passRate, totalAttempts, uniqueLearners, questionStats, scoreDistribution));
    }

    /// <summary>
    /// Get per-learner analytics for a quiz, optionally filtered by team.
    /// </summary>
    [HttpGet("learners")]
    public async Task<ActionResult<IList<QuizLearnerAnalyticsItem>>> GetLearnerAnalytics(
        int quizId,
        [FromQuery] int? teamId)
    {
        var exists = await _db.Quizzes.AnyAsync(q => q.Id == quizId);
        if (!exists)
        {
            return NotFound();
        }

        var attemptsQuery = _db.QuizAttempts.Where(a => a.QuizId == quizId);

        if (teamId.HasValue)
        {
            attemptsQuery = attemptsQuery.Where(a => a.TeamId == teamId.Value);
        }

        var attempts = await attemptsQuery.ToListAsync();

        var learners = attempts
            .GroupBy(a => a.UserId, StringComparer.Ordinal)
            .Select(g =>
            {
                var latest = g.OrderByDescending(a => a.CompletedAt).First();
                return new QuizLearnerAnalyticsItem(latest.UserId, latest.UserName, latest.Score, latest.IsPassed, latest.CompletedAt);
            })
            .ToList();

        return Ok(learners);
    }

    private static List<ScoreDistributionDto> BuildScoreDistribution(IList<double> scores)
    {
        var buckets = new List<ScoreDistributionDto>();
        for (var i = 0; i < 10; i++)
        {
            var lower = i * 10;
            var upper = (i + 1) * 10;
            var count = scores.Count(s => s >= lower && (upper == 100 ? s <= upper : s < upper));
            buckets.Add(new ScoreDistributionDto($"{lower}-{upper}%", count));
        }

        return buckets;
    }
}

public record QuizAnalyticsResponse(
    double AverageScore,
    double PassRate,
    int TotalAttempts,
    int UniqueLearners,
    IList<QuestionStatDto> QuestionStats,
    IList<ScoreDistributionDto> ScoreDistribution);

public record QuestionStatDto(int QuestionId, string QuestionText, double CorrectRate);

public record ScoreDistributionDto(string Range, int Count);

public record QuizLearnerAnalyticsItem(
    string UserId,
    string UserName,
    double LatestScore,
    bool IsPassed,
    DateTime CompletedAt);
