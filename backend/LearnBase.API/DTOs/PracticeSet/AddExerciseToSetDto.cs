using System.ComponentModel.DataAnnotations;

namespace LearnBase.API.DTOs.PracticeSet;

/// <summary>
/// DTO for adding an exercise to a practice set
/// </summary>
public class AddExerciseToSetDto
{
    [Required]
    public Guid ExerciseId { get; set; }

    /// <summary>
    /// Position/order of this exercise in the set.
    /// If null, it's added at the end.
    /// </summary>
    public int? OrderIndex { get; set; }
}