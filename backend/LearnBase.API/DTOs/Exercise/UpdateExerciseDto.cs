using System.ComponentModel.DataAnnotations;
using LearnBase.API.Models.Enums;

namespace LearnBase.API.DTOs.Exercise;

/// <summary>
/// DTO for updating an existing exercise
/// </summary>
public class UpdateExerciseDto
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
    /// For MCQ: Updated list of options (replaces all existing options)
    /// For other types: null or empty
    /// </summary>
    public List<ExerciseOptionDto>? Options { get; set; }

    /// <summary>
    /// Required only for MCQ type.
    /// Specifies which option is correct by its OrderIndex.
    /// This matches CreateExerciseDto and avoids stale option IDs when options are replaced.
    /// </summary>
    public int? CorrectOptionIndex { get; set; }
}
