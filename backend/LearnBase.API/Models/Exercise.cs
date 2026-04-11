using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LearnBase.API.Models.Enums;

namespace LearnBase.API.Models;

/// <summary>
/// Represents a single exercise/question that can be practiced.
/// Types: MCQ (multiple choice), FillBlank (type answer), Flashcard.
/// </summary>
public class Exercise
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ExerciseId { get; set; }

    /// <summary>Type of exercise (MCQ, FillBlank, or Flashcard)</summary>
    [Required]
    public ExerciseType Type { get; set; }

    /// <summary>The question/prompt shown to the user</summary>
    [Required]
    [Column(TypeName = "TEXT")]
    public string Question { get; set; } = string.Empty;

    /// <summary>
    /// The correct answer text.
    /// For MCQ: This is a backup/reference (use CorrectOptionId for primary tracking).
    /// For FillBlank: The exact expected text.
    /// For Flashcard: The "back" side content.
    /// </summary>
    [Required]
    [Column(TypeName = "TEXT")]
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// [NEW] Points to the correct ExerciseOption for MCQ type.
    /// Null for FillBlank and Flashcard types.
    /// This ensures referential integrity for MCQ answers.
    /// </summary>
    public Guid? CorrectOptionId { get; set; }

    // Foreign Keys
    [Required]
    public Guid UserId { get; set; }

    // Navigation Properties
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    /// <summary>MCQ options (only populated when Type == MCQ)</summary>
    public virtual ICollection<ExerciseOption> ExerciseOptions { get; set; } = new List<ExerciseOption>();

    /// <summary>Many-to-Many link to Tags via ExerciseTag junction table</summary>
    public virtual ICollection<ExerciseTag> ExerciseTags { get; set; } = new List<ExerciseTag>();

    /// <summary>Many-to-Many link to PracticeSets via PracticeSetExercise junction table</summary>
    public virtual ICollection<PracticeSetExercise> PracticeSetExercises { get; set; } = new List<PracticeSetExercise>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}