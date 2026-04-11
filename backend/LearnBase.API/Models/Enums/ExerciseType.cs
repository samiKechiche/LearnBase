namespace LearnBase.API.Models.Enums;

/// <summary>
/// Types of exercises supported in LearnBase
/// </summary>
public enum ExerciseType
{
    /// <summary>Multiple Choice Question - requires 2-5 options</summary>
    MCQ,

    /// <summary>User types in the answer text</summary>
    FillBlank,

    /// <summary>Flashcard with front (question) and back (answer)</summary>
    Flashcard
}