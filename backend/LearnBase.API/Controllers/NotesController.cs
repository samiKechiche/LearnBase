using LearnBase.API.DTOs.Note;
using LearnBase.API.DTOs.Shared;
using LearnBase.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearnBase.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        private readonly NoteService _service;

        public NotesController(NoteService service)
        {
            _service = service;
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CreateNoteDto dto)
        {
            var note = await _service.CreateNote(dto);

            return Ok(ApiResponseDto<NoteResponseDto>
                .SuccessResponse(note, "Note created successfully"));
        }

        // GET single note
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var note = await _service.GetNote(id);

            if (note == null)
            {
                return NotFound(ApiResponseDto<string>
                    .ErrorResponse("Note not found"));
            }

            return Ok(ApiResponseDto<NoteResponseDto>
                .SuccessResponse(note));
        }

        // GET notes by lesson
        [HttpGet("lesson/{lessonId}")]
        public async Task<IActionResult> GetByLesson(Guid lessonId)
        {
            var notes = await _service.GetNotesByLesson(lessonId);

            return Ok(ApiResponseDto<List<NoteResponseDto>>
                .SuccessResponse(notes));
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateNoteDto dto)
        {
            var updated = await _service.UpdateNote(id, dto);

            if (!updated)
            {
                return NotFound(ApiResponseDto<string>
                    .ErrorResponse("Note not found"));
            }

            return Ok(ApiResponseDto<string>
                .SuccessResponse("Updated", "Note updated successfully"));
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteNote(id);

            if (!deleted)
            {
                return NotFound(ApiResponseDto<string>
                    .ErrorResponse("Note not found"));
            }

            return Ok(ApiResponseDto<string>
                .SuccessResponse("Deleted", "Note deleted successfully"));
        }
    }
}