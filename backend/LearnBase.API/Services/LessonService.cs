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
        public async Task<LessonResponseDto> CreateLesson(CreateLessonDto dto, Guid userId)
        {
            var lesson = new Lesson
            {
                Title = dto.Title,
                Description = dto.Description,
                UserId = userId,
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
                    CreatedAt = lesson.CreatedAt,
                    UpdatedAt = lesson.UpdatedAt
                };
        }

        // GET ALL by user
        public async Task<List<LessonResponseDto>> GetLessonsByUser(Guid userId, string? search = null)
        {
            var query = _context.Lessons
                .Where(l => l.UserId == userId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(l =>
                    l.Title.Contains(term) ||
                    (l.Description != null && l.Description.Contains(term)));
            }

            return await query
                .OrderByDescending(l => l.UpdatedAt)
                .Select(l => new LessonResponseDto
                {
                    LessonId = l.LessonId,
                    Title = l.Title,
                    Description = l.Description,
                    UserId = l.UserId,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt
                })
                .ToListAsync();
        }

        // GET single
        public async Task<LessonResponseDto?> GetLesson(Guid id, Guid userId)
        {
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.LessonId == id && l.UserId == userId);

            if (lesson == null) return null;

            return new LessonResponseDto
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Description = lesson.Description,
                UserId = lesson.UserId,
                CreatedAt = lesson.CreatedAt,
                UpdatedAt = lesson.UpdatedAt
            };
        }

        public async Task<Lesson?> GetByIdAsync(Guid id, Guid userId)
        {
            return await _context.Lessons
                .FirstOrDefaultAsync(l => l.LessonId == id && l.UserId == userId);
        }

        // UPDATE
        public async Task<LessonResponseDto?> UpdateLesson(Guid id, UpdateLessonDto dto, Guid userId)
        {
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.LessonId == id && l.UserId == userId);

            if (lesson == null) return null;

            lesson.Title = dto.Title;
            lesson.Description = dto.Description;
            lesson.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new LessonResponseDto
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Description = lesson.Description,
                UserId = lesson.UserId,
                CreatedAt = lesson.CreatedAt,
                UpdatedAt = lesson.UpdatedAt
            };
        }

        // DELETE
        public async Task<bool> DeleteLesson(Guid id, Guid userId)
        {
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.LessonId == id && l.UserId == userId);

            if (lesson == null) return false;

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
