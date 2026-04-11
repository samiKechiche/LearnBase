using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnBase.API.Models;

/// <summary>
/// Junction table for Many-to-Many relationship between Exercise and Tag.
/// Composite Primary Key: (ExerciseId, TagId)
/// </summary>
public class ExerciseTag
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public Guid ExerciseId { get; set; }

    [ForeignKey("ExerciseId")]
    public virtual Exercise? Exercise { get; set; }

    [Required]
    public Guid TagId { get; set; }

    [ForeignKey("TagId")]
    public virtual Tag? Tag { get; set; }
}