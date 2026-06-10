using System.ComponentModel.DataAnnotations;

namespace LearnBase.API.DTOs.Tag;

/// <summary>
/// DTO for creating a new tag
/// </summary>
public class CreateTagDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}