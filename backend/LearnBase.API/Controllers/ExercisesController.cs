using LearnBase.API.DTOs.Exercise;
using LearnBase.API.DTOs.Shared;
using LearnBase.API.DTOs.Tag;
using LearnBase.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearnBase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExercisesController : ControllerBase
{
    private readonly ExerciseService _exerciseService;
    private readonly TagService _tagService;

    public ExercisesController(ExerciseService exerciseService, TagService tagService)
    {
        _exerciseService = exerciseService;
        _tagService = tagService;
    }

    private Guid UserId => Guid.Parse("00000000-0000-0000-0000-000000000001");

    /// <summary>
    /// POST: api/exercises
    /// Creates a new exercise
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<ExerciseResponseDto>>> CreateExercise(
        [FromBody] CreateExerciseDto dto)
    {
        var result = await _exerciseService.CreateAsync(dto, UserId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// GET: api/exercises
    /// Gets all exercises for the current user
    /// Optional query params: ?search=term&sortBy=date&ascending=false
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<ExerciseResponseDto>>>> GetExercises(
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = false)
    {
        var result = await _exerciseService.GetAllAsync(UserId, search, sortBy, ascending);

        return Ok(result);
    }

    /// <summary>
    /// GET: api/exercises/{id}
    /// Gets a specific exercise by ID
    /// </summary>
    [HttpGet("{exerciseId:guid}")]
    public async Task<ActionResult<ApiResponseDto<ExerciseResponseDto>>> GetExerciseById(
        Guid exerciseId)
    {
        var result = await _exerciseService.GetByIdAsync(exerciseId, UserId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// PUT: api/exercises/{id}
    /// Updates an existing exercise
    /// </summary>
    [HttpPut("{exerciseId:guid}")]
    public async Task<ActionResult<ApiResponseDto<ExerciseResponseDto>>> UpdateExercise(
        Guid exerciseId,
        [FromBody] UpdateExerciseDto dto)
    {
        var result = await _exerciseService.UpdateAsync(exerciseId, dto, UserId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// DELETE: api/exercises/{id}
    /// Deletes an exercise
    /// </summary>
    [HttpDelete("{exerciseId:guid}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> DeleteExercise(Guid exerciseId)
    {
        var result = await _exerciseService.DeleteAsync(exerciseId, UserId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════════════
    // TAG MANAGEMENT ENDPOINTS FOR EXERCISES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// GET: api/exercises/{exerciseId}/tags
    /// Gets all tags associated with a specific exercise
    /// </summary>
    [HttpGet("{exerciseId:guid}/tags")]
    public async Task<ActionResult<ApiResponseDto<List<TagResponseDto>>>> GetExerciseTags(Guid exerciseId)
    {
        var result = await _tagService.GetTagsForExerciseAsync(exerciseId, UserId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// POST: api/exercises/{exerciseId}/tags/{tagId}
    /// Adds a tag to an exercise
    /// </summary>
    [HttpPost("{exerciseId:guid}/tags/{tagId:guid}")]
    public async Task<ActionResult<ApiResponseDto<TagResponseDto>>> AddTagToExercise(
        Guid exerciseId,
        Guid tagId)
    {
        var result = await _tagService.AddTagToExerciseAsync(exerciseId, tagId, UserId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// DELETE: api/exercises/{exerciseId}/tags/{tagId}
    /// Removes a tag from an exercise
    /// </summary>
    [HttpDelete("{exerciseId:guid}/tags/{tagId:guid}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> RemoveTagFromExercise(
        Guid exerciseId,
        Guid tagId)
    {
        var result = await _tagService.RemoveTagFromExerciseAsync(exerciseId, tagId, UserId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}