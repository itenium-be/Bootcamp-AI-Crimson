using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Itenium.SkillForge.WebApi.Controllers;

public record SsoProviderResponse(string Name, string DisplayName, string AuthorizeUrl);

[ApiController]
[Route("api/auth/sso")]
[AllowAnonymous]
public class SsoController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public SsoController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Returns configured SSO providers. Add providers under Sso:Providers in appsettings.
    /// </summary>
    [HttpGet("providers")]
    public ActionResult<IList<SsoProviderResponse>> GetProviders()
    {
        var providers = _configuration
            .GetSection("Sso:Providers")
            .GetChildren()
            .Select(p => new SsoProviderResponse(
                p["Name"] ?? string.Empty,
                p["DisplayName"] ?? string.Empty,
                p["AuthorizeUrl"] ?? string.Empty))
            .Where(p => !string.IsNullOrEmpty(p.Name))
            .ToList();

        return Ok(providers);
    }
}
