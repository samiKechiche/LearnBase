using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnBase.API.Models;

/// <summary>
/// A label/category for organizing and filtering exercises.
/// Examples: "Chapter1", "Difficult", "Calculus", "History"
/// </summary>
public class Tag
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid TagId { get; set; }

    /// <summary>Tag name (unique per user)</summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    // Foreign Keys
    [Required]
    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    /// <summary>Link to exercises via ExerciseTag junction table</summary>
    public virtual ICollection<ExerciseTag> ExerciseTags { get; set; } = new List<ExerciseTag>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}