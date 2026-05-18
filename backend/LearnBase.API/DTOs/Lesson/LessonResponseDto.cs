namespace LearnBase.API.DTOs.Lesson
{
    public class LessonResponseDto
    {
        public Guid LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
