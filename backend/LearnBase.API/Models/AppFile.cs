using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnBase.API.Models;

/// <summary>
/// Represents a file attachment within a Lesson.
/// Stores metadata about the file; actual file content is stored on disk/cloud.
/// Cannot exist without a parent Lesson (composition relationship).
/// </summary>
public class AppFile
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid FileId { get; set; }

    /// <summary>The original filename as uploaded by user (e.g., "lecture-slides.pdf")</summary>
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File extension or MIME type (e.g., "application/pdf", "image/png", ".pdf")
    /// Useful for determining how to display/download the file.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// Path where the file is stored (relative or absolute).
    /// Example: "uploads/user123/lesson456/document.pdf"
    /// The actual bytes are NOT stored in the database - only the path.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>File size in bytes (useful for displaying to user)</summary>
    public long FileSizeBytes { get; set; }

    // Foreign Keys
    [Required]
    public Guid LessonId { get; set; }

    [ForeignKey("LessonId")]
    public virtual Lesson? Lesson { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}