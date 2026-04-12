using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnBase.API.Models;

/// <summary>
/// A text note within a Lesson.
/// Cannot exist without a parent Lesson (composition relationship).
/// </summary>
public class Note
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid NoteId { get; set; }

    /// <summary>The text content of the note (supports long text)</summary>
    [Required]
    [Column(TypeName = "TEXT")]
    public string Content { get; set; } = string.Empty;

    // Foreign Keys
    [Required]
    public Guid LessonId { get; set; }

    [ForeignKey("LessonId")]
    public virtual Lesson? Lesson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}