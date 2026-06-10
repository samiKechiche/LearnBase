namespace LearnBase.API.DTOs.Tag;

/// <summary>
/// DTO for returning tag data to the client
/// </summary>
public class TagResponseDto
{
    public Guid TagId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Populated when fetching tags for a specific exercise
    public List<Guid>? ExerciseIds { get; set; }

    public DateTime CreatedAt { get; set; }
}