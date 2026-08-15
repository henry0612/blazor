using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using UnifiedAccount.Batch.Framework;
using UnifiedAccount.Batch.Framework.Resilience;
using UnifiedAccount.Infrastructure.Configuration;

// Serilog bootstrap
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/batch-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("統一口座バッチ起動");

    // コンテンツルートを実行ディレクトリに固定する。
    // 既定はカレントディレクトリのため、別ディレクトリから `dotnet run --project ...` を実行すると
    // appsettings.json が読み込まれず、接続文字列も帳票テンプレートパスも未設定のまま起動してしまう。
    // バッチはジョブスケジューラから任意のカレントディレクトリで起動されるため、実行ディレクトリ基準に統一する。
    var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
    {
        Args = args,
        ContentRootPath = AppContext.BaseDirectory,
    });

    // Serilog
    builder.Services.AddSerilog(config => config
        .ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/batch-.txt", rollingInterval: RollingInterval.Day));

    // Infrastructure (EF Core, Repositories, Services)
    builder.Services.AddInfrastructure(builder.Configuration);

    // Reporting output foundation services (F-INF-004 帳票出力基盤)
    builder.Services.AddReportOutputServices(builder.Configuration);

    // Polly リジリエンス対応 HTTP クライアント (FTP/Web通信ジョブ用)
    builder.Services.AddResilientHttpClients(builder.Configuration);

    // Batch Framework
    builder.Services.AddSingleton<JobRunner>();
    builder.Services.AddScoped<IJobRunner, JobRunnerService>();
    builder.Services.AddScoped<ITransactionCoordinator, TransactionCoordinator>();

    // Auto-register all IJob implementations
    builder.Services.Scan(scan => scan
        .FromAssemblyOf<JobRunner>()
        .AddClasses(c => c.AssignableTo<IJob>())
        .AsImplementedInterfaces()
        .WithTransientLifetime());

    var host = builder.Build();

    // Run job from command line args
    var runner = host.Services.GetRequiredService<JobRunner>();
    var exitCode = await runner.RunAsync(args);

    return exitCode;
}
catch (Exception ex)
{
    Log.Fatal(ex, "バッチ異常終了");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
