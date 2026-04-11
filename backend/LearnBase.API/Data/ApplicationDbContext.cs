using Microsoft.EntityFrameworkCore;
using LearnBase.API.Models;

namespace LearnBase.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // =====================================================
    // DBSETS - EXISTING (SAIFEDINE'S - USER MODEL)
    // =====================================================
    public DbSet<User> Users { get; set; }

    // =====================================================
    // DBSETS - SAMI'S MODELS (EXERCISE/PRACTICE MODULE)
    // =====================================================
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<ExerciseOption> ExerciseOptions { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ExerciseTag> ExerciseTags { get; set; }
    public DbSet<PracticeSet> PracticeSets { get; set; }
    public DbSet<PracticeSetExercise> PracticeSetExercises { get; set; }
    public DbSet<PracticeSession> PracticeSessions { get; set; }
    public DbSet<SessionExerciseResult> SessionExerciseResults { get; set; }

    // =====================================================
    // DBSETS - YOUSSEF'S MODELS (LESSON MODULE) - HE WILL ADD
    // =====================================================
    // public DbSet<Lesson> Lessons { get; set; }
    // public DbSet<Note> Notes { get; set; }
    // public DbSet<AppFile> Files { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ================================================
        // COMPOSITE KEYS FOR JUNCTION TABLES
        // ================================================

        modelBuilder.Entity<ExerciseTag>()
            .HasKey(et => new { et.ExerciseId, et.TagId });

        modelBuilder.Entity<PracticeSetExercise>()
            .HasKey(pse => new { pse.PracticeSetId, pse.ExerciseId });

        // ================================================
        // UNIQUE CONSTRAINTS
        // ================================================

        // User cannot have duplicate tag names
        modelBuilder.Entity<Tag>()
            .HasIndex(t => new { t.UserId, t.Name })
            .IsUnique()
            .HasDatabaseName("IX_Tags_UserId_Name_Unique");

        // ================================================
        // INDEXES FOR PERFORMANCE
        // ================================================

        modelBuilder.Entity<Exercise>()
            .HasIndex(e => e.UserId);

        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.UserId);

        modelBuilder.Entity<PracticeSet>()
            .HasIndex(ps => ps.UserId);

        modelBuilder.Entity<PracticeSession>()
            .HasIndex(s => s.UserId);

        modelBuilder.Entity<PracticeSession>()
            .HasIndex(s => s.PracticeSetId);

        modelBuilder.Entity<SessionExerciseResult>()
            .HasIndex(r => r.SessionId);

        // ================================================
        // RELATIONSHIP CONFIGURATIONS
        // ================================================

        // --- USER COMPOSITION RELATIONSHIPS (Cascade Delete) ---

        // User -> Exercises
        modelBuilder.Entity<Exercise>()
            .HasOne(e => e.User)
            .WithMany(u => u.Exercises)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> Tags
        modelBuilder.Entity<Tag>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tags)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> PracticeSets
        modelBuilder.Entity<PracticeSet>()
            .HasOne(ps => ps.User)
            .WithMany(u => u.PracticeSets)
            .HasForeignKey(ps => ps.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> PracticeSessions
        modelBuilder.Entity<PracticeSession>()
            .HasOne(s => s.User)
            .WithMany(u => u.PracticeSessions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- EXERCISE COMPOSITION ---

        // Exercise -> ExerciseOptions (cascade)
        modelBuilder.Entity<ExerciseOption>()
            .HasOne(eo => eo.Exercise)
            .WithMany(e => e.ExerciseOptions)
            .HasForeignKey(eo => eo.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Exercise -> CorrectOption (self-referencing, optional)
        modelBuilder.Entity<Exercise>()
            .HasOne(e => e.ExerciseOptions.FirstOrDefault(o => o.OptionId == e.CorrectOptionId.Value))
            // Note: This is handled implicitly by CorrectOptionId FK

        // --- MANY-TO-MANY: Exercise <-> Tag ---
        modelBuilder.Entity<ExerciseTag>()
            .HasOne(et => et.Exercise)
            .WithMany(e => e.ExerciseTags)
            .HasForeignKey(et => et.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExerciseTag>()
            .HasOne(et => et.Tag)
            .WithMany(t => t.ExerciseTags)
            .HasForeignKey(et => et.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- PRACTICE SET RELATIONSHIPS ---

        // PracticeSet -> Lesson (Optional, SetNull on delete)
        modelBuilder.Entity<PracticeSet>()
            .HasOne(ps => ps.Lesson)
            // Note: Youssef will add .WithMany(l => l.PracticeSets) to Lesson model
            .WithList() // Temporary placeholder until Lesson model exists
            .HasForeignKey(ps => ps.LessonId)
            .OnDelete(DeleteBehavior.SetNull);

        // --- MANY-TO-MANY: PracticeSet <-> Exercise ---
        modelBuilder.Entity<PracticeSetExercise>()
            .HasOne(pse => pse.PracticeSet)
            .WithMany(ps => ps.PracticeSetExercises)
            .HasForeignKey(pse => pse.PracticeSetId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PracticeSetExercise>()
            .HasOne(pse => pse.Exercise)
            .WithMany(e => e.PracticeSetExercises)
            .HasForeignKey(pse => pse.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- PRACTICE SESSION RELATIONSHIPS ---

        // PracticeSession -> PracticeSet (RESTRICT - protect history)
        modelBuilder.Entity<PracticeSession>()
            .HasOne(s => s.PracticeSet)
            .WithMany(ps => ps.PracticeSessions)
            .HasForeignKey(s => s.PracticeSetId)
            .OnDelete(DeleteBehavior.Restrict);

        // PracticeSession -> SessionExerciseResults (Cascade)
        modelBuilder.Entity<SessionExerciseResult>()
            .HasOne(r => r.Session)
            .WithMany(s => s.SessionExerciseResults)
            .HasForeignKey(r => r.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // SessionExerciseResult -> Exercise (optional, no cascade)
        modelBuilder.Entity<SessionExerciseResult>()
            .HasOne(r => r.Exercise)
            .WithMany()
            .HasForeignKey(r => r.ExerciseId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}