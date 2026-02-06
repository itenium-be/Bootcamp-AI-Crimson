namespace Itenium.SkillForge.WebApi.Controllers;

public record CreateCourseRequest(string Name, string? Description, string? Category, string? Level);
