using Itenium.SkillForge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Itenium.SkillForge.WebApi.Controllers;

public record ChangeRoleRequest(string Role);

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

    /// <summary>Get all users. BackOffice only.</summary>
    [HttpGet]
    public async Task<ActionResult<IList<UserResponse>>> GetUsers()
    {
        if (!_currentUser.IsBackOffice) return Forbid();
        var users = await _userRepository.GetAllUsersWithRolesAsync();
        return Ok(users);
    }

    /// <summary>Get a single user by ID. BackOffice only.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUser(string id)
    {
        if (!_currentUser.IsBackOffice) return Forbid();
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user is null) return NotFound();
        return Ok(user);
    }

    /// <summary>Change a user's role. BackOffice only.</summary>
    [HttpPut("{id}/role")]
    public async Task<IActionResult> ChangeRole(string id, [FromBody] ChangeRoleRequest request)
    {
        if (!_currentUser.IsBackOffice) return Forbid();
        var found = await _userRepository.ChangeRoleAsync(id, request.Role);
        if (!found) return NotFound();
        return NoContent();
    }

    /// <summary>Deactivate a user (lock them out). BackOffice only.</summary>
    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(string id)
    {
        if (!_currentUser.IsBackOffice) return Forbid();
        var found = await _userRepository.DeactivateAsync(id);
        if (!found) return NotFound();
        return NoContent();
    }

    /// <summary>Activate a user (remove lockout). BackOffice only.</summary>
    [HttpPut("{id}/activate")]
    public async Task<IActionResult> ActivateUser(string id)
    {
        if (!_currentUser.IsBackOffice) return Forbid();
        var found = await _userRepository.ActivateAsync(id);
        if (!found) return NotFound();
        return NoContent();
    }

    /// <summary>Get user activity / login history (stub). BackOffice only.</summary>
    [HttpGet("{id}/activity")]
    public ActionResult<IList<object>> GetUserActivity(string id)
    {
        if (!_currentUser.IsBackOffice) return Forbid();
        return Ok(new List<object>());
    }
}
