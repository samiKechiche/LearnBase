using LearnBase.API.DTOs.Exercise;
using LearnBase.API.DTOs.Shared;
using LearnBase.API.DTOs.Tag;
using LearnBase.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearnBase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]  // ADD THIS - require authentication for ALL exercise endpoints
public class ExercisesController : ControllerBase
{
    private readonly ExerciseService _exerciseService;
    private readonly TagService _tagService;

    public ExercisesController(ExerciseService exerciseService, TagService tagService)
    {
        _exerciseService = exerciseService;
        _tagService = tagService;
    }

    // EXTRACT UserId from JWT claims (same pattern as other controllers)
    private Guid UserId
    {
        get
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new InvalidOperationException("User ID claim not found or invalid.");
            }

            return userId;
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<ExerciseResponseDto>>> CreateExercise(
        [FromBody] CreateExerciseDto dto)
    {
        var result = await _exerciseService.CreateAsync(dto, UserId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<ExerciseResponseDto>>>> GetExercises(
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = false)
    {
        var result = await _exerciseService.GetAllAsync(UserId, search, sortBy, ascending);
        return Ok(result);
    }

    [HttpGet("{exerciseId:guid}")]
    public async Task<ActionResult<ApiResponseDto<ExerciseResponseDto>>> GetExerciseById(Guid exerciseId)
    {
        var result = await _exerciseService.GetByIdAsync(exerciseId, UserId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPut("{exerciseId:guid}")]
    public async Task<ActionResult<ApiResponseDto<ExerciseResponseDto>>> UpdateExercise(
        Guid exerciseId, [FromBody] UpdateExerciseDto dto)
    {
        var result = await _exerciseService.UpdateAsync(exerciseId, dto, UserId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{exerciseId:guid}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> DeleteExercise(Guid exerciseId)
    {
        var result = await _exerciseService.DeleteAsync(exerciseId, UserId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    // Tag management endpoints
    [HttpGet("{exerciseId:guid}/tags")]
    public async Task<ActionResult<ApiResponseDto<List<TagResponseDto>>>> GetExerciseTags(Guid exerciseId)
    {
        var result = await _tagService.GetTagsForExerciseAsync(exerciseId, UserId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPost("{exerciseId:guid}/tags/{tagId:guid}")]
    public async Task<ActionResult<ApiResponseDto<TagResponseDto>>> AddTagToExercise(
        Guid exerciseId, Guid tagId)
    {
        var result = await _tagService.AddTagToExerciseAsync(exerciseId, tagId, UserId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{exerciseId:guid}/tags/{tagId:guid}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> RemoveTagFromExercise(
        Guid exerciseId, Guid tagId)
    {
        var result = await _tagService.RemoveTagFromExerciseAsync(exerciseId, tagId, UserId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}