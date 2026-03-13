namespace Itenium.SkillForge.Services;

public interface IUserRepository
{
    Task<IList<UserResponse>> GetAllUsersWithRolesAsync();
    Task<UserResponse?> GetUserByIdAsync(string id);
    Task<bool> ChangeRoleAsync(string id, string role);
    Task<bool> DeactivateAsync(string id);
    Task<bool> ActivateAsync(string id);
}
