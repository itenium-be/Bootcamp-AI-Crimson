using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class SsoControllerTests
{
    private SsoController _sut = null!;

    [SetUp]
    public void Setup()
    {
        var config = new ConfigurationBuilder().Build();
        _sut = new SsoController(config);
    }

    [Test]
    public void GetProviders_WhenNoneConfigured_ReturnsEmptyList()
    {
        var result = _sut.GetProviders();

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var providers = ok!.Value as IList<SsoProviderResponse>;
        Assert.That(providers, Is.Empty);
    }

    [Test]
    public void GetProviders_WhenProviderConfigured_ReturnsProvider()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["Sso:Providers:0:Name"] = "Microsoft",
                ["Sso:Providers:0:DisplayName"] = "Sign in with Microsoft",
                ["Sso:Providers:0:AuthorizeUrl"] = "https://login.microsoftonline.com/tenant/oauth2/v2.0/authorize",
            })
            .Build();
        var sut = new SsoController(config);

        var result = sut.GetProviders();

        var ok = result.Result as OkObjectResult;
        var providers = ok!.Value as IList<SsoProviderResponse>;
        Assert.That(providers, Has.Count.EqualTo(1));
        Assert.That(providers![0].Name, Is.EqualTo("Microsoft"));
        Assert.That(providers[0].DisplayName, Is.EqualTo("Sign in with Microsoft"));
        Assert.That(providers[0].AuthorizeUrl, Is.EqualTo("https://login.microsoftonline.com/tenant/oauth2/v2.0/authorize"));
    }
}
