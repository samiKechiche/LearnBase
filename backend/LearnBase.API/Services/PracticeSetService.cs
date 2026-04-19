using Microsoft.EntityFrameworkCore;
using LearnBase.API.Data;
using LearnBase.API.DTOs.PracticeSet;
using LearnBase.API.DTOs.Shared;
using LearnBase.API.Models;
using LearnBase.API.Models.Enums;

namespace LearnBase.API.Services;

/// <summary>
/// Service layer for Practice Set business logic
/// Handles CRUD, exercise management, auto-generation, and lesson linking
/// </summary>
public class PracticeSetService
{
    private readonly ApplicationDbContext _context;

    public PracticeSetService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region CRUD Operations

    /// <summary>
    /// Creates a new empty practice set (manual creation)
    /// </summary>
    public async Task<ApiResponseDto<PracticeSetResponseDto>> CreateAsync(
        CreatePracticeSetDto dto,
        Guid userId)
    {
        var practiceSet = new PracticeSet
        {
            PracticeSetId = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            CreationType = dto.CreationType,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.PracticeSets.Add(practiceSet);
        await _context.SaveChangesAsync();

        return ApiResponseDto<PracticeSetResponseDto>.SuccessResponse(
            MapToResponseDto(practiceSet),
            "Practice set created successfully");
    }

    /// <summary>
    /// Gets all practice sets for user with search, filter, sort
    /// </summary>
    public async Task<ApiResponseDto<List<PracticeSetResponseDto>>> GetAllAsync(
        Guid userId,
        string? searchTerm = null,
        string? sortBy = null,
        bool ascending = false,
        bool? hasLesson = null)
    {
        IQueryable<PracticeSet> query = _context.PracticeSets
            .Include(ps => ps.Lesson)
            .Include(ps => ps.PracticeSetExercises)
                .ThenInclude(pse => pse.Exercise)
            .Where(ps => ps.UserId == userId);

        // Search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(ps =>
                ps.Title.ToLower().Contains(term) ||
                (ps.Description != null && ps.Description.ToLower().Contains(term)));
        }

        // Filter by lesson linkage
        if (hasLesson.HasValue)
        {
            if (hasLesson.Value)
                query = query.Where(ps => ps.LessonId != null);
            else
                query = query.Where(ps => ps.LessonId == null);
        }

        // Sorting
        query = sortBy?.ToLower() switch
        {
            "title" => ascending ? query.OrderBy(ps => ps.Title) : query.OrderByDescending(ps => ps.Title),
            "date" or "createdat" => ascending ? query.OrderBy(ps => ps.CreatedAt) : query.OrderByDescending(ps => ps.CreatedAt),
            "exercisecount" => ascending
                ? query.OrderBy(ps => ps.PracticeSetExercises.Count)
                : query.OrderByDescending(ps => ps.PracticeSetExercises.Count),
            _ => query.OrderByDescending(ps => ps.UpdatedAt)
        };

        var sets = await query.ToListAsync();
        var responseDtos = sets.Select(MapToResponseDto).ToList();

        return ApiResponseDto<List<PracticeSetResponseDto>>.SuccessResponse(responseDtos);
    }

    /// <summary>
    /// Gets a single practice set with all its exercises
    /// </summary>
    public async Task<ApiResponseDto<PracticeSetResponseDto>> GetByIdAsync(Guid practiceSetId, Guid userId)
    {
        var set = await _context.PracticeSets
            .Include(ps => ps.Lesson)
            .Include(ps => ps.PracticeSetExercises)
                .ThenInclude(pse => pse.Exercise)
            .FirstOrDefaultAsync(ps => ps.PracticeSetId == practiceSetId && ps.UserId == userId);

        if (set == null)
        {
            return ApiResponseDto<PracticeSetResponseDto>.ErrorResponse("Practice set not found");
        }

        return ApiResponseDto<PracticeSetResponseDto>.SuccessResponse(MapToResponseDto(set));
    }

    /// <summary>
    /// Updates practice set basic info
    /// </summary>
    public async Task<ApiResponseDto<PracticeSetResponseDto>> UpdateAsync(
        Guid practiceSetId,
        UpdatePracticeSetDto dto,
        Guid userId)
    {
        var set = await _context.PracticeSets
            .FirstOrDefaultAsync(ps => ps.PracticeSetId == practiceSetId && ps.UserId == userId);

        if (set == null)
        {
            return ApiResponseDto<PracticeSetResponseDto>.ErrorResponse("Practice set not found");
        }

        set.Title = dto.Title;
        set.Description = dto.Description;
        set.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload with includes
        set = await _context.PracticeSets
            .Include(ps => ps.Lesson)
            .Include(ps => ps.PracticeSetExercises)
                .ThenInclude(pse => pse.Exercise)
            .FirstAsync(ps => ps.PracticeSetId == practiceSetId);

        return ApiResponseDto<PracticeSetResponseDto>.SuccessResponse(
            MapToResponseDto(set),
            "Practice set updated successfully");
    }

    /// <summary>
    /// Deletes a practice set (only if no sessions exist due to RESTRICT FK)
    /// </summary>
    public async Task<ApiResponseDto<bool>> DeleteAsync(Guid practiceSetId, Guid userId)
    {
        var set = await _context.PracticeSets
            .Include(ps => ps.PracticeSessions) // Check for sessions
            .FirstOrDefaultAsync(ps => ps.PracticeSetId == practiceSetId && ps.UserId == userId);

        if (set == null)
        {
            return ApiResponseDto<bool>.ErrorResponse("Practice set not found");
        }

        if (set.PracticeSessions != null && set.PracticeSessions.Any())
        {
            return ApiResponseDto<bool>.ErrorResponse(
                "Cannot delete practice set that has practice session history. " +
                "Delete the session history first, or consider archiving instead.");
        }

        _context.PracticeSets.Remove(set);
        await _context.SaveChangesAsync();

        return ApiResponseDto<bool>.SuccessResponse(true, "Practice set deleted successfully");
    }

    #endregion

    #region Exercise Management

    /// <summary>
    /// Adds an exercise to a practice set
    /// </summary>
    public async Task<ApiResponseDto<PracticeSetResponseDto>> AddExerciseAsync(
        Guid practiceSetId,
        AddExerciseToSetDto dto,
        Guid userId)
    {
        // Verify set exists and belongs to user
        var set = await _context.PracticeSets
            .FirstOrDefaultAsync(ps => ps.PracticeSetId == practiceSetId && ps.UserId == userId);

        if (set == null)
        {
            return ApiResponseDto<PracticeSetResponseDto>.ErrorResponse("Practice set not found");
        }

        // Verify exercise exists and belongs to user
        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(e => e.ExerciseId == dto.ExerciseId && e.UserId == userId);

        if (exercise == null)
        {
            return ApiResponseDto<PracticeSetResponseDto>.ErrorResponse("Exercise not found");
        }

        // Check if already in set
        var existingEntry = await _context.PracticeSetExercises
            .FirstOrDefaultAsync(pse => pse.PracticeSetId == practiceSetId && pse.ExerciseId == dto.ExerciseId);

        if (existingEntry != null)
        {
            return ApiResponseDto<PracticeSetResponseDto>.ErrorResponse(
                "This exercise is already in the practice set.");
        }

        // Determine order index
        int orderIndex = dto.OrderIndex ??
            (await _context.PracticeSetExercises
                .Where(pse => pse.PracticeSetId == practiceSetId)
                .AnyAsync()
                ? await _context.PracticeSetExercises
                    .Where(pse => pse.PracticeSetId == practiceSetId)
                    .MaxAsync(pse => pse.OrderIndex) + 1
                : 0);

        _context.PracticeSetExercises.Add(new PracticeSetExercise
        {
            Id = Guid.NewGuid(),
            PracticeSetId = practiceSetId,
            ExerciseId = dto.ExerciseId,
            OrderIndex = orderIndex
        });

        set.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Reload
        set = await _context.PracticeSets
            .Include(ps => ps.Lesson)
            .Include(ps => ps.PracticeSetExercises)
                .ThenInclude(pse => pse.Exercise)
            .FirstAsync(ps => ps.PracticeSetId == practiceSetId);

        return ApiResponseDto<PracticeSetResponseDto>.SuccessResponse(
            MapToResponseDto(set),
            "Exercise added to practice set");
    }

    /// <summary>
    /// Removes an exercise from a practice set
    /// </summary>
    public async Task<ApiResponseDto<bool>> RemoveExerciseAsync(
        Guid practiceSetId,
        Guid exerciseId,
        Guid userId)
    {
        // Verify ownership
        var setExists = await _context.PracticeSets
            .AnyAsync(ps => ps.PracticeSetId == practiceSetId && ps.UserId == userId);

        if (!setExists)
        {
            return ApiResponseDto<bool>.ErrorResponse("Practice set not found");
        }

        var entry = await _context.PracticeSetExercises
            .FirstOrDefaultAsync(pse => pse.PracticeSetId == practiceSetId && pse.ExerciseId == exerciseId);

        if (entry == null)
        {
            return ApiResponseDto<bool>.ErrorResponse("Exercise not found in this practice set");
        }

        _context.PracticeSetExercises.Remove(entry);

        // Update the set's timestamp
        var set = await _context.PracticeSets.FindAsync(practiceSetId);
        if (set != null) set.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponseDto<bool>.SuccessResponse(true, "Exercise removed from practice set");
    }

    /// <summary>
    /// Gets all exercises in a practice set
    /// </summary>
    public async Task<ApiResponseDto<List<PracticeSetExerciseSummaryDto>>> GetExercisesAsync(
        Guid practiceSetId,
        Guid userId)
    {
        var setExists = await _context.PracticeSets
            .AnyAsync(ps => ps.PracticeSetId == practiceSetId && ps.UserId == userId);

        if (!setExists)
        {
            return ApiResponseDto<List<PracticeSetExerciseSummaryDto>>.ErrorResponse("Practice set not found");
        }

        var entries = await _context.PracticeSetExercises
            .Include(pse => pse.Exercise)
            .Where(pse => pse.PracticeSetId == practiceSetId)
            .OrderBy(pse => pse.OrderIndex)
            .ToListAsync();

        var summaries = entries.Select(pse => new PracticeSetExerciseSummaryDto
        {
            ExerciseId = pse.ExerciseId,
            Question = pse.Exercise.Question,
            Type = pse.Exercise.Type switch
            {
                Models.Enums.ExerciseType.MCQ => ExerciseTypeSummary.MCQ,
                Models.Enums.ExerciseType.FillBlank => ExerciseTypeSummary.FillBlank,
                Models.Enums.ExerciseType.Flashcard => ExerciseTypeSummary.Flashcard,
                _ => ExerciseTypeSummary.Flashcard
            },
            OrderIndex = pse.OrderIndex
        }).ToList();

        return ApiResponseDto<List<PracticeSetExerciseSummaryDto>>.SuccessResponse(summaries);
    }

    #endregion

    #region Auto-Generation From Tags

    /// <summary>
    /// Auto-generates a practice set from selected tags
    /// Includes ALL exercises that have ANY of the specified tags
    /// </summary>
    public async Task<ApiResponseDto<PracticeSetResponseDto>> GenerateFromTagsAsync(
        GeneratePracticeSetDto dto,
        Guid userId)
    {
        // Find all exercises that have ANY of the specified tags
        var exerciseIds = await _context.ExerciseTags
            .Where(et => dto.TagIds.Contains(et.TagId))
            .Select(et => et.ExerciseId)
            .Distinct()
            .ToListAsync();

        if (!exerciseIds.Any())
        {
            return ApiResponseDto<PracticeSetResponseDto>.ErrorResponse(
                "No exercises found with the specified tags. " +
                "Try selecting different tags or add tags to exercises first.");
        }

        // Verify these exercises belong to the user
        var userExerciseIds = await _context.Exercises
            .Where(e => e.UserId == userId && exerciseIds.Contains(e.ExerciseId))
            .Select(e => e.ExerciseId)
            .ToListAsync();

        if (!userExerciseIds.Any())
        {
            return ApiResponseDto<PracticeSetResponseDto>.ErrorResponse(
                "No matching exercises found that belong to you.");
        }

        // Create the practice set
        var practiceSet = new PracticeSet
        {
            PracticeSetId = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            CreationType = CreationType.AutoGenerated,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.PracticeSets.Add(practiceSet);

        // Add exercises to the set
        int orderIndex = 0;
        foreach (var exId in userExerciseIds)
        {
            _context.PracticeSetExercises.Add(new PracticeSetExercise
            {
                Id = Guid.NewGuid(),
                PracticeSetId = practiceSet.PracticeSetId,
                ExerciseId = exId,
                OrderIndex = orderIndex++
            });
        }

        await _context.SaveChangesAsync();

        // Reload with includes
        practiceSet = await _context.PracticeSets
            .Include(ps => ps.Lesson)
            .Include(ps => ps.PracticeSetExercises)
                .ThenInclude(pse => pse.Exercise)
            .FirstAsync(ps => ps.PracticeSetId == practiceSet.PracticeSetId);

        return ApiResponseDto<PracticeSetResponseDto>.SuccessResponse(
            MapToResponseDto(practiceSet),
            $"Practice set auto-generated with {userExerciseIds.Count} exercises from {dto.TagIds.Count} tag(s)");
    }

    #endregion

    #region Lesson Linking

    /// <summary>
    /// Links a practice set to a lesson
    /// </summary>
    public async Task<ApiResponseDto<PracticeSetResponseDto>> LinkToLessonAsync(
        Guid practiceSetId,
        LinkLessonDto dto,
        Guid userId)
    {
        var set = await _context.PracticeSets
            .FirstOrDefaultAsync(ps => ps.PracticeSetId == practiceSetId && ps.UserId == userId);

        if (set == null)
        {
            return ApiResponseDto<PracticeSetResponseDto>.ErrorResponse("Practice set not found");
        }

        // Verify lesson exists (it might belong to another team member, but should exist)
        var lessonExists = await _context.Lessons.AnyAsync(l => l.LessonId == dto.LessonId);

        if (!lessonExists)
        {
            return ApiResponseDto<PracticeSetResponseDto>.ErrorResponse("Lesson not found");
        }

        set.LessonId = dto.LessonId;
        set.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload
        set = await _context.PracticeSets
            .Include(ps => ps.Lesson)
            .Include(ps => ps.PracticeSetExercises)
                .ThenInclude(pse => pse.Exercise)
            .FirstAsync(ps => ps.PracticeSetId == practiceSetId);

        return ApiResponseDto<PracticeSetResponseDto>.SuccessResponse(
            MapToResponseDto(set),
            "Practice set linked to lesson successfully");
    }

    /// <summary>
    /// Unlinks a practice set from its lesson
    /// </summary>
    public async Task<ApiResponseDto<PracticeSetResponseDto>> UnlinkFromLessonAsync(
        Guid practiceSetId,
        Guid userId)
    {
        var set = await _context.PracticeSets
            .FirstOrDefaultAsync(ps => ps.PracticeSetId == practiceSetId && ps.UserId == userId);

        if (set == null)
        {
            return ApiResponseDto<PracticeSetResponseDto>.ErrorResponse("Practice set not found");
        }

        if (set.LessonId == null)
        {
            return ApiResponseDto<PracticeSetResponseDto>.ErrorResponse("This practice set is not linked to any lesson");
        }

        set.LessonId = null;
        set.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // FIXED: Use Include to load related data for the response
        var reloadedSet = await _context.PracticeSets
            .Include(ps => ps.Lesson)
            .Include(ps => ps.PracticeSetExercises)
                .ThenInclude(pse => pse.Exercise)
            .FirstOrDefaultAsync(ps => ps.PracticeSetId == practiceSetId);

        if (reloadedSet == null)
        {
            return ApiResponseDto<PracticeSetResponseDto>.ErrorResponse("Practice set not found after unlinking");
        }

        return ApiResponseDto<PracticeSetResponseDto>.SuccessResponse(
            MapToResponseDto(reloadedSet),
            "Practice set unlinked from lesson successfully");
    }

    #endregion

    #region Private Helpers

    private PracticeSetResponseDto MapToResponseDto(PracticeSet set)
    {
        var dto = new PracticeSetResponseDto
        {
            PracticeSetId = set.PracticeSetId,
            Title = set.Title,
            Description = set.Description,
            CreationType = set.CreationType,
            LessonId = set.LessonId,
            LessonTitle = set.Lesson?.Title,
            ExerciseCount = set.PracticeSetExercises?.Count ?? 0,
            SessionCount = set.PracticeSessions?.Count ?? 0,
            CreatedAt = set.CreatedAt,
            UpdatedAt = set.UpdatedAt
        };

        // Include exercise summaries if loaded
        if (set.PracticeSetExercises != null && set.PracticeSetExercises.Any())
        {
            dto.Exercises = set.PracticeSetExercises
                .OrderBy(pse => pse.OrderIndex)
                .Select(pse => new PracticeSetExerciseSummaryDto
                {
                    ExerciseId = pse.ExerciseId,
                    Question = pse.Exercise?.Question ?? "Unknown",
                    Type = pse.Exercise?.Type switch
                    {
                        Models.Enums.ExerciseType.MCQ => ExerciseTypeSummary.MCQ,
                        Models.Enums.ExerciseType.FillBlank => ExerciseTypeSummary.FillBlank,
                        Models.Enums.ExerciseType.Flashcard => ExerciseTypeSummary.Flashcard,
                        _ => ExerciseTypeSummary.Flashcard
                    },
                    OrderIndex = pse.OrderIndex
                })
                .ToList();
        }

        return dto;
    }

    #endregion
}