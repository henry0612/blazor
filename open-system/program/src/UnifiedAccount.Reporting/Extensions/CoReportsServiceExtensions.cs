using Microsoft.Extensions.DependencyInjection;
using UnifiedAccount.Reporting.Services;

namespace UnifiedAccount.Reporting.Extensions;

/// <summary>
/// CoReports プロバイダの DI 登録拡張メソッド。
/// </summary>
public static class CoReportsServiceExtensions
{
    /// <summary>
    /// CoReports エンジンを ITabularReportService / IFormReportService として登録する。
    /// 先に AddReportingCore() で登録された CSV 実装を上書きする。
    /// </summary>
    /// <param name="services">サービス一覧を指定する。</param>
    public static IServiceCollection AddCoReportsProvider(this IServiceCollection services)
    {
        ReportingEncodingInitializer.EnsureRegistered();
        services.AddSingleton<ITabularReportService, CoReportsTabularReportService>();
        services.AddSingleton<IFormReportService, CoReportsFormReportService>();
        return services;
    }

    /// <summary>
    /// AddReportingCore() + AddCoReportsProvider() を一括登録する。
    /// レンダラ実装登録用。業務ジョブの入口契約は IReportOutputService 側で統制する。
    /// </summary>
    /// <param name="services">サービス一覧を指定する。</param>
    public static IServiceCollection AddReportingServices(this IServiceCollection services)
        => services.AddReportingCore().AddCoReportsProvider();
}




