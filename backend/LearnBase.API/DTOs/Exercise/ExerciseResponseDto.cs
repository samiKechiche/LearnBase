using LearnBase.API.Models.Enums;

namespace LearnBase.API.DTOs.Exercise;

/// <summary>
/// DTO for returning exercise data to the client
/// </summary>
public class ExerciseResponseDto
{
    public Guid ExerciseId { get; set; }
    public ExerciseType Type { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public Guid? CorrectOptionId { get; set; }

    // Populated only when Type == MCQ
    public List<ExerciseOptionResponseDto>? Options { get; set; }

    // Tag IDs associated with this exercise
    public List<Guid>? TagIds { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Nested DTO for exercise options in responses
/// </summary>
public class ExerciseOptionResponseDto
{
    public Guid OptionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}