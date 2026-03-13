using Itenium.SkillForge.Services;

namespace Itenium.SkillForge.WebApi;

/// <summary>
/// Email sender that logs to the console. Replace with SMTP implementation when email is configured.
/// </summary>
public partial class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string token)
    {
        LogPasswordResetToken(toEmail, token);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Password reset requested for {Email}. Token: {Token}")]
    private partial void LogPasswordResetToken(string email, string token);
}
