using Microsoft.AspNetCore.Mvc;
using LearnBase.API.DTOs.PracticeSession;
using LearnBase.API.DTOs.Shared;
using LearnBase.API.Services;

namespace LearnBase.API.Controllers;

/// <summary>
/// Controller for managing practice sessions.
/// Users can start sessions, submit answers, skip exercises,
/// end sessions, view history, and delete sessions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PracticeSessionsController : ControllerBase
{
    private readonly PracticeSessionService _sessionService;

    public PracticeSessionsController(PracticeSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    // ================================================================
    // START & END SESSION
    // ================================================================

    /// <summary>
    /// POST: api/practicesessions/start
    /// Starts a new practice session from a practice set.
    /// </summary>
    [HttpPost("start")]
    public async Task<ActionResult<ApiResponseDto<SessionResponseDto>>> StartSession(
        [FromBody] StartSessionDto dto)
    {
        var result = await _sessionService.StartSessionAsync(dto, UserId);
        return ResultToActionResult(result);
    }

    /// <summary>
    /// POST: api/practicesessions/{sessionId}/end
    /// Ends an active practice session early.
    /// </summary>
    [HttpPost("{sessionId:guid}/end")]
    public async Task<ActionResult<ApiResponseDto<SessionResponseDto>>> EndSession(Guid sessionId)
    {
        var result = await _sessionService.EndSessionAsync(sessionId, UserId);
        return ResultToActionResult(result);
    }

    // ================================================================
    // ANSWER & SKIP
    // ================================================================

    /// <summary>
    /// POST: api/practicesessions/{sessionId}/submit
    /// Submits an answer for an exercise in the session.
    /// </summary>
    [HttpPost("{sessionId:guid}/submit")]
    public async Task<ActionResult<ApiResponseDto<SessionExerciseResultDto>>> SubmitAnswer(
        Guid sessionId,
        [FromBody] SubmitAnswerDto dto)
    {
        var result = await _sessionService.SubmitAnswerAsync(sessionId, dto, UserId);
        return ResultToActionResult(result);
    }

    /// <summary>
    /// POST: api/practicesessions/{sessionId}/skip
    /// Skips an exercise in the session.
    /// </summary>
    [HttpPost("{sessionId:guid}/skip")]
    public async Task<ActionResult<ApiResponseDto<SessionExerciseResultDto>>> SkipExercise(
        Guid sessionId,
        [FromBody] SkipExerciseDto dto)
    {
        var result = await _sessionService.SkipExerciseAsync(sessionId, dto, UserId);
        return ResultToActionResult(result);
    }

    // ================================================================
    // SESSION HISTORY
    // ================================================================

    /// <summary>
    /// GET: api/practicesessions
    /// Gets all practice sessions for the current user.
    /// Optional: ?activeOnly=true to show only active sessions.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<SessionSummaryDto>>>> GetSessions(
        [FromQuery] bool? activeOnly = null)
    {
        var result = await _sessionService.GetAllSessionsAsync(UserId, activeOnly);
        return Ok(result);
    }

    /// <summary>
    /// GET: api/practicesessions/{sessionId}
    /// Gets a single session with full details and results.
    /// </summary>
    [HttpGet("{sessionId:guid}")]
    public async Task<ActionResult<ApiResponseDto<SessionResponseDto>>> GetSessionById(
        Guid sessionId)
    {
        var result = await _sessionService.GetSessionByIdAsync(sessionId, UserId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    // ================================================================
    // DELETE SESSIONS
    // ================================================================

    /// <summary>
    /// DELETE: api/practicesessions/{sessionId}
    /// Deletes a single practice session.
    /// </summary>
    [HttpDelete("{sessionId:guid}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> DeleteSession(Guid sessionId)
    {
        var result = await _sessionService.DeleteSessionAsync(sessionId, UserId);
        return ResultToActionResult(result);
    }

    /// <summary>
    /// DELETE: api/practicesessions/all
    /// Deletes ALL practice sessions for the current user.
    /// </summary>
    [HttpDelete("all")]
    public async Task<ActionResult<ApiResponseDto<bool>>> DeleteAllSessions()
    {
        var result = await _sessionService.DeleteAllSessionsAsync(UserId);
        return ResultToActionResult(result);
    }

    // ================================================================
    // PRIVATE HELPERS
    // ================================================================

    private Guid UserId => Guid.Parse("00000000-0000-0000-0000-000000000001");

    private ActionResult ResultToActionResult<T>(ApiResponseDto<T> result)
    {
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}