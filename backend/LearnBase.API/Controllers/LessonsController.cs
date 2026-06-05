using LearnBase.API.DTOs.Lesson;
using LearnBase.API.DTOs.Lessons;
using LearnBase.API.DTOs.Shared;
using LearnBase.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearnBase.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);

            var lesson = await _lessonService.CreateLesson(dto, userId);

            return Ok(ApiResponseDto<LessonResponseDto>
                .SuccessResponse(lesson, "Lesson created successfully"));
        }

        // GET by user
        [HttpGet("my")]
        public async Task<IActionResult> GetMyLessons()
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);

            var lessons = await _lessonService.GetLessonsByUser(userId);

            return Ok(ApiResponseDto<List<LessonResponseDto>>
                .SuccessResponse(lessons));
        }

        // GET single
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var lesson = await _lessonService.GetLesson(id);

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
            var updated = await _lessonService.UpdateLesson(id, dto);

            if (!updated)
            {
                return NotFound(ApiResponseDto<string>
                    .ErrorResponse("Lesson not found"));
            }

            return Ok(ApiResponseDto<string>
                .SuccessResponse("Updated", "Lesson updated successfully"));
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _lessonService.DeleteLesson(id);

            if (!deleted)
            {
                return NotFound(ApiResponseDto<string>
                    .ErrorResponse("Lesson not found"));
            }

            return Ok(ApiResponseDto<string>
                .SuccessResponse("Deleted", "Lesson deleted successfully"));
        }
        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetLessonDetails(Guid id)
        {
            var lesson = await _lessonService.GetByIdAsync(id);

            if (lesson == null)
                return NotFound();

            var notes = await _noteService.GetByLessonIdAsync(id);
            var files = await _fileService.GetByLessonIdAsync(id);

            var result = new LessonDetailsDto
            {
                Lesson = lesson,
                Notes = notes,
                Files = files
            };

            return Ok(result);
        }
    }
}