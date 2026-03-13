namespace Itenium.SkillForge.Services;

public interface IUserRepository
{
    Task<IList<UserResponse>> GetAllUsersWithRolesAsync();
    Task<UserResponse?> GetUserByIdAsync(string id);
    Task<bool> ChangeRoleAsync(string id, string role);
    Task<bool> DeactivateAsync(string id);
    Task<bool> ActivateAsync(string id);

    Task<IList<UserResponse>> GetTeamMembersAsync(int teamId);
    Task<IList<UserResponse>> GetActiveLearnersAsync();
    Task<bool> AddTeamMemberAsync(int teamId, string userId);
    Task<bool> RemoveTeamMemberAsync(int teamId, string userId);
}
