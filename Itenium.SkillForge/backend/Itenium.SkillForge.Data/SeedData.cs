using System.Security.Claims;
using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Itenium.SkillForge.Data;

public static class SeedData
{
    public static async Task SeedDevelopmentData(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedOrganizations(db);
        await SeedCourses(db);
        await app.SeedTestUsers();
    }

    private static async Task SeedOrganizations(AppDbContext db)
    {
        if (!await db.Organizations.AnyAsync())
        {
            db.Organizations.AddRange(
                new OrganizationEntity { Id = 1, Name = "Acme Corp" },
                new OrganizationEntity { Id = 2, Name = "TechStart Inc" },
                new OrganizationEntity { Id = 3, Name = "Global Learning" }
            );
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedCourses(AppDbContext db)
    {
        if (!await db.Courses.AnyAsync())
        {
            db.Courses.AddRange(
                new CourseEntity { Id = 1, Name = "Introduction to Programming", Description = "Learn the basics of programming", Category = "Development", Level = "Beginner" },
                new CourseEntity { Id = 2, Name = "Advanced C#", Description = "Master C# programming language", Category = "Development", Level = "Advanced" },
                new CourseEntity { Id = 3, Name = "Cloud Architecture", Description = "Design scalable cloud solutions", Category = "Architecture", Level = "Intermediate" },
                new CourseEntity { Id = 4, Name = "Agile Project Management", Description = "Learn agile methodologies", Category = "Management", Level = "Beginner" }
            );
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedTestUsers(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ForgeUser>>();

        // Central admin - no organization claim (manages all)
        if (await userManager.FindByEmailAsync("central@test.local") == null)
        {
            var admin = new ForgeUser
            {
                UserName = "central",
                Email = "central@test.local",
                EmailConfirmed = true,
                FirstName = "Central",
                LastName = "Admin"
            };
            var result = await userManager.CreateAsync(admin, "AdminPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRolesAsync(admin, ["central"]);
            }
        }

        // Local user for Acme Corp only
        if (await userManager.FindByEmailAsync("acme@test.local") == null)
        {
            var user = new ForgeUser
            {
                UserName = "acme",
                Email = "acme@test.local",
                EmailConfirmed = true,
                FirstName = "Local",
                LastName = "Acme"
            };
            var result = await userManager.CreateAsync(user, "UserPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "local");
                await userManager.AddClaimAsync(user, new Claim("organization", "1")); // Acme Corp
            }
        }

        // Local user for TechStart only
        if (await userManager.FindByEmailAsync("techstart@test.local") == null)
        {
            var user = new ForgeUser
            {
                UserName = "techstart",
                Email = "techstart@test.local",
                EmailConfirmed = true,
                FirstName = "Local",
                LastName = "TechStart"
            };
            var result = await userManager.CreateAsync(user, "UserPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "local");
                await userManager.AddClaimAsync(user, new Claim("organization", "2")); // TechStart
            }
        }

        // Regional user with access to multiple organizations (Acme + TechStart)
        if (await userManager.FindByEmailAsync("regional@test.local") == null)
        {
            var user = new ForgeUser
            {
                UserName = "regional",
                Email = "regional@test.local",
                EmailConfirmed = true,
                FirstName = "Regional",
                LastName = "Manager"
            };
            var result = await userManager.CreateAsync(user, "UserPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "local");
                await userManager.AddClaimAsync(user, new Claim("organization", "1")); // Acme Corp
                await userManager.AddClaimAsync(user, new Claim("organization", "2")); // TechStart
            }
        }
    }
}
