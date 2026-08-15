using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnifiedAccount.Application.Services;
using UnifiedAccount.Application.Services.Reports;
using UnifiedAccount.Infrastructure.Services.Reports;

namespace UnifiedAccount.Infrastructure.Configuration;

/// <summary>
/// 帳票基盤 (F-INF-004) DI 登録
/// ステップ2～7: テンプレート管理 → 実行履歴・リトライ → 出力ルーティング
/// </summary>
public static class ReportingDependencyInjection
{
    public static IServiceCollection AddReportOutputServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(ReportSettings.SectionName);
        var configuredTemplateFolder = section[nameof(ReportSettings.TemplateFolder)];
        var templateBasePath = ResolveTemplateBasePath(configuredTemplateFolder);

        // テンプレート管理（ステップ2）
        services.AddScoped<IReportTemplateResolver>(sp =>
        {
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ReportTemplateResolver>>();
            return new ReportTemplateResolver(logger, templateBasePath);
        });

        // データ組立（ステップ3）- Buffered/Streamed デュアルモード対応
        services.AddScoped<IReportDataBuilder, ReportDataBuilder>();

        // 帳票生成（ステップ4）- CoReports 呼び出し
        services.AddScoped<IReportGenerator, ReportGenerator>();

        // 出力ルーティング（ステップ5）- ファイルシステム出力
        services.AddScoped<IReportOutputDestination, FileSystemReportOutputDestination>();

        // 実行履歴管理（ステップ6-7）- リトライ管理・監査ログ
        services.AddScoped<IReportExecutionResultService, ReportExecutionResultService>();

        // ジョブ/帳票識別子採番
        services.AddSingleton<IExecutionNumberService, ExecutionNumberService>();

        // メイン出力サービス（ステップ1～7 オーケストレータ）
        services.AddScoped<IReportOutputService, ReportOutputService>();

        return services;
    }

    /// <summary>
    /// 既定のテンプレートフォルダ探索順。実行ディレクトリ基準で先頭から評価する。
    /// </summary>
    private static readonly string[] DefaultTemplateFolderCandidates =
    [
        "reports",                          // テンプレートを成果物へ複製した場合（実行ディレクトリ基準）
        "./open-system/program/reports",    // リポジトリルートから実行した場合（カレントディレクトリ基準）
    ];

    internal static string ResolveTemplateBasePath(string? configuredTemplateFolder)
    {
        if (!string.IsNullOrWhiteSpace(configuredTemplateFolder))
        {
            var resolved = ResolveExistingDirectory(configuredTemplateFolder);
            if (resolved is not null)
                return resolved;

            throw new DirectoryNotFoundException(
                $"{ReportSettings.SectionName}:{nameof(ReportSettings.TemplateFolder)} で指定されたディレクトリが存在しません: "
                + $"{configuredTemplateFolder}（実行ディレクトリ: {AppContext.BaseDirectory}）");
        }

        foreach (var candidate in DefaultTemplateFolderCandidates)
        {
            var resolved = ResolveExistingDirectory(candidate);
            if (resolved is not null)
                return resolved;
        }

        throw new DirectoryNotFoundException(
            "既定のテンプレートディレクトリが存在しません。探索したパス: "
            + string.Join(" / ", DefaultTemplateFolderCandidates)
            + $"（実行ディレクトリ: {AppContext.BaseDirectory}）。"
            + $"appsettings.json の {ReportSettings.SectionName}:{nameof(ReportSettings.TemplateFolder)} で明示指定すること。");
    }

    /// <summary>
    /// 相対パスは実行ディレクトリ（<see cref="AppContext.BaseDirectory"/>）基準で解決する。
    /// <c>dotnet run</c> と成果物の直接実行ではカレントディレクトリが異なるため、
    /// カレントディレクトリ基準では解決先が実行方法によって変わってしまう。
    /// 互換のためカレントディレクトリ基準も後段で評価する。
    /// </summary>
    private static string? ResolveExistingDirectory(string path)
    {
        if (Path.IsPathRooted(path))
            return Directory.Exists(path) ? Path.GetFullPath(path) : null;

        var fromBaseDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path));
        if (Directory.Exists(fromBaseDirectory))
            return fromBaseDirectory;

        var fromCurrentDirectory = Path.GetFullPath(path);
        return Directory.Exists(fromCurrentDirectory) ? fromCurrentDirectory : null;
    }
}
