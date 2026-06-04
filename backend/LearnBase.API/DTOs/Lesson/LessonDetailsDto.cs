using LearnBase.API.Models;

namespace LearnBase.API.DTOs.Lessons
{
    public class LessonDetailsDto
    {
        public LearnBase.API.Models.Lesson Lesson { get; set; } = null!;
        public List<LearnBase.API.Models.Note> Notes { get; set; } = new();
        public List<AppFile> Files { get; set; } = new();
    }
}