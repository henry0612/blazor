using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Integration.Tests.Helpers;

/// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
public static class TestDbContextFactory
{
    public static AppDbContext Create(string? dbName = null)
    {
        dbName ??= $"TestDb_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}








