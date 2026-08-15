using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UnifiedAccount.Batch.Framework;
using UnifiedAccount.Infrastructure.Persistence;
using UnifiedAccount.Reporting.Extensions;

namespace UnifiedAccount.Integration.Tests.Helpers;

/// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
public static class JobTestHelper
{
    public static JobContext CreateContext(AppDbContext db, DateOnly? processDate = null, IDictionary<string, string>? parameters = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(db);
        services.AddLogging(builder => builder.AddDebug());
        services.AddReportingServices();

        // Add specific loggers for all job types
        var serviceProvider = services.BuildServiceProvider();

        return new JobContext(
            processDate ?? DateOnly.FromDateTime(DateTime.Today),
            serviceProvider,
            parameters as Dictionary<string, string>);
    }
}








