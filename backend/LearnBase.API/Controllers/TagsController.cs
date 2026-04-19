using Microsoft.AspNetCore.Mvc;
using LearnBase.API.DTOs.Shared;
using LearnBase.API.DTOs.Tag;
using LearnBase.API.Services;

namespace LearnBase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly TagService _tagService;

    public TagsController(TagService tagService)
    {
        _tagService = tagService;
    }

    private Guid UserId => Guid.Parse("00000000-0000-0000-0000-000000000001");

    /// <summary>
    /// POST: api/tags
    /// Creates a new tag
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<TagResponseDto>>> CreateTag(
        [FromBody] CreateTagDto dto)
    {
        var result = await _tagService.CreateAsync(dto, UserId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// GET: api/tags
    /// Gets all tags for current user
    /// Optional: ?search=term&sortBy=name&ascending=true
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<TagResponseDto>>>> GetTags(
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = true)
    {
        var result = await _tagService.GetAllAsync(UserId, search, sortBy, ascending);

        return Ok(result);
    }

    /// <summary>
    /// GET: api/tags/{id}
    /// Gets a specific tag by ID
    /// </summary>
    [HttpGet("{tagId:guid}")]
    public async Task<ActionResult<ApiResponseDto<TagResponseDto>>> GetTagById(Guid tagId)
    {
        var result = await _tagService.GetByIdAsync(tagId, UserId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// PUT: api/tags/{id}
    /// Updates a tag
    /// </summary>
    [HttpPut("{tagId:guid}")]
    public async Task<ActionResult<ApiResponseDto<TagResponseDto>>> UpdateTag(
        Guid tagId,
        [FromBody] UpdateTagDto dto)
    {
        var result = await _tagService.UpdateAsync(tagId, dto, UserId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// DELETE: api/tags/{id}
    /// Deletes a tag
    /// </summary>
    [HttpDelete("{tagId:guid}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> DeleteTag(Guid tagId)
    {
        var result = await _tagService.DeleteAsync(tagId, UserId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}