using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UnifiedAccount.Infrastructure.Persistence;

/// <summary>
/// EF Core Tools 用デザインタイムファクトリ (migrations add 専用)
/// </summary>
public class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=UnifiedAccountDesignTime;Trusted_Connection=True;")
            .Options;
        return new AppDbContext(options);
    }
}

