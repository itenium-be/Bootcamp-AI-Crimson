using System.Text.Json;
using Itenium.SkillForge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Itenium.SkillForge.WebApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _authService = authService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    /// <summary>
    /// Authenticate with email and password, returns a JWT access token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var username = await _authService.GetUsernameAsync(request.Email, request.Password);
        if (username is null)
            return Unauthorized();

        var client = _httpClientFactory.CreateClient();
        var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["grant_type"] = "password",
            ["username"] = username,
            ["password"] = request.Password,
            ["client_id"] = "skillforge-spa",
            ["scope"] = "openid profile email",
        });

        // When Auth:InternalBaseUrl is set (e.g. in Docker), send the request to the internal port
        // but forward the original Host header so OpenIddict generates the token with the correct
        // external issuer URL (matching what the frontend uses for subsequent API calls).
        var internalBaseUrl = _configuration["Auth:InternalBaseUrl"];
        var tokenUrl = $"{(internalBaseUrl ?? $"{Request.Scheme}://{Request.Host}")}/connect/token";
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenUrl) { Content = tokenContent };
        if (internalBaseUrl != null)
            tokenRequest.Headers.Host = Request.Host.Value;

        var response = await client.SendAsync(tokenRequest);
        if (!response.IsSuccessStatusCode)
            return Unauthorized();

        var tokenData = await response.Content.ReadFromJsonAsync<JsonElement>();
        return Ok(new { access_token = tokenData.GetProperty("access_token").GetString() });
    }
}
