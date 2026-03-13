using Itenium.SkillForge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Itenium.SkillForge.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ISkillForgeUser _currentUser;

    public UsersController(IUserRepository userRepository, ISkillForgeUser currentUser)
    {
        _userRepository = userRepository;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Get all users. BackOffice only.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IList<UserResponse>>> GetUsers()
    {
        if (!_currentUser.IsBackOffice)
        {
            return Forbid();
        }

        var users = await _userRepository.GetAllUsersWithRolesAsync();
        return Ok(users);
    }
}
