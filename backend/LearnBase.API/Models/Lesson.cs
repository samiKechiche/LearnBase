using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Compression;

namespace LearnBase.API.Models;

/// <summary>
/// A container/folder for organizing learning materials.
/// Acts like a "folder" that holds notes and files related to a specific topic.
/// Can optionally be linked to Practice Sets for quick access during revision.
/// </summary>
public class Lesson
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid LessonId { get; set; }

    /// <summary>Title/name of this lesson</summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional description explaining what this lesson covers</summary>
    public string? Description { get; set; }

    // Foreign Keys
    [Required]
    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    // Navigation Properties (Composition - owned by Lesson)

    /// <summary>Text notes belonging to this lesson (deleted when lesson deleted)</summary>
    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();

    /// <summary>File attachments belonging to this lesson (deleted when lesson deleted)</summary>
    public virtual ICollection<AppFile> Files { get; set; } = new List<AppFile>();

    /// <summary>
    /// Practice Sets linked to this lesson for quick access.
    /// These are NOT deleted when lesson is deleted (just unlinked).
    /// </summary>
    public virtual ICollection<PracticeSet> PracticeSets { get; set; } = new List<PracticeSet>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}