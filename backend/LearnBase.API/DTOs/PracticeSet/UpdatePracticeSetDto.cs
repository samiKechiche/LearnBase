using System.ComponentModel.DataAnnotations;

namespace LearnBase.API.DTOs.PracticeSet;

/// <summary>
/// DTO for updating an existing practice set
/// </summary>
public class UpdatePracticeSetDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
}