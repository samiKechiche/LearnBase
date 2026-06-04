using LearnBase.API.Data;
using LearnBase.API.DTOs.Lesson;
using LearnBase.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LearnBase.API.Services
{
    public class LessonService
    {
        private readonly ApplicationDbContext _context;

        public LessonService(ApplicationDbContext context)
        {
            _context = context;
        }

        // CREATE
        public async Task<LessonResponseDto> CreateLesson(CreateLessonDto dto)
        {
            var lesson = new Lesson
            {
                Title = dto.Title,
                Description = dto.Description,
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return new LessonResponseDto
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Description = lesson.Description,
                UserId = lesson.UserId,
                CreatedAt = lesson.CreatedAt
            };
        }

        // GET ALL by user
        public async Task<List<LessonResponseDto>> GetLessonsByUser(Guid userId)
        {
            return await _context.Lessons
                .Where(l => l.UserId == userId)
                .Select(l => new LessonResponseDto
                {
                    LessonId = l.LessonId,
                    Title = l.Title,
                    Description = l.Description,
                    UserId = l.UserId,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();
        }

        // GET single
        public async Task<LessonResponseDto?> GetLesson(Guid id)
        {
            var lesson = await _context.Lessons.FindAsync(id);

            if (lesson == null) return null;

            return new LessonResponseDto
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Description = lesson.Description,
                UserId = lesson.UserId,
                CreatedAt = lesson.CreatedAt
            };
        }
        public async Task<Lesson?> GetByIdAsync(Guid id)
        {
            return await _context.Lessons
                .FirstOrDefaultAsync(l => l.LessonId == id);
        }

        // UPDATE
        public async Task<bool> UpdateLesson(Guid id, UpdateLessonDto dto)
        {
            var lesson = await _context.Lessons.FindAsync(id);

            if (lesson == null) return false;

            lesson.Title = dto.Title;
            lesson.Description = dto.Description;
            lesson.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // DELETE
        public async Task<bool> DeleteLesson(Guid id)
        {
            var lesson = await _context.Lessons.FindAsync(id);

            if (lesson == null) return false;

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}