namespace LearnBase.API.DTOs.Note
{
    public class NoteResponseDto
    {
        public Guid NoteId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
