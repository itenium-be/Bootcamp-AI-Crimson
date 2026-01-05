using Itenium.Forge.Security;

namespace Itenium.SkillForge.Services;

/// <summary>
/// Provides access to the current user
/// </summary>
public interface ISkillForgeUser : ICurrentUser
{
    /// <summary>
    /// Whether the current user is BackOffice management.
    /// </summary>
    bool IsBackOffice { get; }

    /// <summary>
    /// Ids of the Teams the user has access to.
    /// </summary>
    IEnumerable<int> Teams { get; }
}
