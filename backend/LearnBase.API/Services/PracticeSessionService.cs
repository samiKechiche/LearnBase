using Microsoft.EntityFrameworkCore;
using LearnBase.API.Data;
using LearnBase.API.DTOs.PracticeSession;
using LearnBase.API.DTOs.Shared;
using LearnBase.API.Models;
using LearnBase.API.Models.Enums;

namespace LearnBase.API.Services;

/// <summary>
/// Service layer for Practice Session business logic.
/// Handles starting sessions, submitting answers, skipping, ending sessions,
/// and retrieving session history with statistics.
/// </summary>
public class PracticeSessionService
{
    private readonly ApplicationDbContext _context;

    public PracticeSessionService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ================================================================
    // START SESSION
    // ================================================================

    /// <summary>
    /// Starts a new practice session for a practice set.
    /// Loads all exercises from the set, creates snapshot records.
    /// </summary>
    public async Task<ApiResponseDto<SessionResponseDto>> StartSessionAsync(
        StartSessionDto dto, Guid userId)
    {
        // 1. Verify the practice set exists and belongs to user
        var practiceSet = await _context.PracticeSets
            .Include(ps => ps.PracticeSetExercises)
                .ThenInclude(pse => pse.Exercise)
                    .ThenInclude(e => e.ExerciseOptions)
            .FirstOrDefaultAsync(ps =>
                ps.PracticeSetId == dto.PracticeSetId &&
                ps.UserId == userId);

        if (practiceSet == null)
        {
            return ApiResponseDto<SessionResponseDto>.ErrorResponse("Practice set not found");
        }

        // 2. Check if the practice set has exercises
        if (practiceSet.PracticeSetExercises == null ||
            !practiceSet.PracticeSetExercises.Any())
        {
            return ApiResponseDto<SessionResponseDto>.ErrorResponse(
                "This practice set has no exercises. Add exercises before starting a session.");
        }

        // 3. Create the session
        var session = new PracticeSession
        {
            SessionId = Guid.NewGuid(),
            PracticeSetId = dto.PracticeSetId,
            PracticeOrder = dto.PracticeOrder,
            UserId = userId,
            StartedAt = DateTime.UtcNow
        };

        _context.PracticeSessions.Add(session);

        // 4. Get exercises ordered according to PracticeOrder
        var exercises = practiceSet.PracticeSetExercises
            .OrderBy(pse => pse.OrderIndex)
            .Select(pse => pse.Exercise)
            .Where(e => e != null)
            .ToList();

        // 5. If randomized, shuffle the exercises
        if (dto.PracticeOrder == PracticeOrder.Randomized)
        {
            var random = new Random();
            exercises = exercises.OrderBy(_ => random.Next()).ToList();
        }

        // 6. Create snapshot result records for each exercise
        int orderIndex = 0;
        foreach (var exercise in exercises)
        {
            if (exercise == null) continue;

            // Determine the correct answer snapshot
            string correctAnswerSnapshot = exercise.Answer;

            // For MCQ: store the correct option's content as the correct answer
            if (exercise.Type == ExerciseType.MCQ && exercise.CorrectOptionId.HasValue)
            {
                var correctOption = exercise.ExerciseOptions
                    ?.FirstOrDefault(o => o.OptionId == exercise.CorrectOptionId.Value);

                if (correctOption != null)
                {
                    correctAnswerSnapshot = correctOption.Content;
                }
            }

            _context.SessionExerciseResults.Add(new SessionExerciseResult
            {
                SessionExerciseResultId = Guid.NewGuid(),
                SessionId = session.SessionId,
                ExerciseId = exercise.ExerciseId,
                ExerciseSnapshotQuestion = exercise.Question,
                CorrectAnswerSnapshot = correctAnswerSnapshot,
                ResultStatus = ResultStatus.Skipped, // Default until answered
                OrderIndex = orderIndex++,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        // 7. Return the session with data
        return ApiResponseDto<SessionResponseDto>.SuccessResponse(
            await MapToResponseDto(session),
            "Practice session started successfully");
    }

    // ================================================================
    // SUBMIT ANSWER
    // ================================================================

    /// <summary>
    /// Submits an answer for a specific exercise in an active session.
    /// Checks correctness and updates the result record.
    /// </summary>
    public async Task<ApiResponseDto<SessionExerciseResultDto>> SubmitAnswerAsync(
        Guid sessionId, SubmitAnswerDto dto, Guid userId)
    {
        // 1. Find the session and verify ownership
        var session = await _context.PracticeSessions
            .Include(s => s.SessionExerciseResults)
            .FirstOrDefaultAsync(s =>
                s.SessionId == sessionId &&
                s.UserId == userId);

        if (session == null)
        {
            return ApiResponseDto<SessionExerciseResultDto>.ErrorResponse("Session not found");
        }

        // 2. Check if session is still active
        if (session.EndedAt.HasValue)
        {
            return ApiResponseDto<SessionExerciseResultDto>.ErrorResponse(
                "This session has already ended. Start a new session.");
        }

        // 3. Find the result record for this exercise
        var result = session.SessionExerciseResults
            .FirstOrDefault(r => r.ExerciseId == dto.ExerciseId);

        if (result == null)
        {
            return ApiResponseDto<SessionExerciseResultDto>.ErrorResponse(
                "Exercise not found in this session");
        }

        // 4. Check if already answered (not skipped = already processed)
        if (result.ResultStatus != ResultStatus.Skipped || result.UserAnswer != null)
        {
            return ApiResponseDto<SessionExerciseResultDto>.ErrorResponse(
                "This exercise has already been answered");
        }

        // 5. Check correctness (case-insensitive comparison)
        bool isCorrect = string.Equals(
            dto.UserAnswer.Trim(),
            result.CorrectAnswerSnapshot.Trim(),
            StringComparison.OrdinalIgnoreCase);

        // 6. Update the result
        result.UserAnswer = dto.UserAnswer.Trim();
        result.ResultStatus = isCorrect ? ResultStatus.Correct : ResultStatus.Incorrect;

        await _context.SaveChangesAsync();

        return ApiResponseDto<SessionExerciseResultDto>.SuccessResponse(
            MapToResultDto(result),
            isCorrect ? "Correct!" : "Incorrect");
    }

    // ================================================================
    // SKIP EXERCISE
    // ================================================================

    /// <summary>
    /// Marks an exercise as skipped in an active session.
    /// </summary>
    public async Task<ApiResponseDto<SessionExerciseResultDto>> SkipExerciseAsync(
        Guid sessionId, SkipExerciseDto dto, Guid userId)
    {
        // 1. Find the session and verify ownership
        var session = await _context.PracticeSessions
            .Include(s => s.SessionExerciseResults)
            .FirstOrDefaultAsync(s =>
                s.SessionId == sessionId &&
                s.UserId == userId);

        if (session == null)
        {
            return ApiResponseDto<SessionExerciseResultDto>.ErrorResponse("Session not found");
        }

        // 2. Check if session is still active
        if (session.EndedAt.HasValue)
        {
            return ApiResponseDto<SessionExerciseResultDto>.ErrorResponse(
                "This session has already ended.");
        }

        // 3. Find the result record
        var result = session.SessionExerciseResults
            .FirstOrDefault(r => r.ExerciseId == dto.ExerciseId);

        if (result == null)
        {
            return ApiResponseDto<SessionExerciseResultDto>.ErrorResponse(
                "Exercise not found in this session");
        }

        // 4. Check if already processed
        if (result.UserAnswer != null)
        {
            return ApiResponseDto<SessionExerciseResultDto>.ErrorResponse(
                "This exercise has already been answered");
        }

        // 5. Mark as skipped
        result.ResultStatus = ResultStatus.Skipped;
        result.UserAnswer = null;

        await _context.SaveChangesAsync();

        return ApiResponseDto<SessionExerciseResultDto>.SuccessResponse(
            MapToResultDto(result),
            "Exercise skipped");
    }

    // ================================================================
    // END SESSION
    // ================================================================

    /// <summary>
    /// Ends an active practice session early.
    /// Any un-answered exercises remain as Skipped.
    /// </summary>
    public async Task<ApiResponseDto<SessionResponseDto>> EndSessionAsync(
        Guid sessionId, Guid userId)
    {
        // 1. Find the session
        var session = await _context.PracticeSessions
            .Include(s => s.SessionExerciseResults)
            .Include(s => s.PracticeSet)
            .FirstOrDefaultAsync(s =>
                s.SessionId == sessionId &&
                s.UserId == userId);

        if (session == null)
        {
            return ApiResponseDto<SessionResponseDto>.ErrorResponse("Session not found");
        }

        // 2. Check if already ended
        if (session.EndedAt.HasValue)
        {
            return ApiResponseDto<SessionResponseDto>.ErrorResponse(
                "This session has already ended.");
        }

        // 3. Set end time
        session.EndedAt = DateTime.UtcNow;

        // 4. Any exercises that were never touched stay as Skipped
        // (they were initialized as Skipped, so nothing to change)

        await _context.SaveChangesAsync();

        return ApiResponseDto<SessionResponseDto>.SuccessResponse(
            await MapToResponseDto(session),
            "Session ended successfully");
    }

    // ================================================================
    // GET SESSIONS (HISTORY)
    // ================================================================

    /// <summary>
    /// Gets all practice sessions for a user, ordered by most recent first.
    /// Optional filter: active only (not ended).
    /// </summary>
    public async Task<ApiResponseDto<List<SessionSummaryDto>>> GetAllSessionsAsync(
        Guid userId, bool? activeOnly = null)
    {
        IQueryable<PracticeSession> query = _context.PracticeSessions
            .Include(s => s.PracticeSet)
            .Include(s => s.SessionExerciseResults)
            .Where(s => s.UserId == userId);

        // Filter: only active sessions
        if (activeOnly.HasValue && activeOnly.Value)
        {
            query = query.Where(s => s.EndedAt == null);
        }

        var sessions = await query
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync();

        var summaries = sessions.Select(s => MapToSummaryDto(s)).ToList();

        return ApiResponseDto<List<SessionSummaryDto>>.SuccessResponse(summaries);
    }

    /// <summary>
    /// Gets a single session with full details including all exercise results.
    /// </summary>
    public async Task<ApiResponseDto<SessionResponseDto>> GetSessionByIdAsync(
        Guid sessionId, Guid userId)
    {
        var session = await _context.PracticeSessions
            .Include(s => s.PracticeSet)
            .Include(s => s.SessionExerciseResults)
            .FirstOrDefaultAsync(s =>
                s.SessionId == sessionId &&
                s.UserId == userId);

        if (session == null)
        {
            return ApiResponseDto<SessionResponseDto>.ErrorResponse("Session not found");
        }

        return ApiResponseDto<SessionResponseDto>.SuccessResponse(
            await MapToResponseDto(session));
    }

    // ================================================================
    // DELETE SESSIONS
    // ================================================================

    /// <summary>
    /// Deletes a single practice session and all its results.
    /// </summary>
    public async Task<ApiResponseDto<bool>> DeleteSessionAsync(
        Guid sessionId, Guid userId)
    {
        var session = await _context.PracticeSessions
            .FirstOrDefaultAsync(s =>
                s.SessionId == sessionId &&
                s.UserId == userId);

        if (session == null)
        {
            return ApiResponseDto<bool>.ErrorResponse("Session not found");
        }

        // Cascade delete will remove all SessionExerciseResults
        _context.PracticeSessions.Remove(session);
        await _context.SaveChangesAsync();

        return ApiResponseDto<bool>.SuccessResponse(true, "Session deleted successfully");
    }

    /// <summary>
    /// Deletes ALL practice sessions for a user (with confirmation check).
    /// </summary>
    public async Task<ApiResponseDto<bool>> DeleteAllSessionsAsync(Guid userId)
    {
        var sessions = await _context.PracticeSessions
            .Where(s => s.UserId == userId)
            .ToListAsync();

        if (!sessions.Any())
        {
            return ApiResponseDto<bool>.ErrorResponse("No sessions to delete");
        }

        _context.PracticeSessions.RemoveRange(sessions);
        await _context.SaveChangesAsync();

        return ApiResponseDto<bool>.SuccessResponse(
            true,
            $"{sessions.Count} session(s) deleted successfully");
    }

    // ================================================================
    // PRIVATE HELPER METHODS
    // ================================================================

    /// <summary>
    /// Maps a PracticeSession entity to a full SessionResponseDto.
    /// Calculates statistics from the results.
    /// </summary>
    private async Task<SessionResponseDto> MapToResponseDto(PracticeSession session)
    {
        // Ensure PracticeSet is loaded
        string practiceSetTitle = session.PracticeSet?.Title ?? "Unknown";

        // If PracticeSet is not loaded, fetch it
        if (session.PracticeSet == null)
        {
            var set = await _context.PracticeSets
                .FirstOrDefaultAsync(ps => ps.PracticeSetId == session.PracticeSetId);
            practiceSetTitle = set?.Title ?? "Unknown";
        }

        // Calculate statistics
        var results = session.SessionExerciseResults ?? new List<SessionExerciseResult>();
        int correct = results.Count(r => r.ResultStatus == ResultStatus.Correct);
        int incorrect = results.Count(r => r.ResultStatus == ResultStatus.Incorrect);
        int skipped = results.Count(r => r.ResultStatus == ResultStatus.Skipped);

        var dto = new SessionResponseDto
        {
            SessionId = session.SessionId,
            PracticeSetId = session.PracticeSetId,
            PracticeSetTitle = practiceSetTitle,
            PracticeOrder = session.PracticeOrder,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            TotalExercises = results.Count,
            CorrectCount = correct,
            IncorrectCount = incorrect,
            SkippedCount = skipped,
            Results = results
                .OrderBy(r => r.OrderIndex)
                .Select(MapToResultDto)
                .ToList()
        };

        return dto;
    }

    /// <summary>
    /// Maps a single SessionExerciseResult entity to a DTO.
    /// </summary>
    private SessionExerciseResultDto MapToResultDto(SessionExerciseResult result)
    {
        return new SessionExerciseResultDto
        {
            SessionExerciseResultId = result.SessionExerciseResultId,
            ExerciseId = result.ExerciseId,
            Question = result.ExerciseSnapshotQuestion,
            UserAnswer = result.UserAnswer,
            CorrectAnswer = result.CorrectAnswerSnapshot,
            ResultStatus = result.ResultStatus,
            OrderIndex = result.OrderIndex
        };
    }

    /// <summary>
    /// Maps a PracticeSession entity to a summary DTO for list views.
    /// </summary>
    private SessionSummaryDto MapToSummaryDto(PracticeSession session)
    {
        var results = session.SessionExerciseResults ?? new List<SessionExerciseResult>();
        int correct = results.Count(r => r.ResultStatus == ResultStatus.Correct);
        int incorrect = results.Count(r => r.ResultStatus == ResultStatus.Incorrect);
        int skipped = results.Count(r => r.ResultStatus == ResultStatus.Skipped);
        int totalAnswered = correct + incorrect;

        double? score = totalAnswered > 0
            ? Math.Round((double)correct / totalAnswered * 100, 1)
            : null;

        return new SessionSummaryDto
        {
            SessionId = session.SessionId,
            PracticeSetTitle = session.PracticeSet?.Title ?? "Unknown",
            PracticeOrder = session.PracticeOrder,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            TotalExercises = results.Count,
            CorrectCount = correct,
            IncorrectCount = incorrect,
            SkippedCount = skipped,
            ScorePercentage = score
        };
    }
}