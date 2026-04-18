using System.ComponentModel.DataAnnotations;

namespace LearnBase.API.DTOs.PracticeSet;

/// <summary>
/// DTO for auto-generating a practice set from tags
/// </summary>
public class GeneratePracticeSetDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Tag IDs to include. All exercises with ANY of these tags will be included.
    /// </summary>
    [Required]
    [MinLength(1)]
    public List<Guid> TagIds { get; set; } = new();
}