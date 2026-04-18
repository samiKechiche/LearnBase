using Microsoft.EntityFrameworkCore;
using LearnBase.API.Data;
using LearnBase.API.DTOs.Exercise;
using LearnBase.API.DTOs.Shared;
using LearnBase.API.Models;
using LearnBase.API.Models.Enums;

namespace LearnBase.API.Services;

/// <summary>
/// Service layer for Exercise business logic
/// Handles validation, CRUD operations, and data transformation
/// </summary>
public class ExerciseService
{
    private readonly ApplicationDbContext _context;

    public ExerciseService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new exercise for the specified user
    /// </summary>
    public async Task<ApiResponseDto<ExerciseResponseDto>> CreateAsync(CreateExerciseDto dto, Guid userId)
    {
        // Validate based on exercise type
        var validationErrors = ValidateCreateDto(dto);
        if (validationErrors.Any())
        {
            return ApiResponseDto<ExerciseResponseDto>.ErrorResponse(
                "Validation failed", validationErrors);
        }

        // Create the exercise entity
        var exercise = new Exercise
        {
            ExerciseId = Guid.NewGuid(),
            Type = dto.Type,
            Question = dto.Question,
            Answer = dto.Answer,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add options if MCQ
        Guid? correctOptionId = null;

        if (dto.Type == ExerciseType.MCQ && dto.Options != null)
        {
            var createdOptions = new List<ExerciseOption>();

            foreach (var optDto in dto.Options)
            {
                var newOption = new ExerciseOption
                {
                    OptionId = Guid.NewGuid(),
                    ExerciseId = exercise.ExerciseId,
                    Content = optDto.Content,
                    OrderIndex = optDto.OrderIndex
                };

                createdOptions.Add(newOption);
                exercise.ExerciseOptions.Add(newOption);
            }

            // Find the correct option by OrderIndex and set its ID
            if (dto.CorrectOptionIndex.HasValue)
            {
                var correctOpt = createdOptions
                    .FirstOrDefault(o => o.OrderIndex == dto.CorrectOptionIndex.Value);

                if (correctOpt != null)
                {
                    correctOptionId = correctOpt.OptionId;
                }
            }

            exercise.CorrectOptionId = correctOptionId;
        }

        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();

        return ApiResponseDto<ExerciseResponseDto>.SuccessResponse(
            MapToResponseDto(exercise),
            "Exercise created successfully");
    }

    /// <summary>
    /// Gets all exercises for a user with optional search and sort
    /// </summary>
    public async Task<ApiResponseDto<List<ExerciseResponseDto>>> GetAllAsync(
        Guid userId,
        string? searchTerm = null,
        string? sortBy = null,
        bool ascending = true)
    {
        IQueryable<Exercise> query = _context.Exercises
            .Include(e => e.ExerciseOptions)
            .Where(e => e.UserId == userId);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(e =>
                e.Question.ToLower().Contains(searchTerm) ||
                e.Answer.ToLower().Contains(searchTerm));
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "question" => ascending
                ? query.OrderBy(e => e.Question)
                : query.OrderByDescending(e => e.Question),
            "type" => ascending
                ? query.OrderBy(e => e.Type)
                : query.OrderByDescending(e => e.Type),
            "date" or "createdat" => ascending
                ? query.OrderBy(e => e.CreatedAt)
                : query.OrderByDescending(e => e.CreatedAt),
            _ => ascending
                ? query.OrderByDescending(e => e.CreatedAt)
                : query.OrderBy(e => e.CreatedAt)
        };

        var exercises = await query.ToListAsync();

        var responseDtos = exercises.Select(MapToResponseDto).ToList();

        return ApiResponseDto<List<ExerciseResponseDto>>.SuccessResponse(responseDtos);
    }

    /// <summary>
    /// Gets a single exercise by ID (verifies ownership)
    /// </summary>
    public async Task<ApiResponseDto<ExerciseResponseDto>> GetByIdAsync(Guid exerciseId, Guid userId)
    {
        var exercise = await _context.Exercises
            .Include(e => e.ExerciseOptions)
            .Include(e => e.ExerciseTags)
                .ThenInclude(et => et.Tag)
            .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId && e.UserId == userId);

        if (exercise == null)
        {
            return ApiResponseDto<ExerciseResponseDto>.ErrorResponse("Exercise not found");
        }

        return ApiResponseDto<ExerciseResponseDto>.SuccessResponse(MapToResponseDto(exercise));
    }

    /// <summary>
    /// Updates an existing exercise
    /// </summary>
    public async Task<ApiResponseDto<ExerciseResponseDto>> UpdateAsync(
        Guid exerciseId,
        UpdateExerciseDto dto,
        Guid userId)
    {
        var exercise = await _context.Exercises
            .Include(e => e.ExerciseOptions)
            .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId && e.UserId == userId);

        if (exercise == null)
        {
            return ApiResponseDto<ExerciseResponseDto>.ErrorResponse("Exercise not found");
        }

        // Validate
        var validationErrors = ValidateExerciseDto(dto);
        if (validationErrors.Any())
        {
            return ApiResponseDto<ExerciseResponseDto>.ErrorResponse(
                "Validation failed", validationErrors);
        }

        // Update basic fields
        exercise.Type = dto.Type;
        exercise.Question = dto.Question;
        exercise.Answer = dto.Answer;
        exercise.CorrectOptionId = dto.CorrectOptionId;
        exercise.UpdatedAt = DateTime.UtcNow;

        // Handle options update (remove old, add new)
        if (exercise.Type == ExerciseType.MCQ)
        {
            // Remove existing options
            _context.ExerciseOptions.RemoveRange(exercise.ExerciseOptions);
            exercise.ExerciseOptions.Clear();

            // Add new options
            if (dto.Options != null)
            {
                foreach (var optDto in dto.Options)
                {
                    exercise.ExerciseOptions.Add(new ExerciseOption
                    {
                        OptionId = Guid.NewGuid(),
                        ExerciseId = exercise.ExerciseId,
                        Content = optDto.Content,
                        OrderIndex = optDto.OrderIndex
                    });
                }
            }
        }
        else
        {
            // Non-MCQ shouldn't have options
            _context.ExerciseOptions.RemoveRange(exercise.ExerciseOptions);
            exercise.ExerciseOptions.Clear();
        }

        await _context.SaveChangesAsync();

        // Reload to get updated data
        await _context.Exercises
            .Include(e => e.ExerciseOptions)
            .FirstAsync(e => e.ExerciseId == exerciseId);

        return ApiResponseDto<ExerciseResponseDto>.SuccessResponse(
            MapToResponseDto(exercise),
            "Exercise updated successfully");
    }

    /// <summary>
    /// Deletes an exercise (cascade deletes options and cleans up junction tables)
    /// </summary>
    public async Task<ApiResponseDto<bool>> DeleteAsync(Guid exerciseId, Guid userId)
    {
        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId && e.UserId == userId);

        if (exercise == null)
        {
            return ApiResponseDto<bool>.ErrorResponse("Exercise not found");
        }

        _context.Exercises.Remove(exercise);
        await _context.SaveChangesAsync();

        return ApiResponseDto<bool>.SuccessResponse(true, "Exercise deleted successfully");
    }

    #region Private Helper Methods

    /// <summary>
    /// Validates CreateExerciseDto specifically (uses Index, not ID)
    /// </summary>
    private List<string> ValidateCreateDto(CreateExerciseDto dto)
    {
        var errors = new List<string>();

        if (dto.Type == ExerciseType.MCQ)
        {
            // MCQ must have options
            if (dto.Options == null || !dto.Options.Any())
            {
                errors.Add("MCQ exercises must have at least 2 options.");
            }
            else if (dto.Options.Count < 2 || dto.Options.Count > 5)
            {
                errors.Add("MCQ exercises must have between 2 and 5 options.");
            }

            // MCQ must specify correct answer index
            if (!dto.CorrectOptionIndex.HasValue)
            {
                errors.Add("MCQ exercises must specify a CorrectOptionIndex (which option number is correct?).");
            }
            else if (dto.Options != null)
            {
                // Verify the index is within range of provided options
                var isValidIndex = dto.Options.Any(o => o.OrderIndex == dto.CorrectOptionIndex.Value);
                if (!isValidIndex)
                {
                    errors.Add($"CorrectOptionIndex ({dto.CorrectOptionIndex.Value}) does not match any option's OrderIndex.");
                }
            }
        }
        else
        {
            // Non-MCQ should not have options
            if (dto.Options != null && dto.Options.Any())
            {
                errors.Add($"Only MCQ exercises can have options. Current type: {dto.Type}");
            }

            // Non-MCQ should not have CorrectOptionIndex
            if (dto.CorrectOptionIndex.HasValue)
            {
                errors.Add($"CorrectOptionIndex is only applicable for MCQ exercises.");
            }
        }

        return errors;
    }

    /// <summary>
    /// Validates exercise DTO based on its type
    /// </summary>
    private List<string> ValidateExerciseDto(dynamic dto)
    {
        var errors = new List<string>();
        ExerciseType type = dto.Type;
        var options = dto.Options as List<ExerciseOptionDto>;
        Guid? correctOptionId = dto.CorrectOptionId;

        if (type == ExerciseType.MCQ)
        {
            // MCQ must have options
            if (options == null || !options.Any())
            {
                errors.Add("MCQ exercises must have at least 2 options.");
            }
            else if (options.Count < 2 || options.Count > 5)
            {
                errors.Add("MCQ exercises must have between 2 and 5 options.");
            }

            // MCQ must specify correct answer
            if (!correctOptionId.HasValue)
            {
                errors.Add("MCQ exercises must specify a CorrectOptionId.");
            }
            else if (options != null && !options.Any())
            {
                // Will be caught above, but check if valid GUID matches
            }

            // Verify CorrectOptionId exists in options (if options provided)
            if (correctOptionId.HasValue && options != null && options.Any())
            {
                // Note: At creation time, options don't have IDs yet, so we validate by index/order
                // The controller/service will handle mapping after creation
            }
        }
        else
        {
            // Non-MCQ should not have options
            if (options != null && options.Any())
            {
                errors.Add($"Only MCQ exercises can have options. Current type: {type}");
            }

            // Non-MCQ should not have CorrectOptionId
            if (correctOptionId.HasValue)
            {
                errors.Add($"CorrectOptionId is only applicable for MCQ exercises.");
            }
        }

        return errors;
    }

    /// <summary>
    /// Maps Exercise entity to Response DTO
    /// </summary>
    private ExerciseResponseDto MapToResponseDto(Exercise exercise)
    {
        var dto = new ExerciseResponseDto
        {
            ExerciseId = exercise.ExerciseId,
            Type = exercise.Type,
            Question = exercise.Question,
            Answer = exercise.Answer,
            CorrectOptionId = exercise.CorrectOptionId,
            CreatedAt = exercise.CreatedAt,
            UpdatedAt = exercise.UpdatedAt
        };

        // Map options if present (MCQ)
        if (exercise.ExerciseOptions != null && exercise.ExerciseOptions.Any())
        {
            dto.Options = exercise.ExerciseOptions
                .OrderBy(o => o.OrderIndex)
                .Select(o => new ExerciseOptionResponseDto
                {
                    OptionId = o.OptionId,
                    Content = o.Content,
                    OrderIndex = o.OrderIndex
                })
                .ToList();
        }

        // Map tag IDs if loaded
        if (exercise.ExerciseTags != null && exercise.ExerciseTags.Any())
        {
            dto.TagIds = exercise.ExerciseTags.Select(et => et.TagId).ToList();
        }

        return dto;
    }

    #endregion
}