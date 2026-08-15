using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnifiedAccount.Application.Services;
using UnifiedAccount.Application.Services.Reports;
using UnifiedAccount.Domain.Interfaces;
using UnifiedAccount.Infrastructure.Persistence;
using UnifiedAccount.Infrastructure.Persistence.Repositories;
using UnifiedAccount.Infrastructure.Services;
using UnifiedAccount.Infrastructure.Services.Reports;

namespace UnifiedAccount.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // EF Core
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));
        services.AddScoped<AppDbContext>(sp =>
            sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<CompanyRepository>();
        services.AddScoped<ContractRepository>();
        services.AddScoped<ZenginRepository>();

        // Services (COBOL CALL先相当)
        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<ICharacterConverter, CharacterConverter>();
        // コード設定マスタは更新頻度が低いため、キャッシュのみシングルトンで共有する。
        services.AddSingleton<CodeLookupCache>();
        services.AddScoped<ICodeLookupService, CodeLookupService>();
        services.AddScoped<ICodeSettingService, CodeSettingService>();
        services.AddScoped<ICheckDigitValidator, CheckDigitValidator>();
        services.AddScoped<IJobExecutionGuard, JobExecutionGuard>();
        services.AddSingleton<TransferCycleValidator>();
        services.AddSingleton<DateHelper>(sp =>
        {
            // アプリケーション起動時にJapaneseErasテーブルからデータを読み込みキャッシュ
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var eras = db.JapaneseEras.OrderBy(e => e.Code).ToList();
            return new DateHelper(eras);
        });
        // 銀行支店マスター サービス
        services.AddScoped<IBankBranchService, BankBranchService>();

        // 認証サービス (F-INF-001)
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        // 帳票出力設定
        services.Configure<ReportSettings>(opts =>
        {
            var section = configuration.GetSection(ReportSettings.SectionName);
            if (section[nameof(ReportSettings.OutputFolder)] is { } outputFolder)
                opts.OutputFolder = outputFolder;
            if (section[nameof(ReportSettings.TemplateFolder)] is { } templateFolder)
                opts.TemplateFolder = templateFolder;
        });
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IReportFilePathProvider, FileSystemReportFilePathProvider>();

        return services;
    }
}
