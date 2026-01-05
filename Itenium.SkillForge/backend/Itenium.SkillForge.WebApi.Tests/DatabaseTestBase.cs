using Itenium.SkillForge.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Itenium.SkillForge.WebApi.Tests;

public abstract class DatabaseTestBase
{
    protected AppDbContext Db { get; private set; } = null!;
    private IDbContextTransaction _transaction = null!;

    [SetUp]
    public async Task BaseSetUp()
    {
        Db = new AppDbContext(PostgresFixture.CreateDbContextOptions());
        _transaction = await Db.Database.BeginTransactionAsync();
    }

    [TearDown]
    public async Task BaseTearDown()
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        await Db.DisposeAsync();
    }
}
