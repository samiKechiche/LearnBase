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
    /// For MCQ: Which option is correct (must be in Options list)
    /// </summary>
    public Guid? CorrectOptionId { get; set; }
}