namespace Itenium.SkillForge.Services;

public interface IUserRepository
{
    Task<IList<UserResponse>> GetAllUsersWithRolesAsync();
}
