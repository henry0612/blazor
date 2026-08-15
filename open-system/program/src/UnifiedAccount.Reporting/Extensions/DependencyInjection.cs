using Microsoft.Extensions.DependencyInjection;
using UnifiedAccount.Reporting.Services;

namespace UnifiedAccount.Reporting.Extensions;

/// <summary>
/// DependencyInjection を表すクラス。
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// 抽象層のみ登録 (CSV フォールバック)。
    /// PDF が必要な環境では対応する帳票プロバイダを追加すること。
    /// </summary>
    /// <param name="services">サービス一覧を指定する。</param>
    public static IServiceCollection AddReportingCore(this IServiceCollection services)
    {
        ReportingEncodingInitializer.EnsureRegistered();
        services.AddSingleton<ITabularReportService, CsvTabularReportService>();
        services.AddSingleton<IFormReportService, CsvFormReportService>();
        return services;
    }
}





