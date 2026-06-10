using System.ComponentModel.DataAnnotations;
using LearnBase.API.Models.Enums;

namespace LearnBase.API.DTOs.Exercise;

/// <summary>
/// DTO for creating a new exercise
/// </summary>
public class CreateExerciseDto
{
    [Required]
    public ExerciseType Type { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Question { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// Required ONLY for MCQ type.
    /// Must contain 2-5 options.
    /// </summary>
    public List<ExerciseOptionDto>? Options { get; set; }

    /// <summary>
    /// REQUIRED only for MCQ type.
    /// Specifies WHICH option is correct by its OrderIndex (1-based).
    /// Example: If correctOptionIndex = 2, then the 2nd option in the list is correct.
    /// </summary>
    public int? CorrectOptionIndex { get; set; }
}