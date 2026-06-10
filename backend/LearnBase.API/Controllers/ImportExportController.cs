using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearnBase.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using LearnBase.API.DTOs; // <--- new DTO namespace

namespace LearnBase.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportExportController : ControllerBase
    {
        private readonly ImportExportService _importExportService;
        private readonly ILogger<ImportExportController> _logger;

        public ImportExportController(ImportExportService importExportService, ILogger<ImportExportController> logger)
        {
            _importExportService = importExportService;
            _logger = logger;
        }

        // Export a Lesson (JSON)
        [HttpGet("lesson/{id}/export")]
        [Authorize]
        public async Task<IActionResult> ExportLesson(Guid id)
        {
            var result = await _importExportService.ExportLessonAsync(id);
            if (result == null)
                return NotFound();

            var (content, fileName) = result.Value;
            return File(content, "application/json", fileName);
        }

        // Export a PracticeSet (JSON)
        [HttpGet("practiceset/{id}/export")]
        [Authorize]
        public async Task<IActionResult> ExportPracticeSet(Guid id)
        {
            var result = await _importExportService.ExportPracticeSetAsync(id);
            if (result == null)
                return NotFound();

            var (content, fileName) = result.Value;
            return File(content, "application/json", fileName);
        }

        // Import a Lesson from uploaded JSON file
        // Keep consumes so Swagger shows file picker
        [HttpPost("lesson/import")]
        [Authorize]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> ImportLesson([FromForm] FileUploadDto dto)
        {
            IFormFile file = dto?.File;

            try
            {
                if (file == null)
                {
                    // Fallback: raw body -> FormFile
                    if (Request.ContentLength == null || Request.ContentLength == 0)
                        return BadRequest("No file uploaded.");

                    var ms = new MemoryStream();
                    await Request.Body.CopyToAsync(ms);
                    ms.Position = 0;

                    file = new FormFile(ms, 0, ms.Length, "file", "upload.json")
                    {
                        Headers = Request.Headers,
                        ContentType = Request.ContentType ?? "application/octet-stream"
                    };
                }

                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");

                var userId = GetUserIdFromClaims();
                if (userId == null)
                    return BadRequest("Unable to determine user id from claims.");

                _logger.LogInformation("ImportLesson start. FileLength={Length}", file.Length);
                var createdId = await _importExportService.ImportLessonAsync(file, userId.Value);
                _logger.LogInformation("ImportLesson completed. CreatedLessonId={Id}", createdId);
                return CreatedAtAction(nameof(ExportLesson), new { id = createdId }, new { id = createdId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportLesson failed");
                return StatusCode(500, $"Import failed: {ex.Message}");
            }
        }

        // Import a PracticeSet from uploaded JSON file
        [HttpPost("practiceset/import")]
        [Authorize]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> ImportPracticeSet([FromForm] FileUploadDto dto)
        {
            IFormFile file = dto?.File;

            try
            {
                if (file == null)
                {
                    if (Request.ContentLength == null || Request.ContentLength == 0)
                        return BadRequest("No file uploaded.");

                    var ms = new MemoryStream();
                    await Request.Body.CopyToAsync(ms);
                    ms.Position = 0;

                    file = new FormFile(ms, 0, ms.Length, "file", "upload.json")
                    {
                        Headers = Request.Headers,
                        ContentType = Request.ContentType ?? "application/octet-stream"
                    };
                }

                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");

                var userId = GetUserIdFromClaims();
                if (userId == null)
                    return BadRequest("Unable to determine user id from claims.");

                _logger.LogInformation("ImportPracticeSet start. FileLength={Length}", file.Length);
                var createdId = await _importExportService.ImportPracticeSetAsync(file, userId.Value);
                _logger.LogInformation("ImportPracticeSet completed. CreatedPracticeSetId={Id}", createdId);
                return CreatedAtAction(nameof(ExportPracticeSet), new { id = createdId }, new { id = createdId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportPracticeSet failed");
                return StatusCode(500, $"Import failed: {ex.Message}");
            }
        }

        private Guid? GetUserIdFromClaims()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                        User.FindFirst("id") ??
                        User.FindFirst("sub");

            if (claim == null)
                return null;

            if (Guid.TryParse(claim.Value, out var guid))
                return guid;

            return null;
        }
    }
}
