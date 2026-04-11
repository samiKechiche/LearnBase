using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnBase.API.Models;

/// <summary>
/// Represents the single user of LearnBase (single-user personal platform).
/// Owns all data through composition relationships - nothing survives user deletion.
/// </summary>
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    /// <summary>Profile picture URL or relative path</summary>
    public string? ProfilePicture { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime PasswordUpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ════════════════════════════════════════════════════════════
    // NAVIGATION PROPERTIES - SAMI'S MODULES (Exercise & Practice)
    // ════════════════════════════════════════════════════════════

    /// <summary>All exercises created by this user</summary>
    public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();

    /// <summary>All tags created by this user</summary>
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    /// <summary>All practice sets created by this user</summary>
    public virtual ICollection<PracticeSet> PracticeSets { get; set; } = new List<PracticeSet>();

    /// <summary>All practice sessions completed by this user</summary>
    public virtual ICollection<PracticeSession> PracticeSessions { get; set; } = new List<PracticeSession>();

    // ════════════════════════════════════════════════════════════
    // NAVIGATION PROPERTIES - YOUSSEF'S MODULE (Lessons)
    // ════════════════════════════════════════════════════════════

    /// <summary>All lessons created by this user</summary>
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}