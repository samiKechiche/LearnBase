using System.ComponentModel.DataAnnotations;

namespace LearnBase.API.DTOs.PracticeSession;

/// <summary>
/// DTO for skipping an exercise during a session
/// </summary>
public class SkipExerciseDto
{
    /// <summary>The exercise being skipped</summary>
    [Required]
    public Guid ExerciseId { get; set; }
}