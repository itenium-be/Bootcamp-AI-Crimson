using System.Security.Claims;
using System.Text.Json;
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

        await SeedTeams(db);
        await SeedCourses(db);
        await app.SeedTestUsers();

        // Re-open scope so user IDs are resolvable after creation
        using var scope2 = app.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager2 = scope2.ServiceProvider.GetRequiredService<UserManager<ForgeUser>>();
        await SeedLessonsAndContent(db2);
        await SeedEnrollmentsAndProgress(db2, userManager2);
        await SeedFeedback(db2, userManager2);
        await SeedAnnotations(db2, userManager2);
    }

    private static async Task SeedTeams(AppDbContext db)
    {
        if (!await db.Teams.AnyAsync())
        {
            db.Teams.AddRange(
                new TeamEntity { Id = 1, Name = "Java" },
                new TeamEntity { Id = 2, Name = ".NET" },
                new TeamEntity { Id = 3, Name = "PO & Analysis" },
                new TeamEntity { Id = 4, Name = "QA" });
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedCourses(AppDbContext db)
    {
        if (!await db.Courses.AnyAsync())
        {
            db.Courses.AddRange(
                new CourseEntity
                {
                    Id = 1,
                    Name = "Introduction to Programming",
                    Description = "Learn the basics of programming with hands-on exercises covering variables, loops, and functions.",
                    Category = "Development",
                    Level = "Beginner",
                    Status = CourseStatus.Published,
                    EstimatedDuration = 120
                },
                new CourseEntity
                {
                    Id = 2,
                    Name = "Advanced C#",
                    Description = "Master C# programming: LINQ, async/await, generics, and modern .NET patterns.",
                    Category = "Development",
                    Level = "Advanced",
                    Status = CourseStatus.Published,
                    EstimatedDuration = 240
                },
                new CourseEntity
                {
                    Id = 3,
                    Name = "Cloud Architecture",
                    Description = "Design scalable cloud solutions using Azure services, microservices, and event-driven patterns.",
                    Category = "Architecture",
                    Level = "Intermediate",
                    Status = CourseStatus.Published,
                    EstimatedDuration = 180
                },
                new CourseEntity
                {
                    Id = 4,
                    Name = "Agile Project Management",
                    Description = "Learn agile methodologies: Scrum, Kanban, sprint planning, and retrospectives.",
                    Category = "Management",
                    Level = "Beginner",
                    Status = CourseStatus.Published,
                    EstimatedDuration = 90
                },
                new CourseEntity
                {
                    Id = 5,
                    Name = "Docker & Kubernetes",
                    Description = "Container fundamentals, Docker Compose, and deploying workloads to Kubernetes clusters.",
                    Category = "DevOps",
                    Level = "Intermediate",
                    Status = CourseStatus.Draft,
                    EstimatedDuration = 200
                });
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedLessonsAndContent(AppDbContext db)
    {
        if (await db.Set<LessonEntity>().AnyAsync())
            return;

        var lessons = new List<LessonEntity>
        {
            // Course 1: Introduction to Programming
            new() { Id = 1,  CourseId = 1, Title = "What is Programming?",      SortOrder = 1, EstimatedDuration = 15 },
            new() { Id = 2,  CourseId = 1, Title = "Variables and Data Types",  SortOrder = 2, EstimatedDuration = 25 },
            new() { Id = 3,  CourseId = 1, Title = "Loops and Conditionals",    SortOrder = 3, EstimatedDuration = 30 },
            // Course 2: Advanced C#
            new() { Id = 4,  CourseId = 2, Title = "LINQ Fundamentals",         SortOrder = 1, EstimatedDuration = 40 },
            new() { Id = 5,  CourseId = 2, Title = "Async / Await Deep Dive",   SortOrder = 2, EstimatedDuration = 45 },
            new() { Id = 6,  CourseId = 2, Title = "Generics and Constraints",  SortOrder = 3, EstimatedDuration = 35 },
            // Course 3: Cloud Architecture
            new() { Id = 7,  CourseId = 3, Title = "Cloud Fundamentals",        SortOrder = 1, EstimatedDuration = 30 },
            new() { Id = 8,  CourseId = 3, Title = "Microservices Patterns",    SortOrder = 2, EstimatedDuration = 50 },
            // Course 4: Agile
            new() { Id = 9,  CourseId = 4, Title = "Scrum Roles & Ceremonies",  SortOrder = 1, EstimatedDuration = 20 },
            new() { Id = 10, CourseId = 4, Title = "Kanban and Flow",           SortOrder = 2, EstimatedDuration = 20 },
        };
        db.Set<LessonEntity>().AddRange(lessons);
        await db.SaveChangesAsync();

        var blocks = new List<ContentBlockEntity>
        {
            new() { LessonId = 1, Type = "text", Order = 1, Content = Json("markdown",
                "## What is Programming?\n\nProgramming is the process of giving instructions to a computer.\n\n" +
                "A **program** is a sequence of instructions written in a language the computer can understand.\n\n" +
                "### Why learn programming?\n- Automate repetitive tasks\n- Build software products\n- Solve real-world problems") },

            new() { LessonId = 2, Type = "text", Order = 1, Content = Json("markdown",
                "## Variables and Data Types\n\nA **variable** is a named storage location.\n\n" +
                "```csharp\nint age = 25;\nstring name = \"Alice\";\nbool isActive = true;\n```\n\n" +
                "### Common types\n| Type | Example |\n|------|---------|\n| `int` | 42 |\n| `string` | \"hello\" |\n| `bool` | true |") },

            new() { LessonId = 3, Type = "text", Order = 1, Content = Json("markdown",
                "## Loops and Conditionals\n\n### If / Else\n```csharp\nif (age >= 18)\n    Console.WriteLine(\"Adult\");\nelse\n    Console.WriteLine(\"Minor\");\n```\n\n" +
                "### For loop\n```csharp\nfor (int i = 0; i < 5; i++)\n    Console.WriteLine(i);\n```") },

            new() { LessonId = 4, Type = "text", Order = 1, Content = Json("markdown",
                "## LINQ Fundamentals\n\nLINQ (Language Integrated Query) lets you query collections in a declarative style.\n\n" +
                "```csharp\nvar evens = numbers.Where(n => n % 2 == 0).ToList();\nvar names = people.Select(p => p.Name).OrderBy(n => n);\n```") },

            new() { LessonId = 5, Type = "text", Order = 1, Content = Json("markdown",
                "## Async / Await\n\nAsync programming prevents blocking the calling thread.\n\n" +
                "```csharp\npublic async Task<string> FetchDataAsync()\n{\n    var result = await httpClient.GetStringAsync(url);\n    return result;\n}\n```\n\n" +
                "> Always `await` async calls — never use `.Result` or `.Wait()`.") },

            new() { LessonId = 6, Type = "text", Order = 1, Content = Json("markdown",
                "## Generics and Constraints\n\n```csharp\npublic T Max<T>(T a, T b) where T : IComparable<T>\n    => a.CompareTo(b) > 0 ? a : b;\n```\n\n" +
                "Constraints: `where T : class`, `new()`, `IComparable<T>`, `struct`") },

            new() { LessonId = 7, Type = "text", Order = 1, Content = Json("markdown",
                "## Cloud Fundamentals\n\n### Key concepts\n- **IaaS** — Infrastructure as a Service (VMs, networking)\n- **PaaS** — Platform as a Service (App Service, Functions)\n- **SaaS** — Software as a Service (Office 365, GitHub)\n\n" +
                "### Azure regions and availability zones\nAzure has 60+ regions worldwide, each with multiple availability zones for resilience.") },

            new() { LessonId = 8, Type = "text", Order = 1, Content = Json("markdown",
                "## Microservices Patterns\n\n### Common patterns\n- **API Gateway** — single entry point for all clients\n- **Saga** — distributed transactions via choreography or orchestration\n- **Circuit Breaker** — fail fast when a downstream service is unhealthy\n- **CQRS** — separate read and write models") },

            new() { LessonId = 9, Type = "text", Order = 1, Content = Json("markdown",
                "## Scrum Roles & Ceremonies\n\n### Roles\n| Role | Responsibility |\n|------|----------------|\n| Product Owner | Owns the backlog |\n| Scrum Master | Removes impediments |\n| Dev Team | Delivers increment |\n\n" +
                "### Ceremonies\n- Sprint Planning, Daily Standup, Sprint Review, Retrospective") },

            new() { LessonId = 10, Type = "text", Order = 1, Content = Json("markdown",
                "## Kanban and Flow\n\nKanban is a pull-based system focused on limiting WIP (Work In Progress).\n\n" +
                "### Key metrics\n- **Lead time** — time from request to delivery\n- **Cycle time** — time from start to finish\n- **Throughput** — items delivered per period") },
        };
        db.Set<ContentBlockEntity>().AddRange(blocks);
        await db.SaveChangesAsync();
    }

    private static async Task SeedEnrollmentsAndProgress(AppDbContext db, UserManager<ForgeUser> userManager)
    {
        if (await db.Enrollments.AnyAsync())
            return;

        var learner = await userManager.FindByEmailAsync("learner@test.local");
        if (learner == null)
            return;

        var learnerId = learner.Id;

        db.Enrollments.AddRange(
            new EnrollmentEntity
            {
                UserId = learnerId,
                CourseId = 1,
                EnrolledAt = DateTime.UtcNow.AddDays(-14),
                Status = EnrollmentStatus.Active,
                LastVisitedLessonId = 3
            },
            new EnrollmentEntity
            {
                UserId = learnerId,
                CourseId = 2,
                EnrolledAt = DateTime.UtcNow.AddDays(-7),
                Status = EnrollmentStatus.Active,
                LastVisitedLessonId = 4
            },
            new EnrollmentEntity
            {
                UserId = learnerId,
                CourseId = 3,
                EnrolledAt = DateTime.UtcNow.AddDays(-3),
                Status = EnrollmentStatus.Active
            });
        await db.SaveChangesAsync();

        // Course 1: lessons 1 & 2 done, lesson 3 not yet started
        db.Set<LessonProgressEntity>().AddRange(
            new LessonProgressEntity { UserId = learnerId, LessonId = 1, CompletedAt = DateTime.UtcNow.AddDays(-13) },
            new LessonProgressEntity { UserId = learnerId, LessonId = 2, CompletedAt = DateTime.UtcNow.AddDays(-10) });
        await db.SaveChangesAsync();
    }

    private static async Task SeedFeedback(AppDbContext db, UserManager<ForgeUser> userManager)
    {
        if (await db.CourseFeedbacks.AnyAsync())
            return;

        var learner = await userManager.FindByEmailAsync("learner@test.local");
        if (learner == null)
            return;

        var learnerId = learner.Id;

        db.CourseFeedbacks.AddRange(
            new CourseFeedbackEntity
            {
                UserId = learnerId,
                CourseId = 1,
                LessonId = null,
                Rating = 4,
                Comment = "Great intro course! Clear explanations and good pacing. Would love more exercises.",
                SubmittedAt = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new CourseFeedbackEntity
            {
                UserId = learnerId,
                CourseId = 1,
                LessonId = 1,
                Rating = 5,
                Comment = "Perfect introduction. Concise and to the point.",
                SubmittedAt = DateTime.UtcNow.AddDays(-13),
                UpdatedAt = DateTime.UtcNow.AddDays(-13)
            },
            new CourseFeedbackEntity
            {
                UserId = learnerId,
                CourseId = 1,
                LessonId = 2,
                Rating = 4,
                Comment = "Good overview of data types. The table was helpful.",
                SubmittedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-10)
            });
        await db.SaveChangesAsync();
    }

    private static async Task SeedAnnotations(AppDbContext db, UserManager<ForgeUser> userManager)
    {
        if (await db.LessonAnnotations.AnyAsync())
            return;

        var learner = await userManager.FindByEmailAsync("learner@test.local");
        var java = await userManager.FindByEmailAsync("java@test.local");
        if (learner == null || java == null)
            return;

        db.LessonAnnotations.AddRange(
            new LessonAnnotationEntity
            {
                UserId = learner.Id,
                DisplayName = "Test",
                LessonId = 1,
                Content = "Great starting point! I recommend pairing this with a simple Python tutorial to see how the concepts translate across languages.",
                Rating = 5,
                CreatedAt = DateTime.UtcNow.AddDays(-12),
                UpdatedAt = DateTime.UtcNow.AddDays(-12)
            },
            new LessonAnnotationEntity
            {
                UserId = java.Id,
                DisplayName = "Java",
                LessonId = 1,
                Content = "For Java devs: the 'variables' concept maps directly to Java — same primitive types (int, boolean) with slightly different syntax.",
                Rating = 4,
                CreatedAt = DateTime.UtcNow.AddDays(-11),
                UpdatedAt = DateTime.UtcNow.AddDays(-11)
            },
            new LessonAnnotationEntity
            {
                UserId = learner.Id,
                DisplayName = "Test",
                LessonId = 2,
                Content = "Tip: in C# you can also use `var` and let the compiler infer the type. Saves a lot of typing once you're comfortable.",
                Rating = null,
                CreatedAt = DateTime.UtcNow.AddDays(-9),
                UpdatedAt = DateTime.UtcNow.AddDays(-9)
            },
            new LessonAnnotationEntity
            {
                UserId = java.Id,
                DisplayName = "Java",
                LessonId = 4,
                Content = "LINQ is essentially Java Streams. If you know `.stream().filter().map().collect()`, LINQ will feel very familiar.",
                Rating = 5,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            });
        await db.SaveChangesAsync();
    }

    private static string Json(string key, string value)
        => JsonSerializer.Serialize(new Dictionary<string, string>(StringComparer.Ordinal) { [key] = value });

    private static async Task SeedTestUsers(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ForgeUser>>();

        // BackOffice admin - no team claim (manages all)
        if (await userManager.FindByEmailAsync("backoffice@test.local") == null)
        {
            var admin = new ForgeUser
            {
                UserName = "backoffice",
                Email = "backoffice@test.local",
                EmailConfirmed = true,
                FirstName = "BackOffice",
                LastName = "Admin"
            };
            var result = await userManager.CreateAsync(admin, "AdminPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRolesAsync(admin, ["backoffice"]);
            }
        }

        // Local user for Java team only
        if (await userManager.FindByEmailAsync("java@test.local") == null)
        {
            var user = new ForgeUser
            {
                UserName = "java",
                Email = "java@test.local",
                EmailConfirmed = true,
                FirstName = "Java",
                LastName = "Developer"
            };
            var result = await userManager.CreateAsync(user, "UserPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "manager");
                await userManager.AddClaimAsync(user, new Claim("team", "1")); // Java
            }
        }

        // Local user for .NET team only
        if (await userManager.FindByEmailAsync("dotnet@test.local") == null)
        {
            var user = new ForgeUser
            {
                UserName = "dotnet",
                Email = "dotnet@test.local",
                EmailConfirmed = true,
                FirstName = "DotNet",
                LastName = "Developer"
            };
            var result = await userManager.CreateAsync(user, "UserPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "manager");
                await userManager.AddClaimAsync(user, new Claim("team", "2")); // .NET
            }
        }

        // User with access to multiple teams (Java + .NET)
        if (await userManager.FindByEmailAsync("multi@test.local") == null)
        {
            var user = new ForgeUser
            {
                UserName = "multi",
                Email = "multi@test.local",
                EmailConfirmed = true,
                FirstName = "Multi",
                LastName = "Team"
            };
            var result = await userManager.CreateAsync(user, "UserPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "manager");
                await userManager.AddClaimAsync(user, new Claim("team", "1")); // Java
                await userManager.AddClaimAsync(user, new Claim("team", "2")); // .NET
            }
        }

        // Learner user - basic learner role
        if (await userManager.FindByEmailAsync("learner@test.local") == null)
        {
            var user = new ForgeUser
            {
                UserName = "learner",
                Email = "learner@test.local",
                EmailConfirmed = true,
                FirstName = "Test",
                LastName = "Learner"
            };
            var result = await userManager.CreateAsync(user, "UserPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "learner");
            }
        }
    }
}
