using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Entities;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.Data;

public class AppDbContext : ForgeIdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TeamEntity> Teams => Set<TeamEntity>();

    public DbSet<CourseEntity> Courses => Set<CourseEntity>();

    public DbSet<QuizEntity> Quizzes => Set<QuizEntity>();

    public DbSet<QuestionEntity> Questions => Set<QuestionEntity>();

    public DbSet<QuizAttemptEntity> QuizAttempts => Set<QuizAttemptEntity>();

    public DbSet<QuestionResponseEntity> QuestionResponses => Set<QuestionResponseEntity>();

    public DbSet<EnrollmentEntity> Enrollments => Set<EnrollmentEntity>();

    public DbSet<CourseAssignmentEntity> CourseAssignments => Set<CourseAssignmentEntity>();

    public DbSet<LessonEntity> Lessons => Set<LessonEntity>();

    public DbSet<LessonStatusEntity> LessonStatuses => Set<LessonStatusEntity>();

    public DbSet<CourseFeedbackEntity> CourseFeedbacks => Set<CourseFeedbackEntity>();

    public DbSet<ModuleEntity> Modules => Set<ModuleEntity>();

    public DbSet<LessonProgressEntity> LessonProgresses => Set<LessonProgressEntity>();

    public DbSet<ContentBlockEntity> ContentBlocks => Set<ContentBlockEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<LessonStatusEntity>()
            .HasKey(s => new { s.UserId, s.LessonId });

        builder.Entity<LessonProgressEntity>()
            .HasKey(p => new { p.UserId, p.LessonId });

        builder.Entity<EnrollmentEntity>()
            .HasIndex(e => new { e.UserId, e.CourseId })
            .IsUnique();
    }
}
