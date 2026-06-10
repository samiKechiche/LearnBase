using Microsoft.EntityFrameworkCore;
using LearnBase.API.Models;

namespace LearnBase.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // ══════════════════════════════════════════════════════════════════
    // DBSETS - SAIFEDINE'S MODULE (Authentication)
    // ══════════════════════════════════════════════════════════════════

    public DbSet<User> Users { get; set; }

    // ══════════════════════════════════════════════════════════════════
    // DBSETS - SAMI'S MODULES (Exercises, Practice Sessions, History)
    // ══════════════════════════════════════════════════════════════════

    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<ExerciseOption> ExerciseOptions { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ExerciseTag> ExerciseTags { get; set; }
    public DbSet<PracticeSet> PracticeSets { get; set; }
    public DbSet<PracticeSetExercise> PracticeSetExercises { get; set; }
    public DbSet<PracticeSession> PracticeSessions { get; set; }
    public DbSet<SessionExerciseResult> SessionExerciseResults { get; set; }

    // ══════════════════════════════════════════════════════════════════
    // DBSETS - YOUSSEF'S MODULE (Lessons, Notes, Files)
    // ══════════════════════════════════════════════════════════════════

    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<AppFile> Files { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ═══════════════════════════════════════════════════════════
        // COMPOSITE KEYS (Junction Tables)
        // ═══════════════════════════════════════════════════════════

        //modelBuilder.Entity<ExerciseTag>()
        //    .HasKey(et => new { et.ExerciseId, et.TagId });

        //modelBuilder.Entity<PracticeSetExercise>()
        //    .HasKey(pse => new { pse.PracticeSetId, pse.ExerciseId });
        // ADD unique indexes instead:
        modelBuilder.Entity<ExerciseTag>()
            .HasIndex(et => new { et.ExerciseId, et.TagId })
            .IsUnique()
            .HasDatabaseName("IX_ExerciseTag_ExerciseId_TagId_Unique");

        modelBuilder.Entity<PracticeSetExercise>()
            .HasIndex(pse => new { pse.PracticeSetId, pse.ExerciseId })
            .IsUnique()
            .HasDatabaseName("IX_PracticeSetExercise_SetId_ExerciseId_Unique");

        // ═══════════════════════════════════════════════════════════
        // UNIQUE CONSTRAINTS
        // ═══════════════════════════════════════════════════════════

        // User cannot have duplicate tag names
        modelBuilder.Entity<Tag>()
            .HasIndex(t => new { t.UserId, t.Name })
            .IsUnique()
            .HasDatabaseName("IX_Tags_UserId_Name_Unique");

        // ═══════════════════════════════════════════════════════════
        // INDEXES (Performance Optimization)
        // ═══════════════════════════════════════════════════════════

        modelBuilder.Entity<Exercise>().HasIndex(e => e.UserId);
        modelBuilder.Entity<Tag>().HasIndex(t => t.UserId);
        modelBuilder.Entity<PracticeSet>().HasIndex(ps => ps.UserId);
        modelBuilder.Entity<PracticeSession>().HasIndex(s => s.UserId);
        modelBuilder.Entity<PracticeSession>().HasIndex(s => s.PracticeSetId);
        modelBuilder.Entity<SessionExerciseResult>().HasIndex(r => r.SessionId);
        modelBuilder.Entity<Lesson>().HasIndex(l => l.UserId);
        modelBuilder.Entity<Note>().HasIndex(n => n.LessonId);
        modelBuilder.Entity<AppFile>().HasIndex(f => f.LessonId);

        // ═══════════════════════════════════════════════════════════
        // USER COMPOSITION RELATIONSHIPS (Cascade Delete on User)
        // When User is deleted → Everything is deleted
        // ═══════════════════════════════════════════════════════════

        // User → Exercises
        modelBuilder.Entity<Exercise>()
            .HasOne(e => e.User)
            .WithMany(u => u.Exercises)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User → Tags
        modelBuilder.Entity<Tag>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tags)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User → PracticeSets
        modelBuilder.Entity<PracticeSet>()
            .HasOne(ps => ps.User)
            .WithMany(u => u.PracticeSets)
            .HasForeignKey(ps => ps.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User → PracticeSessions
        modelBuilder.Entity<PracticeSession>()
            .HasOne(s => s.User)
            .WithMany(u => u.PracticeSessions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User → Lessons
        modelBuilder.Entity<Lesson>()
            .HasOne(l => l.User)
            .WithMany(u => u.Lessons)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ═══════════════════════════════════════════════════════════
        // EXERCISE COMPOSITION
        // ═══════════════════════════════════════════════════════════

        // Exercise → ExerciseOptions (Cascade: deleting exercise removes its MCQ options)
        modelBuilder.Entity<ExerciseOption>()
            .HasOne(eo => eo.Exercise)
            .WithMany(e => e.ExerciseOptions)
            .HasForeignKey(eo => eo.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        // ═══════════════════════════════════════════════════════════
        // LESSON COMPOSITION (Youssef's Models)
        // ═══════════════════════════════════════════════════════════

        // Lesson → Notes (Cascade: deleting lesson removes all notes)
        modelBuilder.Entity<Note>()
            .HasOne(n => n.Lesson)
            .WithMany(l => l.Notes)
            .HasForeignKey(n => n.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        // Lesson → AppFiles (Cascade: deleting lesson removes all files)
        modelBuilder.Entity<AppFile>()
            .HasOne(f => f.Lesson)
            .WithMany(l => l.Files)
            .HasForeignKey(f => f.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        // ═══════════════════════════════════════════════════════════
        // PRACTICE SET ↔ LESSON (Optional Link)
        // ═══════════════════════════════════════════════════════════

        // PracticeSet → Lesson (Optional: SetNull when lesson deleted)
        // If a Lesson is deleted, linked PracticeSets are NOT deleted,
        // they just lose their link (LessonId becomes null)
        modelBuilder.Entity<PracticeSet>()
            .HasOne(ps => ps.Lesson)
            .WithMany(l => l.PracticeSets)
            .HasForeignKey(ps => ps.LessonId)
            .OnDelete(DeleteBehavior.SetNull);

        // ═══════════════════════════════════════════════════════════
        // MANY-TO-MANY: Exercise ↔ Tag (via ExerciseTag)
        // ═══════════════════════════════════════════════════════════

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

        // ═══════════════════════════════════════════════════════════
        // MANY-TO-MANY: PracticeSet ↔ Exercise (via PracticeSetExercise)
        // ═══════════════════════════════════════════════════════════

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

        // ═══════════════════════════════════════════════════════════
        // PRACTICE SESSION RELATIONSHIPS
        // ═══════════════════════════════════════════════════════════

        // PracticeSession → PracticeSet (RESTRICT!)
        // Prevents deleting a PracticeSet that has session history.
        // This protects historical data from accidental deletion.
        modelBuilder.Entity<PracticeSession>()
            .HasOne(s => s.PracticeSet)
            .WithMany(ps => ps.PracticeSessions)
            .HasForeignKey(s => s.PracticeSetId)
            .OnDelete(DeleteBehavior.Restrict);

        // PracticeSession → SessionExerciseResults (Cascade)
        modelBuilder.Entity<SessionExerciseResult>()
            .HasOne(r => r.Session)
            .WithMany(s => s.SessionExerciseResults)
            .HasForeignKey(r => r.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // SessionExerciseResult → Exercise (SetNull: results survive even if exercise deleted)
        modelBuilder.Entity<SessionExerciseResult>()
            .HasOne(r => r.Exercise)
            .WithMany()
            .HasForeignKey(r => r.ExerciseId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}