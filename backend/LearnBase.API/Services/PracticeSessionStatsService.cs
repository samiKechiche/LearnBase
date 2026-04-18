using Microsoft.EntityFrameworkCore;
using LearnBase.API.Data;
using LearnBase.API.DTOs.PracticeSession;
using LearnBase.API.DTOs.Shared;
using LearnBase.API.Models.Enums;

namespace LearnBase.API.Services;

/// <summary>
/// Service for calculating overall practice statistics.
/// Provides aggregated performance data across all sessions.
/// </summary>
public class PracticeSessionStatsService
{
    private readonly ApplicationDbContext _context;

    public PracticeSessionStatsService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets overall statistics for all practice sessions of a user.
    /// </summary>
    public async Task<ApiResponseDto<PracticeStatsDto>> GetStatsAsync(Guid userId)
    {
        // Get all sessions with their results
        var sessions = await _context.PracticeSessions
            .Include(s => s.SessionExerciseResults)
            .Include(s => s.PracticeSet)
            .Where(s => s.UserId == userId && s.EndedAt != null) // Only completed sessions
            .ToListAsync();

        if (!sessions.Any())
        {
            return ApiResponseDto<PracticeStatsDto>.SuccessResponse(
                new PracticeStatsDto { Message = "No completed sessions yet. Start practicing!" });
        }

        int totalSessions = sessions.Count;
        int totalExercises = sessions.Sum(s => s.SessionExerciseResults?.Count ?? 0);
        int correct = sessions.Sum(s => s.SessionExerciseResults?.Count(r => r.ResultStatus == ResultStatus.Correct) ?? 0);
        int incorrect = sessions.Sum(s => s.SessionExerciseResults?.Count(r => r.ResultStatus == ResultStatus.Incorrect) ?? 0);
        int skipped = sessions.Sum(s => s.SessionExerciseResults?.Count(r => r.ResultStatus == ResultStatus.Skipped) ?? 0);

        double? overallAccuracy = (correct + incorrect) > 0
            ? Math.Round((double)correct / (correct + incorrect) * 100, 1)
            : null;

        // Find best practice set
        var setScores = sessions
            .Where(s => s.PracticeSet != null)
            .GroupBy(s => s.PracticeSet!.Title)
            .Select(g => new
            {
                Title = g.Key,
                AvgScore = g.Average(s =>
                {
                    var results = s.SessionExerciseResults ?? new List<Models.SessionExerciseResult>();
                    var c = results.Count(r => r.ResultStatus == ResultStatus.Correct);
                    var total = results.Count(r => r.ResultStatus == ResultStatus.Correct || r.ResultStatus == ResultStatus.Incorrect);
                    return total > 0 ? (double)c / total * 100 : 0;
                }),
                Count = g.Count()
            })
            .OrderByDescending(x => x.AvgScore)
            .FirstOrDefault();

        var stats = new PracticeStatsDto
        {
            TotalCompletedSessions = totalSessions,
            TotalExercisesPracticed = totalExercises,
            TotalCorrect = correct,
            TotalIncorrect = incorrect,
            TotalSkipped = skipped,
            OverallAccuracyPercentage = overallAccuracy,
            BestPracticeSetName = setScores?.Title,
            BestPracticeSetAccuracy = setScores != null ? Math.Round(setScores.AvgScore, 1) : null,
            AverageExercisesPerSession = Math.Round((double)totalExercises / totalSessions, 1),
            LastPracticedAt = sessions.Max(s => s.EndedAt)
        };

        return ApiResponseDto<PracticeStatsDto>.SuccessResponse(stats);
    }
}