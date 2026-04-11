using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LearnBase.API.Models.Enums;

namespace LearnBase.API.Models;

/// <summary>
/// Stores the result of answering ONE exercise within a practice session.
/// Uses SNAPSHOTS so that historical accuracy is preserved even if the
/// original exercise is edited or deleted later.
/// </summary>
public class SessionExerciseResult
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid SessionExerciseResultId { get; set; }

    [Required]
    public Guid SessionId { get; set; }

    [ForeignKey("SessionId")]
    public virtual PracticeSession? Session { get; set; }

    /// <summary>
    /// Reference to the original exercise (optional).
    /// Kept for linking back, but snapshots below contain actual data shown.
    /// </summary>
    public Guid? ExerciseId { get; set; }

    [ForeignKey("ExerciseId")]
    public virtual Exercise? Exercise { get; set; }

    // ====== SNAPSHOT FIELDS (frozen at time of answering) ======

    /// <summary>[SNAPSHOT] The exact question text the user saw during this session</summary>
    [Required]
    [Column(TypeName = "TEXT")]
    public string ExerciseSnapshotQuestion { get; set; } = string.Empty;

    /// <summary>The answer the user provided (null if skipped)</summary>
    [Column(TypeName = "TEXT")]
    public string? UserAnswer { get; set; }

    /// <summary>[SNAPSHOT] The correct answer at the time of the session</summary>
    [Required]
    [Column(TypeName = "TEXT")]
    public string CorrectAnswerSnapshot { get; set; } = string.Empty;

    /// <summary>Whether the user got it right, wrong, or skipped</summary>
    [Required]
    public ResultStatus ResultStatus { get; set; }

    /// <summary>The position/sequence number of this exercise during the session</summary>
    public int OrderIndex { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}