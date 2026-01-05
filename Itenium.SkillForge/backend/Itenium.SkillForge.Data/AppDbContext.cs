using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Entities;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.Data;

public class AppDbContext : ForgeIdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TeamEntity> Teams => Set<TeamEntity>();
    public DbSet<CourseEntity> Courses => Set<CourseEntity>();
    public DbSet<TeamCourseEntity> TeamCourses => Set<TeamCourseEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TeamCourseEntity>(entity =>
        {
            entity.HasOne(tc => tc.Course)
                .WithMany(c => c.TeamCourses)
                .HasForeignKey(tc => tc.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
