using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LearnBase.API.Data;
using LearnBase.API.Models;
using LearnBase.API.DTOs.ImportExport;
using Microsoft.EntityFrameworkCore;

namespace LearnBase.API.Services
{
    public class ImportExportService
    {
        private readonly ApplicationDbContext _context;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public ImportExportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(byte[] Content, string FileName)?> ExportLessonAsync(Guid lessonId)
        {
            var lesson = await _context.Lessons
                .AsNoTracking()
                .Include(l => l.Notes)
                .Include(l => l.Files)
                .Include(l => l.PracticeSets)
                    .ThenInclude(ps => ps.PracticeSetExercises)
                        .ThenInclude(pse => pse.Exercise)
                            .ThenInclude(e => e.ExerciseOptions)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            if (lesson == null)
                return null;

            var dto = new LessonExportDto
            {
                Title = lesson.Title,
                Description = lesson.Description,
                CreatedAt = lesson.CreatedAt,
                UpdatedAt = lesson.UpdatedAt,
                Notes = lesson.Notes.Select(n => new NoteExportDto { Content = n.Content, CreatedAt = n.CreatedAt, UpdatedAt = n.UpdatedAt }).ToList(),
                Files = lesson.Files.Select(f => new FileExportDto { FileName = f.FileName, FileType = f.FileType, FilePath = f.FilePath, FileSizeBytes = f.FileSizeBytes }).ToList(),
                PracticeSets = lesson.PracticeSets.Select(ps => new PracticeSetExportDto
                {
                    Title = ps.Title,
                    Description = ps.Description,
                    CreationType = ps.CreationType,
                    CreatedAt = ps.CreatedAt,
                    UpdatedAt = ps.UpdatedAt,
                    Exercises = ps.PracticeSetExercises
                        .OrderBy(pse => pse.OrderIndex)
                        .Select(pse =>
                        {
                            var e = pse.Exercise!;
                            return new ExerciseExportDto
                            {
                                Type = e.Type,
                                Question = e.Question,
                                Answer = e.Answer,
                                CreatedAt = e.CreatedAt,
                                UpdatedAt = e.UpdatedAt,
                                Options = e.ExerciseOptions
                                            .OrderBy(o => o.OrderIndex)
                                            .Select(o => new ExerciseOptionExportDto { Content = o.Content, OrderIndex = o.OrderIndex })
                                            .ToList(),
                                // Correct option exported as index relative to ordered Options (or null)
                                CorrectOptionIndex = e.CorrectOptionId == null ? (int?)null :
                                    e.ExerciseOptions
                                        .OrderBy(o => o.OrderIndex)
                                        .Select((opt, idx) => new { opt.OptionId, idx })
                                        .FirstOrDefault(x => x.OptionId == e.CorrectOptionId)?.idx
                            };
                        }).ToList()
                }).ToList()
            };

            var bytes = JsonSerializer.SerializeToUtf8Bytes(dto, _jsonOptions);
            var fileName = $"lesson-{lesson.LessonId}.json";
            return (bytes, fileName);
        }

        public async Task<(byte[] Content, string FileName)?> ExportPracticeSetAsync(Guid practiceSetId)
        {
            var ps = await _context.PracticeSets
                .AsNoTracking()
                .Include(p => p.PracticeSetExercises)
                    .ThenInclude(pse => pse.Exercise)
                        .ThenInclude(e => e.ExerciseOptions)
                .FirstOrDefaultAsync(p => p.PracticeSetId == practiceSetId);

            if (ps == null)
                return null;

            var dto = new PracticeSetExportDto
            {
                Title = ps.Title,
                Description = ps.Description,
                CreationType = ps.CreationType,
                CreatedAt = ps.CreatedAt,
                UpdatedAt = ps.UpdatedAt,
                Exercises = ps.PracticeSetExercises
                    .OrderBy(pse => pse.OrderIndex)
                    .Select(pse =>
                    {
                        var e = pse.Exercise!;
                        return new ExerciseExportDto
                        {
                            Type = e.Type,
                            Question = e.Question,
                            Answer = e.Answer,
                            CreatedAt = e.CreatedAt,
                            UpdatedAt = e.UpdatedAt,
                            Options = e.ExerciseOptions
                                        .OrderBy(o => o.OrderIndex)
                                        .Select(o => new ExerciseOptionExportDto { Content = o.Content, OrderIndex = o.OrderIndex })
                                        .ToList(),
                            CorrectOptionIndex = e.CorrectOptionId == null ? (int?)null :
                                e.ExerciseOptions
                                    .OrderBy(o => o.OrderIndex)
                                    .Select((opt, idx) => new { opt.OptionId, idx })
                                    .FirstOrDefault(x => x.OptionId == e.CorrectOptionId)?.idx
                        };
                    }).ToList()
            };

            var bytes = JsonSerializer.SerializeToUtf8Bytes(dto, _jsonOptions);
            var fileName = $"practiceset-{ps.PracticeSetId}.json";
            return (bytes, fileName);
        }

        public async Task<Guid> ImportLessonAsync(IFormFile file, Guid userId)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            var dto = JsonSerializer.Deserialize<LessonExportDto>(ms.ToArray(), _jsonOptions)
                      ?? throw new InvalidOperationException("Invalid lesson file format.");

            var lesson = new Lesson
            {
                LessonId = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            // Notes
            foreach (var n in dto.Notes ?? Enumerable.Empty<NoteExportDto>())
            {
                lesson.Notes.Add(new Note
                {
                    NoteId = Guid.NewGuid(),
                    Content = n.Content,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // Lesson Files (todo later when youssef finishes this part)
            //// Files: only metadata included in export — the physical file bytes are not transported.
            //foreach (var f in dto.Files ?? Enumerable.Empty<FileExportDto>())
            //{
            //    lesson.Files.Add(new AppFile
            //    {
            //        FileId = Guid.NewGuid(),
            //        FileName = f.FileName,
            //        FileType = f.FileType,
            //        FilePath = f.FilePath,
            //        FileSizeBytes = f.FileSizeBytes,
            //        CreatedAt = DateTime.UtcNow
            //    });
            //}

            // PracticeSets + Exercises
            if (dto.PracticeSets != null)
            {
                foreach (var psDto in dto.PracticeSets)
                {
                    var ps = new PracticeSet
                    {
                        PracticeSetId = Guid.NewGuid(),
                        Title = psDto.Title,
                        Description = psDto.Description,
                        CreationType = psDto.CreationType,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Lesson = lesson,
                        LessonId = lesson.LessonId
                    };

                    // Create exercises for this practice set
                    var exerciseMap = new List<(Guid NewExerciseId, Exercise exercise)>();
                    if (psDto.Exercises != null)
                    {
                        foreach (var exDto in psDto.Exercises)
                        {
                            var ex = new Exercise
                            {
                                ExerciseId = Guid.NewGuid(),
                                Type = exDto.Type,
                                Question = exDto.Question,
                                Answer = exDto.Answer,
                                UserId = userId,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            // Options
                            if (exDto.Options != null && exDto.Options.Count > 0)
                            {
                                int order = 1;
                                foreach (var optDto in exDto.Options)
                                {
                                    var opt = new ExerciseOption
                                    {
                                        OptionId = Guid.NewGuid(),
                                        ExerciseId = ex.ExerciseId,
                                        Content = optDto.Content,
                                        OrderIndex = optDto.OrderIndex
                                    };
                                    ex.ExerciseOptions.Add(opt);
                                    order++;
                                }

                                // Set CorrectOptionId based on exported index
                                if (exDto.CorrectOptionIndex.HasValue)
                                {
                                    var idx = exDto.CorrectOptionIndex.Value;
                                    var opt = ex.ExerciseOptions.OrderBy(o => o.OrderIndex).ElementAtOrDefault(idx);
                                    if (opt != null)
                                        ex.CorrectOptionId = opt.OptionId;
                                }
                            }

                            _context.Exercises.Add(ex);
                            exerciseMap.Add((ex.ExerciseId, ex));
                        }

                        // Add PracticeSetExercises preserving order
                        for (int i = 0; i < exerciseMap.Count; i++)
                        {
                            ps.PracticeSetExercises.Add(new PracticeSetExercise
                            {
                                Id = Guid.NewGuid(),
                                ExerciseId = exerciseMap[i].NewExerciseId,
                                PracticeSetId = ps.PracticeSetId,
                                OrderIndex = i
                            });
                        }
                    }

                    _context.PracticeSets.Add(ps);
                    lesson.PracticeSets.Add(ps);
                }
            }

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            return lesson.LessonId;
        }

        public async Task<Guid> ImportPracticeSetAsync(IFormFile file, Guid userId)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            var dto = JsonSerializer.Deserialize<PracticeSetExportDto>(ms.ToArray(), _jsonOptions)
                      ?? throw new InvalidOperationException("Invalid practice set file format.");

            var ps = new PracticeSet
            {
                PracticeSetId = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                CreationType = dto.CreationType,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var exerciseMap = new List<(Guid NewExerciseId, Exercise exercise)>();
            if (dto.Exercises != null)
            {
                foreach (var exDto in dto.Exercises)
                {
                    var ex = new Exercise
                    {
                        ExerciseId = Guid.NewGuid(),
                        Type = exDto.Type,
                        Question = exDto.Question,
                        Answer = exDto.Answer,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    if (exDto.Options != null && exDto.Options.Count > 0)
                    {
                        foreach (var optDto in exDto.Options)
                        {
                            var opt = new ExerciseOption
                            {
                                OptionId = Guid.NewGuid(),
                                ExerciseId = ex.ExerciseId,
                                Content = optDto.Content,
                                OrderIndex = optDto.OrderIndex
                            };
                            ex.ExerciseOptions.Add(opt);
                        }

                        if (exDto.CorrectOptionIndex.HasValue)
                        {
                            var idx = exDto.CorrectOptionIndex.Value;
                            var opt = ex.ExerciseOptions.OrderBy(o => o.OrderIndex).ElementAtOrDefault(idx);
                            if (opt != null)
                                ex.CorrectOptionId = opt.OptionId;
                        }
                    }

                    _context.Exercises.Add(ex);
                    exerciseMap.Add((ex.ExerciseId, ex));
                }

                for (int i = 0; i < exerciseMap.Count; i++)
                {
                    ps.PracticeSetExercises.Add(new PracticeSetExercise
                    {
                        Id = Guid.NewGuid(),
                        ExerciseId = exerciseMap[i].NewExerciseId,
                        PracticeSetId = ps.PracticeSetId,
                        OrderIndex = i
                    });
                }
            }

            _context.PracticeSets.Add(ps);
            await _context.SaveChangesAsync();
            return ps.PracticeSetId;
        }
    }
}
