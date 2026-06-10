using System.ComponentModel.DataAnnotations;
using LearnBase.API.Models.Enums;

namespace LearnBase.API.DTOs.PracticeSet;

/// <summary>
/// DTO for creating a new practice set manually
/// </summary>
public class CreatePracticeSetDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public CreationType CreationType { get; set; }
}