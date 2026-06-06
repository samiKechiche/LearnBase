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

        public async Task<List<AppFile>> GetFilesByLesson(Guid lessonId)
        {
            return await _context.Files
                .Where(f => f.LessonId == lessonId)
                .ToListAsync();
        }
        public async Task<AppFile?> GetFileByIdAsync(Guid id)
        {
            return await _context.Files.FirstOrDefaultAsync(f => f.FileId == id);
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
            var extension = Path.GetExtension(dto.File.FileName).ToLower();
            
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

            var relativePath = Path.Combine("uploads", "lessons", dto.LessonId.ToString(), uniqueFileName);

            var physicalPath = Path.Combine(_environment.ContentRootPath,relativePath);

            // Save physical file
            using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }

                // Create DB record
                var appFile = new AppFile
                {
                    FileId = Guid.NewGuid(),
                    FileName = dto.File.FileName,
                    FileType = dto.File.ContentType,
                    FilePath = relativePath,
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
        public async Task<bool> DeleteFileAsync(Guid id)
        {
            var file = await _context.Files.FirstOrDefaultAsync(f => f.FileId == id);

            if (file == null) return false;

            var fullPath = Path.Combine(_environment.ContentRootPath, file.FilePath);

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            _context.Files.Remove(file);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<(byte[] FileBytes, string ContentType, string FileName)?> GetFileForViewAsync(Guid fileId)
        {
            var file = await _context.Files.FirstOrDefaultAsync(f => f.FileId == fileId);

            if (file == null)
                return null;

            if (!System.IO.File.Exists(file.FilePath))
                return null;

            var bytes = await System.IO.File.ReadAllBytesAsync(file.FilePath);

            return (bytes, file.FileType, file.FileName);
        }

    }
    }