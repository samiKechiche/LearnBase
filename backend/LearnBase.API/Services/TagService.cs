using Microsoft.EntityFrameworkCore;
using LearnBase.API.Data;
using LearnBase.API.DTOs.Shared;
using LearnBase.API.DTOs.Tag;
using LearnBase.API.Models;

namespace LearnBase.API.Services;

/// <summary>
/// Service layer for Tag business logic
/// Handles CRUD operations and Exercise-Tag linking
/// </summary>
public class TagService
{
    private readonly ApplicationDbContext _context;

    public TagService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new tag for the specified user
    /// </summary>
    public async Task<ApiResponseDto<TagResponseDto>> CreateAsync(CreateTagDto dto, Guid userId)
    {
        // Check if user already has a tag with same name (unique constraint)
        var existingTag = await _context.Tags
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Name == dto.Name);

        if (existingTag != null)
        {
            return ApiResponseDto<TagResponseDto>.ErrorResponse(
                $"You already have a tag named '{dto.Name}'. Tag names must be unique.");
        }

        var tag = new Tag
        {
            TagId = Guid.NewGuid(),
            Name = dto.Name,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        return ApiResponseDto<TagResponseDto>.SuccessResponse(
            MapToResponseDto(tag),
            "Tag created successfully");
    }

    /// <summary>
    /// Gets all tags for a user with optional search and sort
    /// </summary>
    public async Task<ApiResponseDto<List<TagResponseDto>>> GetAllAsync(
        Guid userId,
        string? searchTerm = null,
        string? sortBy = null,
        bool ascending = true)
    {
        IQueryable<Tag> query = _context.Tags
            .Where(t => t.UserId == userId);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(searchTerm));
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "name" => ascending
                ? query.OrderBy(t => t.Name)
                : query.OrderByDescending(t => t.Name),
            "date" or "createdat" => ascending
                ? query.OrderBy(t => t.CreatedAt)
                : query.OrderByDescending(t => t.CreatedAt),
            _ => ascending
                ? query.OrderByDescending(t => t.CreatedAt)
                : query.OrderBy(t => t.CreatedAt)
        };

        var tags = await query.ToListAsync();
        var responseDtos = tags.Select(MapToResponseDto).ToList();

        return ApiResponseDto<List<TagResponseDto>>.SuccessResponse(responseDtos);
    }

    /// <summary>
    /// Gets a single tag by ID (verifies ownership)
    /// </summary>
    public async Task<ApiResponseDto<TagResponseDto>> GetByIdAsync(Guid tagId, Guid userId)
    {
        var tag = await _context.Tags
            .Include(t => t.ExerciseTags)
            .FirstOrDefaultAsync(t => t.TagId == tagId && t.UserId == userId);

        if (tag == null)
        {
            return ApiResponseDto<TagResponseDto>.ErrorResponse("Tag not found");
        }

        return ApiResponseDto<TagResponseDto>.SuccessResponse(MapToResponseDto(tag));
    }

    /// <summary>
    /// Updates an existing tag
    /// </summary>
    public async Task<ApiResponseDto<TagResponseDto>> UpdateAsync(Guid tagId, UpdateTagDto dto, Guid userId)
    {
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.TagId == tagId && t.UserId == userId);

        if (tag == null)
        {
            return ApiResponseDto<TagResponseDto>.ErrorResponse("Tag not found");
        }

        // Check for duplicate name (excluding current tag)
        var duplicateTag = await _context.Tags
            .FirstOrDefaultAsync(t => t.UserId == userId
                && t.Name == dto.Name
                && t.TagId != tagId);

        if (duplicateTag != null)
        {
            return ApiResponseDto<TagResponseDto>.ErrorResponse(
                $"Another tag named '{dto.Name}' already exists.");
        }

        tag.Name = dto.Name;

        await _context.SaveChangesAsync();

        // Reload with includes
        tag = await _context.Tags
            .Include(t => t.ExerciseTags)
            .FirstAsync(t => t.TagId == tagId);

        return ApiResponseDto<TagResponseDto>.SuccessResponse(
            MapToResponseDto(tag),
            "Tag updated successfully");
    }

    /// <summary>
    /// Deletes a tag (also removes it from all exercises via cascade)
    /// </summary>
    public async Task<ApiResponseDto<bool>> DeleteAsync(Guid tagId, Guid userId)
    {
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.TagId == tagId && t.UserId == userId);

        if (tag == null)
        {
            return ApiResponseDto<bool>.ErrorResponse("Tag not found");
        }

        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();

        return ApiResponseDto<bool>.SuccessResponse(true, "Tag deleted successfully");
    }

    #region Exercise-Tag Linking Methods

    /// <summary>
    /// Adds a tag to an exercise
    /// </summary>
    public async Task<ApiResponseDto<TagResponseDto>> AddTagToExerciseAsync(
        Guid exerciseId,
        Guid tagId,
        Guid userId)
    {
        // Verify exercise exists and belongs to user
        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId && e.UserId == userId);

        if (exercise == null)
        {
            return ApiResponseDto<TagResponseDto>.ErrorResponse("Exercise not found");
        }

        // Verify tag exists and belongs to user
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.TagId == tagId && t.UserId == userId);

        if (tag == null)
        {
            return ApiResponseDto<TagResponseDto>.ErrorResponse("Tag not found");
        }

        // Check if link already exists
        var existingLink = await _context.ExerciseTags
            .FirstOrDefaultAsync(et => et.ExerciseId == exerciseId && et.TagId == tagId);

        if (existingLink != null)
        {
            return ApiResponseDto<TagResponseDto>.ErrorResponse(
                "This exercise already has this tag.");
        }

        // Create the link
        _context.ExerciseTags.Add(new ExerciseTag
        {
            Id = Guid.NewGuid(),
            ExerciseId = exerciseId,
            TagId = tagId
        });

        await _context.SaveChangesAsync();

        return ApiResponseDto<TagResponseDto>.SuccessResponse(
            MapToResponseDto(tag),
            $"Tag '{tag.Name}' added to exercise successfully");
    }

    /// <summary>
    /// Removes a tag from an exercise
    /// </summary>
    public async Task<ApiResponseDto<bool>> RemoveTagFromExerciseAsync(
        Guid exerciseId,
        Guid tagId,
        Guid userId)
    {
        // Verify exercise belongs to user
        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId && e.UserId == userId);

        if (exercise == null)
        {
            return ApiResponseDto<bool>.ErrorResponse("Exercise not found");
        }

        // Find and remove the link
        var link = await _context.ExerciseTags
            .FirstOrDefaultAsync(et => et.ExerciseId == exerciseId && et.TagId == tagId);

        if (link == null)
        {
            return ApiResponseDto<bool>.ErrorResponse(
                "This exercise does not have this tag.");
        }

        _context.ExerciseTags.Remove(link);
        await _context.SaveChangesAsync();

        return ApiResponseDto<bool>.SuccessResponse(true, "Tag removed from exercise successfully");
    }

    /// <summary>
    /// Gets all tags for a specific exercise
    /// </summary>
    public async Task<ApiResponseDto<List<TagResponseDto>>> GetTagsForExerciseAsync(
        Guid exerciseId,
        Guid userId)
    {
        // Verify exercise belongs to user
        var exerciseExists = await _context.Exercises
            .AnyAsync(e => e.ExerciseId == exerciseId && e.UserId == userId);

        if (!exerciseExists)
        {
            return ApiResponseDto<List<TagResponseDto>>.ErrorResponse("Exercise not found");
        }

        var tags = await _context.ExerciseTags
            .Where(et => et.ExerciseId == exerciseId)
            .Include(et => et.Tag)
            .Select(et => et.Tag!)
            .OrderBy(t => t.Name)
            .ToListAsync();

        var responseDtos = tags.Select(MapToResponseDto).ToList();

        return ApiResponseDto<List<TagResponseDto>>.SuccessResponse(responseDtos);
    }

    #endregion

    #region Private Helpers

    private TagResponseDto MapToResponseDto(Tag tag)
    {
        var dto = new TagResponseDto
        {
            TagId = tag.TagId,
            Name = tag.Name,
            CreatedAt = tag.CreatedAt
        };

        // Include exercise IDs if loaded
        if (tag.ExerciseTags != null && tag.ExerciseTags.Any())
        {
            dto.ExerciseIds = tag.ExerciseTags.Select(et => et.ExerciseId).ToList();
        }

        return dto;
    }

    #endregion
}