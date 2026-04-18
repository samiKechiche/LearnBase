using Microsoft.AspNetCore.Mvc;
using LearnBase.API.DTOs.PracticeSet;
using LearnBase.API.DTOs.Shared;
using LearnBase.API.Services;

namespace LearnBase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PracticeSetsController : ControllerBase
{
    private readonly PracticeSetService _practiceSetService;

    public PracticeSetsController(PracticeSetService practiceSetService)
    {
        _practiceSetService = practiceSetService;
    }

    #region CRUD Endpoints

    /// <summary>
    /// POST: api/practicesets
    /// Creates a new empty practice set
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<PracticeSetResponseDto>>> CreatePracticeSet(
        [FromBody] CreatePracticeSetDto dto)
    {
        var result = await _practiceSetService.CreateAsync(dto, UserId);
        return ResultToActionResult(result);
    }

    /// <summary>
    /// GET: api/practicesets
    /// Lists all practice sets
    /// Query params: ?search=term&sortBy=title&ascending=true&hasLesson=true
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<PracticeSetResponseDto>>>> GetPracticeSets(
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = false,
        [FromQuery] bool? hasLesson = null)
    {
        var result = await _practiceSetService.GetAllAsync(UserId, search, sortBy, ascending, hasLesson);
        return Ok(result);
    }

    /// <summary>
    /// GET: api/practicesets/{id}
    /// Gets a practice set with all its exercises
    /// </summary>
    [HttpGet("{practiceSetId:guid}")]
    public async Task<ActionResult<ApiResponseDto<PracticeSetResponseDto>>> GetPracticeSetById(Guid practiceSetId)
    {
        var result = await _practiceSetService.GetByIdAsync(practiceSetId, UserId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// PUT: api/practicesets/{id}
    /// Updates practice set info
    /// </summary>
    [HttpPut("{practiceSetId:guid}")]
    public async Task<ActionResult<ApiResponseDto<PracticeSetResponseDto>>> UpdatePracticeSet(
        Guid practiceSetId,
        [FromBody] UpdatePracticeSetDto dto)
    {
        var result = await _practiceSetService.UpdateAsync(practiceSetId, dto, UserId);
        return ResultToActionResult(result);
    }

    /// <summary>
    /// DELETE: api/practicesets/{id}
    /// Deletes a practice set (only if no session history exists)
    /// </summary>
    [HttpDelete("{practiceSetId:guid}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> DeletePracticeSet(Guid practiceSetId)
    {
        var result = await _practiceSetService.DeleteAsync(practiceSetId, UserId);

        if (!result.Success)
            return BadRequest(result); // Returns error message about sessions

        return Ok(result);
    }

    #endregion

    #region Exercise Management Endpoints

    /// <summary>
    /// GET: api/practicesets/{id}/exercises
    /// Gets all exercises in a practice set (ordered)
    /// </summary>
    [HttpGet("{practiceSetId:guid}/exercises")]
    public async Task<ActionResult<ApiResponseDto<List<PracticeSetExerciseSummaryDto>>>> GetSetExercises(Guid practiceSetId)
    {
        var result = await _practiceSetService.GetExercisesAsync(practiceSetId, UserId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// POST: api/practicesets/{id}/exercises
    /// Adds an exercise to the practice set
    /// </summary>
    [HttpPost("{practiceSetId:guid}/exercises")]
    public async Task<ActionResult<ApiResponseDto<PracticeSetResponseDto>>> AddExerciseToSet(
        Guid practiceSetId,
        [FromBody] AddExerciseToSetDto dto)
    {
        var result = await _practiceSetService.AddExerciseAsync(practiceSetId, dto, UserId);
        return ResultToActionResult(result);
    }

    /// <summary>
    /// DELETE: api/practicesets/{id}/exercises/{exerciseId}
    /// Removes an exercise from the practice set
    /// </summary>
    [HttpDelete("{practiceSetId:guid}/exercises/{exerciseId:guid}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> RemoveExerciseFromSet(
        Guid practiceSetId,
        Guid exerciseId)
    {
        var result = await _practiceSetService.RemoveExerciseAsync(practiceSetId, exerciseId, UserId);
        return ResultToActionResult(result);
    }

    #endregion

    #region Auto-Generation Endpoint

    /// <summary>
    /// POST: api/practicesets/generate
    /// Auto-generates a practice set from tags
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<ApiResponseDto<PracticeSetResponseDto>>> GenerateFromTags(
        [FromBody] GeneratePracticeSetDto dto)
    {
        var result = await _practiceSetService.GenerateFromTagsAsync(dto, UserId);
        return ResultToActionResult(result);
    }

    #endregion

    #region Lesson Linking Endpoints

    /// <summary>
    /// POST: api/practicesets/{id}/link
    /// Links practice set to a lesson
    /// </summary>
    [HttpPost("{practiceSetId:guid}/link")]
    public async Task<ActionResult<ApiResponseDto<PracticeSetResponseDto>>> LinkToLesson(
        Guid practiceSetId,
        [FromBody] LinkLessonDto dto)
    {
        var result = await _practiceSetService.LinkToLessonAsync(practiceSetId, dto, UserId);
        return ResultToActionResult(result);
    }

    /// <summary>
    /// DELETE: api/practicesets/{id}/unlink
    /// Unlinks practice set from its lesson
    /// </summary>
    [HttpDelete("{practiceSetId:guid}/unlink")]
    public async Task<ActionResult<ApiResponseDto<PracticeSetResponseDto>>> UnlinkFromLesson(Guid practiceSetId)
    {
        var result = await _practiceSetService.UnlinkFromLessonAsync(practiceSetId, UserId);
        return ResultToActionResult(result);
    }

    #endregion

    #region Private Helpers

    private Guid UserId => Guid.Parse("00000000-0000-0000-0000-000000000001");

    private ActionResult ResultToActionResult<T>(ApiResponseDto<T> result)
    {
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    #endregion
}