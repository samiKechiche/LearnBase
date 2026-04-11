using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LearnBase.API.Models.Enums;

namespace LearnBase.API.Models;

/// <summary>
/// A collection/grouping of exercises for practicing together.
/// Can be manually created or auto-generated from tags.
/// Can optionally be linked to one Lesson for context.
/// </summary>
public class PracticeSet
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid PracticeSetId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional description of this practice set</summary>
    public string? Description { get; set; }

    /// <summary>How this practice set was created</summary>
    [Required]
    public CreationType CreationType { get; set; }

    /// <summary>
    /// Optional link to a Lesson (0 or 1).
    /// Null means this practice set is standalone/not linked.
    /// </summary>
    public Guid? LessonId { get; set; }

    [ForeignKey("LessonId")]
    public virtual Lesson? Lesson { get; set; }

    // Foreign Keys
    [Required]
    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    /// <summary>Exercises in this set (ordered via junction table)</summary>
    public virtual ICollection<PracticeSetExercise> PracticeSetExercises { get; set; } = new List<PracticeSetExercise>();

    /// <summary>All practice sessions completed using this set</summary>
    public virtual ICollection<PracticeSession> PracticeSessions { get; set; } = new List<PracticeSession>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}