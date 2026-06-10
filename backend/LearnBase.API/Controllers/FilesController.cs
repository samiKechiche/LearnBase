using LearnBase.API.Data;
using LearnBase.API.DTOs.File;
using LearnBase.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearnBase.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly FileService _fileService;
        private readonly NoteService _noteService;
        private readonly IWebHostEnvironment _environment;
        public FilesController(FileService fileService, NoteService noteService, IWebHostEnvironment environment)
        {
            _fileService = fileService;
            _noteService = noteService;
            _environment = environment;

        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadFileDto dto)
        {
            try
            {
                var extension = Path.GetExtension(dto.File.FileName).ToLower();

                // DOCX → NOTES
                if (extension == ".docx")
                {
                    var note = await _noteService.CreateFromDocxAsync(dto.File, dto.LessonId);

                    return Ok(new
                    {
                        type = "note",
                        data = note
                    });
                }

                // EVERYTHING ELSE → FILES
                var result = await _fileService.UploadFileAsync(dto);

                return Ok(new
                {
                    type = "file",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("lesson/{lessonId}")]
        public async Task<IActionResult> GetFilesByLesson(Guid lessonId)
        {
            var files = await _fileService.GetFilesByLesson(lessonId);

            var result = files.Select(f => new FileResponseDto
            {
                FileId = f.FileId,
                FileName = f.FileName,
                FileType = f.FileType,
                FileSizeBytes = f.FileSizeBytes,
                UploadedAt = f.UploadedAt,
                LessonId = f.LessonId
            });

            return Ok(result);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _fileService.DeleteFileAsync(id);

            if (!deleted)
                return NotFound("File not found");

            return Ok();
        }
        [HttpGet("view/{fileId}")]
        public async Task<IActionResult> ViewFile(Guid fileId)
        {
            Console.WriteLine("VIEW FILE ID: " + fileId);

            var result = await _fileService.GetFileForViewAsync(fileId);

            if (result == null)
            {
                Console.WriteLine("FILE NOT FOUND IN SERVICE");
                return NotFound();
            }

            return File(result.Value.FileBytes, result.Value.ContentType);
        }
        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(Guid id)
        {
            var file = await _fileService.GetFileByIdAsync(id);

            if (file == null)
                return NotFound("File not found");

            var path = Path.Combine(_environment.ContentRootPath, file.FilePath);

            if (!System.IO.File.Exists(path))
                return NotFound("File missing on disk");

            var memory = new MemoryStream();

            await using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;

            return File(memory, file.FileType, file.FileName);
        }
    }
}