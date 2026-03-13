namespace Itenium.SkillForge.Services;

/// <summary>
/// Validates user credentials for email/password login.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Returns the username if email and password are valid, otherwise null.
    /// </summary>
    Task<string?> GetUsernameAsync(string email, string password);
}
