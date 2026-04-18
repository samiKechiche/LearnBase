using LearnBase.API.Models.Enums;

namespace LearnBase.API.DTOs.PracticeSet;

/// <summary>
/// DTO for returning practice set data
/// </summary>
public class PracticeSetResponseDto
{
    public Guid PracticeSetId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CreationType CreationType { get; set; }
    public Guid? LessonId { get; set; }
    public string? LessonTitle { get; set; } // Populated when linked

    // Exercise summaries included in this set
    public List<PracticeSetExerciseSummaryDto>? Exercises { get; set; }

    // Count of exercises (for list views without full exercise data)
    public int ExerciseCount { get; set; }

    // Session count (how many times practiced)
    public int SessionCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Summary of an exercise within a practice set
/// </summary>
public class PracticeSetExerciseSummaryDto
{
    public Guid ExerciseId { get; set; }
    public string Question { get; set; } = string.Empty;
    public ExerciseTypeSummary Type { get; set; }
    public int OrderIndex { get; set; }
}

public enum ExerciseTypeSummary
{
    MCQ,
    FillBlank,
    Flashcard
}