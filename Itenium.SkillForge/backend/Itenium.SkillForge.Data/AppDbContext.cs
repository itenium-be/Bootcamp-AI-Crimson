using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Entities;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.Data;

public class AppDbContext : ForgeIdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<OrganizationEntity> Organizations => Set<OrganizationEntity>();
    public DbSet<CourseEntity> Courses => Set<CourseEntity>();
    public DbSet<OrganizationCourseEntity> OrganizationCourses => Set<OrganizationCourseEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<OrganizationCourseEntity>(entity =>
        {
            entity.HasOne(oc => oc.Course)
                .WithMany(c => c.OrganizationCourses)
                .HasForeignKey(oc => oc.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
