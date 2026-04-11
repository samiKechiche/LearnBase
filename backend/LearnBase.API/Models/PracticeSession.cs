using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LearnBase.API.Models.Enums;

namespace LearnBase.API.Models;

/// <summary>
/// Records a single practice attempt by a user using a specific PracticeSet.
/// Contains results for each exercise attempted.
/// </summary>
public class PracticeSession
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid SessionId { get; set; }

    /// <summary>The order in which exercises were presented</summary>
    [Required]
    public PracticeOrder PracticeOrder { get; set; }

    [Required]
    public Guid PracticeSetId { get; set; }

    [ForeignKey("PracticeSetId")]
    public virtual PracticeSet? PracticeSet { get; set; }

    // Foreign Keys
    [Required]
    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    /// <summary>When the session started</summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When the session ended (null while session is active)</summary>
    public DateTime? EndedAt { get; set; }

    /// <summary>Individual results for each exercise in this session</summary>
    public virtual ICollection<SessionExerciseResult> SessionExerciseResults { get; set; } = new List<SessionExerciseResult>();
}