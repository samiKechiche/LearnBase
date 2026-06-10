using LearnBase.API.Models.Enums;

namespace LearnBase.API.DTOs.PracticeSession;

/// <summary>
/// DTO for returning a practice session with all its details
/// </summary>
public class SessionResponseDto
{
    public Guid SessionId { get; set; }
    public Guid PracticeSetId { get; set; }
    public string PracticeSetTitle { get; set; } = string.Empty;
    public PracticeOrder PracticeOrder { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    /// <summary>Whether the session is still active (not ended)</summary>
    public bool IsActive => !EndedAt.HasValue;

    // Statistics
    public int TotalExercises { get; set; }
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public int SkippedCount { get; set; }
    public int AnsweredCount => CorrectCount + IncorrectCount;

    /// <summary>Percentage score (0-100). Null if no exercises answered.</summary>
    public double? ScorePercentage
    {
        get
        {
            var totalAnswered = CorrectCount + IncorrectCount;
            if (totalAnswered == 0) return null;
            return Math.Round((double)CorrectCount / totalAnswered * 100, 1);
        }
    }

    /// <summary>Individual results for each exercise</summary>
    public List<SessionExerciseResultDto> Results { get; set; } = new();
}

/// <summary>
/// DTO for a single exercise result within a session
/// </summary>
public class SessionExerciseResultDto
{
    public Guid SessionExerciseResultId { get; set; }
    public Guid? ExerciseId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string? UserAnswer { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public ResultStatus ResultStatus { get; set; }
    public int OrderIndex { get; set; }
}

/// <summary>
/// Summary DTO for listing sessions (lighter weight than full response)
/// </summary>
public class SessionSummaryDto
{
    public Guid SessionId { get; set; }
    public string PracticeSetTitle { get; set; } = string.Empty;
    public PracticeOrder PracticeOrder { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public bool IsActive => !EndedAt.HasValue;
    public int TotalExercises { get; set; }
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public int SkippedCount { get; set; }
    public double? ScorePercentage { get; set; }
}