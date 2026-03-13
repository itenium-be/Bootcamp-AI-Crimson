using System.Net;
using System.Text;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class AuthControllerTests
{
    private IAuthService _authService = null!;
    private IHttpClientFactory _httpClientFactory = null!;
    private AuthController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _authService = Substitute.For<IAuthService>();
        _httpClientFactory = Substitute.For<IHttpClientFactory>();

        _sut = new AuthController(_authService, _httpClientFactory);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost");
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Test]
    public async Task Login_WhenCredentialsInvalid_ReturnsUnauthorized()
    {
        _authService.GetUsernameAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult<string?>(null));

        var result = await _sut.Login(new LoginRequest { Email = "user@test.local", Password = "wrong" });

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
    }

    [Test]
    public async Task Login_WhenCredentialsValid_ReturnsAccessToken()
    {
        _authService.GetUsernameAsync("user@test.local", "ValidPassword123!")
            .Returns(Task.FromResult<string?>("testuser"));

        var tokenJson = @"{""access_token"":""test.jwt.token"",""token_type"":""Bearer"",""expires_in"":3600}";
        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(tokenJson, Encoding.UTF8, "application/json"),
        });
        _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));

        var result = await _sut.Login(new LoginRequest { Email = "user@test.local", Password = "ValidPassword123!" });

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var json = System.Text.Json.JsonSerializer.Serialize(okResult!.Value);
        Assert.That(json, Does.Contain("test.jwt.token"));
    }

    [Test]
    public async Task Login_WhenTokenEndpointFails_ReturnsUnauthorized()
    {
        _authService.GetUsernameAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult<string?>("testuser"));

        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.BadRequest));
        _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));

        var result = await _sut.Login(new LoginRequest { Email = "user@test.local", Password = "ValidPassword123!" });

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
    }
}

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;

    public FakeHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(_response);
}
