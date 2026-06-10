namespace LearnBase.API.DTOs.PracticeSession;

/// <summary>
/// DTO for overall practice statistics across all sessions
/// </summary>
public class PracticeStatsDto
{
    public string? Message { get; set; }

    public int TotalCompletedSessions { get; set; }
    public int TotalExercisesPracticed { get; set; }
    public int TotalCorrect { get; set; }
    public int TotalIncorrect { get; set; }
    public int TotalSkipped { get; set; }

    /// <summary>Overall accuracy percentage (0-100). Null if no answered exercises.</summary>
    public double? OverallAccuracyPercentage { get; set; }

    /// <summary>Name of the practice set with highest average score</summary>
    public string? BestPracticeSetName { get; set; }
    public double? BestPracticeSetAccuracy { get; set; }

    /// <summary>Average number of exercises per session</summary>
    public double AverageExercisesPerSession { get; set; }

    /// <summary>When the user last completed a session</summary>
    public DateTime? LastPracticedAt { get; set; }
}