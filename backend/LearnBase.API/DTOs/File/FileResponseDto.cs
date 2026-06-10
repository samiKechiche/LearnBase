namespace LearnBase.API.DTOs.File
{
    public class FileResponseDto
    {
        public Guid FileId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FileType { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        public DateTime UploadedAt { get; set; }

        public Guid LessonId { get; set; }
    }
}