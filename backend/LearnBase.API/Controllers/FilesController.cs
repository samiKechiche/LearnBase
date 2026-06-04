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
        [HttpGet]
        public async Task<IActionResult> GetFilesByLesson(Guid lessonId)
        {
            var files = await _fileService.GetFilesByLesson(lessonId);
            return Ok(files);
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