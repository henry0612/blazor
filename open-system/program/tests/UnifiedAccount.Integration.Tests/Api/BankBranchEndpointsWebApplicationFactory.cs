using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Infrastructure.Persistence;
using UnifiedAccount.Integration.Tests.Helpers;

namespace UnifiedAccount.Integration.Tests.Api;

public sealed class BankBranchEndpointsWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"BankBranchApiTests_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(IDbContextFactory<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>));
            var sqlServerDescriptors = services
                .Where(descriptor =>
                    descriptor.ServiceType.FullName?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true
                    || descriptor.ImplementationType?.FullName?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
            foreach (var descriptor in sqlServerDescriptors)
                services.Remove(descriptor);

            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
            services.AddScoped<AppDbContext>(sp =>
                sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = TestAuthHandler.SchemeName;
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName,
                _ => { });
        });
    }

    public async Task SeedAsync(params BankBranch[] branches)
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.BankBranches.AddRange(branches);
        await db.SaveChangesAsync();
    }
}
