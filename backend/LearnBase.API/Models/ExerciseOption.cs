using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnBase.API.Models;

/// <summary>
/// Represents a single multiple-choice option.
/// Only used when parent Exercise.Type == MCQ.
/// Constraint: Each MCQ exercise must have between 2 and 5 options.
/// </summary>
public class ExerciseOption
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid OptionId { get; set; }

    [Required]
    public Guid ExerciseId { get; set; }

    [ForeignKey("ExerciseId")]
    public virtual Exercise? Exercise { get; set; }

    /// <summary>The text content of this option (e.g., "A) Paris")</summary>
    [Required]
    [Column(TypeName = "TEXT")]
    public string Content { get; set; } = string.Empty;

    /// <summary>Display order (1 = first option, 2 = second, etc.)</summary>
    public int OrderIndex { get; set; }
}