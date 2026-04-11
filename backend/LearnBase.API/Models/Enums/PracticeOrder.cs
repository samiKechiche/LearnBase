namespace LearnBase.API.Models.Enums;

/// <summary>
/// Order in which exercises are presented during practice
/// </summary>
public enum PracticeOrder
{
    /// <summary>Original order as arranged in Practice Set</summary>
    Default,

    /// <summary>Randomly shuffled order</summary>
    Randomized
}