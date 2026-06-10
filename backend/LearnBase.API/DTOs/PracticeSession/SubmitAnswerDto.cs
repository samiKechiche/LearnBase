using System.ComponentModel.DataAnnotations;

namespace LearnBase.API.DTOs.PracticeSession;

/// <summary>
/// DTO for submitting an answer during a practice session
/// </summary>
public class SubmitAnswerDto
{
    /// <summary>The exercise being answered</summary>
    [Required]
    public Guid ExerciseId { get; set; }

    /// <summary>The user's answer text (for MCQ: the selected option's text content)</summary>
    [Required]
    public string UserAnswer { get; set; } = string.Empty;
}