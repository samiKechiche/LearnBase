using System.ComponentModel.DataAnnotations;

namespace LearnBase.API.DTOs.Tag;

/// <summary>
/// DTO for updating an existing tag
/// </summary>
public class UpdateTagDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}