namespace Itenium.SkillForge.WebApi.Controllers;

public record PasswordResetConfirmRequest(string Email, string Token, string NewPassword);
