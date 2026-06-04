using LearnBase.API.Data;
using LearnBase.API.DTOs.File;
using LearnBase.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LearnBase.API.Services
{
    public class FileService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public FileService(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<FileResponseDto> UploadFileAsync(UploadFileDto dto)
        {
            // Check lesson exists
            var lessonExists = await _context.Lessons
                .AnyAsync(l => l.LessonId == dto.LessonId);

            if (!lessonExists)
            {
                throw new Exception("Lesson not found.");
            }

            // Create uploads folder if missing
            var uploadsPath = Path.Combine(
    _environment.ContentRootPath,
    "uploads",
    "lessons",
    dto.LessonId.ToString());

            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Generate unique filename
            var uniqueFileName =
                $"{Guid.NewGuid()}_{dto.File.FileName}";

            var filePath = Path.Combine(
                uploadsPath,
                uniqueFileName);

            // Save physical file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            // Create DB record
            var appFile = new AppFile
            {
                FileId = Guid.NewGuid(),
                FileName = dto.File.FileName,
                FileType = dto.File.ContentType,
                FilePath = filePath,
                FileSizeBytes = dto.File.Length,
                LessonId = dto.LessonId
            };

            _context.Files.Add(appFile);

            await _context.SaveChangesAsync();

            return new FileResponseDto
            {
                FileId = appFile.FileId,
                FileName = appFile.FileName,
                FileType = appFile.FileType,
                FileSizeBytes = appFile.FileSizeBytes,
                UploadedAt = appFile.UploadedAt,
                LessonId = appFile.LessonId
            };
        }
    }
}