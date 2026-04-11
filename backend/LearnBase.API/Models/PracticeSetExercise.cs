using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnBase.API.Models;

/// <summary>
/// Junction table for Many-to-Many relationship between PracticeSet and Exercise.
/// Includes OrderIndex to control the sequence of exercises within a practice set.
/// Composite Primary Key: (PracticeSetId, ExerciseId)
/// </summary>
public class PracticeSetExercise
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public Guid PracticeSetId { get; set; }

    [ForeignKey("PracticeSetId")]
    public virtual PracticeSet? PracticeSet { get; set; }

    [Required]
    public Guid ExerciseId { get; set; }

    [ForeignKey("ExerciseId")]
    public virtual Exercise? Exercise { get; set; }

    /// <summary>Position/order of this exercise within the practice set (0-based or 1-based)</summary>
    public int OrderIndex { get; set; }
}