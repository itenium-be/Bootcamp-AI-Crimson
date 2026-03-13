namespace Itenium.SkillForge.Services;

public record UserResponse(
    string Id,
    string Name,
    string Email,
    string Role,
    bool IsActive,
    DateTimeOffset? LastActiveAt = null
);
