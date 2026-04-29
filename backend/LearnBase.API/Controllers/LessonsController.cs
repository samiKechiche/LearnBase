using LearnBase.API.DTOs.Lesson;
using LearnBase.API.DTOs.Shared;
using LearnBase.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearnBase.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LessonsController : ControllerBase
    {
        private readonly LessonService _service;

        public LessonsController(LessonService service)
        {
            _service = service;
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CreateLessonDto dto)
        {
            var lesson = await _service.CreateLesson(dto);

            return Ok(ApiResponseDto<LessonResponseDto>
                .SuccessResponse(lesson, "Lesson created successfully"));
        }

        // GET by user
        [HttpGet]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            var lessons = await _service.GetLessonsByUser(userId);

            return Ok(ApiResponseDto<List<LessonResponseDto>>
                .SuccessResponse(lessons));
        }

        // GET single
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var lesson = await _service.GetLesson(id);

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
            var updated = await _service.UpdateLesson(id, dto);

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
            var deleted = await _service.DeleteLesson(id);

            if (!deleted)
            {
                return NotFound(ApiResponseDto<string>
                    .ErrorResponse("Lesson not found"));
            }

            return Ok(ApiResponseDto<string>
                .SuccessResponse("Deleted", "Lesson deleted successfully"));
        }
    }
}