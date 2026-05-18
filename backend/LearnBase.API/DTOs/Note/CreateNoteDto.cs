namespace LearnBase.API.DTOs.Note
{
    public class CreateNoteDto
    {
        public string Content { get; set; } = string.Empty;
        public Guid LessonId { get; set; }
    }
}
