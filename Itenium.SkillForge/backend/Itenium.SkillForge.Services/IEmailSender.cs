namespace Itenium.SkillForge.Services;

/// <summary>
/// Sends transactional emails.
/// </summary>
public interface IEmailSender
{
    Task SendPasswordResetEmailAsync(string toEmail, string token);
}
