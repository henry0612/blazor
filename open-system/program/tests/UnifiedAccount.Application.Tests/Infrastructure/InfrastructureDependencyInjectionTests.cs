using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnifiedAccount.Infrastructure.Configuration;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Application.Tests.Infrastructure;

public class InfrastructureDependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_RegistersDbContextFactoryForBlazorPages()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=UnifiedAccountTests;Trusted_Connection=True;TrustServerCertificate=True;"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetService<IDbContextFactory<AppDbContext>>();

        factory.Should().NotBeNull();
        provider.GetService<AppDbContext>().Should().NotBeNull();
    }
}
