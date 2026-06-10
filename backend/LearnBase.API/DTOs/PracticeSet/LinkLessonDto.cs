using System.ComponentModel.DataAnnotations;

namespace LearnBase.API.DTOs.PracticeSet;

/// <summary>
/// DTO for linking a practice set to a lesson
/// </summary>
public class LinkLessonDto
{
    [Required]
    public Guid LessonId { get; set; }
}