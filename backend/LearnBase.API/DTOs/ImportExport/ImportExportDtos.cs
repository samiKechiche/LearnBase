using LearnBase.API.Models.Enums;

namespace LearnBase.API.DTOs.ImportExport
{
    // Export DTOs - intentionally lightweight and independent of EF navigation properties

    public class LessonExportDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<NoteExportDto>? Notes { get; set; }
        public List<FileExportDto>? Files { get; set; }
        public List<PracticeSetExportDto>? PracticeSets { get; set; }
    }

    public class NoteExportDto
    {
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class FileExportDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
    }

    public class PracticeSetExportDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CreationType CreationType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ExerciseExportDto>? Exercises { get; set; }
    }

    public class PracticeSetExportDtoSimple
    {
        public string Title { get; set; } = string.Empty;
        public Guid? LinkedLessonId { get; set; }
    }

    public class ExerciseExportDto
    {
        public ExerciseType Type { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public List<ExerciseOptionExportDto>? Options { get; set; }
        /// <summary>
        /// If MCQ, correct option is exported as the zero-based index into the ordered Options list.
        /// If null then there is no correct option recorded.
        /// </summary>
        public int? CorrectOptionIndex { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ExerciseOptionExportDto
    {
        public string Content { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
    }
}
