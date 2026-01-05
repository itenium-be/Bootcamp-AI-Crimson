using Itenium.Forge.Security;

namespace Itenium.SkillForge.Services;

/// <summary>
/// Provides access to the current user
/// </summary>
public interface ISkillForgeUser : ICurrentUser
{
    /// <summary>
    /// Whether the current user is central management.
    /// </summary>
    bool IsCentral { get; }

    /// <summary>
    /// Ids of the Organizations the user has access to.
    /// </summary>
    IEnumerable<int> Organizations { get; }
}
