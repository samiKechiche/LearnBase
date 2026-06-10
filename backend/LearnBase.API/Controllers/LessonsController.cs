using LearnBase.API.DTOs.Lesson;
using LearnBase.API.DTOs.Lessons;
using LearnBase.API.DTOs.Shared;
using LearnBase.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearnBase.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LessonsController : ControllerBase
    {
        private readonly LessonService _lessonService;
        private readonly NoteService _noteService;
        private readonly FileService _fileService;

        public LessonsController(
            LessonService lessonService,
            NoteService noteService,
            FileService fileService)
        {
            _lessonService = lessonService;
            _noteService = noteService;
            _fileService = fileService;
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CreateLessonDto dto)
        {
            var lesson = await _lessonService.CreateLesson(dto, UserId);

            return Ok(ApiResponseDto<LessonResponseDto>
                .SuccessResponse(lesson, "Lesson created successfully"));
        }

        // GET by user
        [HttpGet]
        public async Task<IActionResult> GetByUser([FromQuery] string? search = null)
        {
            var lessons = await _lessonService.GetLessonsByUser(UserId, search);

            return Ok(ApiResponseDto<List<LessonResponseDto>>
                .SuccessResponse(lessons));
        }

        // GET single
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var lesson = await _lessonService.GetLesson(id, UserId);

            if (lesson == null)
            {
                return NotFound(ApiResponseDto<string>
                    .ErrorResponse("Lesson not found"));
            }

            return Ok(ApiResponseDto<LessonResponseDto>
                .SuccessResponse(lesson));
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateLessonDto dto)
        {
            var updated = await _lessonService.UpdateLesson(id, dto, UserId);

            if (updated == null)
            {
                return NotFound(ApiResponseDto<string>
                    .ErrorResponse("Lesson not found"));
            }

            return Ok(ApiResponseDto<LessonResponseDto>
                .SuccessResponse(updated, "Lesson updated successfully"));
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _lessonService.DeleteLesson(id, UserId);

            if (!deleted)
            {
                return NotFound(ApiResponseDto<string>
                    .ErrorResponse("Lesson not found"));
            }

            return Ok(ApiResponseDto<string>
                .SuccessResponse("Deleted", "Lesson deleted successfully"));
        }

        // DETAILS (notes + files)
        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetLessonDetails(Guid id)
        {
            var lesson = await _lessonService.GetByIdAsync(id, UserId);

            if (lesson == null)
                return NotFound();

            var notes = await _noteService.GetByLessonIdAsync(id);
            var files = await _fileService.GetFilesByLesson(id);

            var result = new LessonDetailsDto
            {
                Lesson = lesson,
                Notes = notes,
                Files = files
            };

            return Ok(result);
        }

        // =========================
        // USER ID RESOLVER
        // =========================
        private Guid UserId
        {
            get
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                               ?? User.FindFirstValue("sub");

                if (string.IsNullOrEmpty(userIdClaim) ||
                    !Guid.TryParse(userIdClaim, out var userId))
                {
                    throw new InvalidOperationException(
                        "User ID claim not found or invalid in the current request.");
                }

                return userId;
            }
        }
    }
}