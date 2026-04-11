namespace LearnBase.API.Models.Enums;

/// <summary>
/// Result of answering a single exercise in a session
/// </summary>
public enum ResultStatus
{
    /// <summary>User answered correctly</summary>
    Correct,

    /// <summary>User answered incorrectly</summary>
    Incorrect,

    /// <summary>User skipped this exercise</summary>
    Skipped
}