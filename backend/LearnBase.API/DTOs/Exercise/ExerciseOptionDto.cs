using System.ComponentModel.DataAnnotations;

namespace LearnBase.API.DTOs.Exercise;

/// <summary>
/// DTO for creating/updating an MCQ option
/// </summary>
public class ExerciseOptionDto
{
    [Required]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;

    public int OrderIndex { get; set; }
}