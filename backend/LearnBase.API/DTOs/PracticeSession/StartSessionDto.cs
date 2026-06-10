using System.ComponentModel.DataAnnotations;
using LearnBase.API.Models.Enums;

namespace LearnBase.API.DTOs.PracticeSession;

/// <summary>
/// DTO for starting a new practice session
/// </summary>
public class StartSessionDto
{
    /// <summary>The practice set to use</summary>
    [Required]
    public Guid PracticeSetId { get; set; }

    /// <summary>Order: Default (as arranged) or Randomized (shuffled)</summary>
    [Required]
    public PracticeOrder PracticeOrder { get; set; } = PracticeOrder.Default;
}