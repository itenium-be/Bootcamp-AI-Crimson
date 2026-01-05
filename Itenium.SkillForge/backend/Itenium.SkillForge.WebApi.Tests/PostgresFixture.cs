using Itenium.SkillForge.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Itenium.SkillForge.WebApi.Tests;

[SetUpFixture]
public class PostgresFixture
{
    private static PostgreSqlContainer _container = null!;
    public static string ConnectionString { get; private set; } = null!;
    private const string MigrationAssembly = "Itenium.SkillForge.Data";

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:17")
            .Build();

        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString, x => x.MigrationsAssembly(MigrationAssembly))
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.MigrateAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _container.DisposeAsync();
    }

    internal static DbContextOptions<AppDbContext> CreateDbContextOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString, x => x.MigrationsAssembly(MigrationAssembly))
            .Options;
    }
}
