using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnifiedAccount.Application.Services.Reports;
using UnifiedAccount.Infrastructure.Configuration;
using UnifiedAccount.Infrastructure.Services.Reports;

namespace UnifiedAccount.Application.Tests.Services.Reports;

public class ReportingDependencyInjectionTests : IDisposable
{
    private readonly string _originalCurrentDirectory;
    private readonly string _tempRoot;

    public ReportingDependencyInjectionTests()
    {
        _originalCurrentDirectory = Directory.GetCurrentDirectory();
        _tempRoot = Path.Combine(Path.GetTempPath(), $"reporting_di_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalCurrentDirectory);

        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public void AddReportOutputServices_UsesConfiguredTemplateFolder()
    {
        var configuredTemplateFolder = Path.Combine(_tempRoot, "ConfiguredTemplates");
        Directory.CreateDirectory(configuredTemplateFolder);

        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            [$"{ReportSettings.SectionName}:{nameof(ReportSettings.TemplateFolder)}"] = configuredTemplateFolder
        });

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddReportOutputServices(configuration);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var resolver = scope.ServiceProvider.GetRequiredService<IReportTemplateResolver>();

        var templateBasePath = GetTemplateBasePath(resolver);
        templateBasePath.Should().Be(Path.GetFullPath(configuredTemplateFolder));
    }

    /// <summary>
    /// このテストでは 相対パスを指定したとき、カレントディレクトリではなく実行ディレクトリ基準で解決される。
    /// バッチはジョブスケジューラから任意のカレントディレクトリで起動されるため、解決基準を実行ディレクトリに固定している。
    /// </summary>
    [Fact]
    public void AddReportOutputServices_ResolvesRelativeTemplateFolderAgainstBaseDirectory()
    {
        var relativeFolder = $"relative_templates_{Guid.NewGuid():N}";
        var expected = Path.Combine(AppContext.BaseDirectory, relativeFolder);
        Directory.CreateDirectory(expected);

        try
        {
            // カレントディレクトリを実行ディレクトリと無関係な場所へ移しても解決できること。
            Directory.SetCurrentDirectory(_tempRoot);

            var configuration = BuildConfiguration(new Dictionary<string, string?>
            {
                [$"{ReportSettings.SectionName}:{nameof(ReportSettings.TemplateFolder)}"] = relativeFolder
            });

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddReportOutputServices(configuration);

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var resolver = scope.ServiceProvider.GetRequiredService<IReportTemplateResolver>();

            GetTemplateBasePath(resolver).Should().Be(Path.GetFullPath(expected));
        }
        finally
        {
            Directory.SetCurrentDirectory(_originalCurrentDirectory);
            Directory.Delete(expected, recursive: true);
        }
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public void AddReportOutputServices_ThrowsWhenConfiguredTemplateFolderDoesNotExist()
    {
        var missingTemplateFolder = Path.Combine(_tempRoot, "MissingTemplates");
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            [$"{ReportSettings.SectionName}:{nameof(ReportSettings.TemplateFolder)}"] = missingTemplateFolder
        });

        var services = new ServiceCollection();

        var act = () => services.AddReportOutputServices(configuration);

        act.Should().Throw<DirectoryNotFoundException>()
            .WithMessage($"*{missingTemplateFolder}*");
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public void AddReportOutputServices_UsesDefaultReportsPathWhenTemplateFolderIsNotConfigured()
    {
        Directory.SetCurrentDirectory(_tempRoot);
        Directory.CreateDirectory(Path.Combine(_tempRoot, "open-system", "program", "reports"));
        var configuration = BuildConfiguration();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddReportOutputServices(configuration);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var resolver = scope.ServiceProvider.GetRequiredService<IReportTemplateResolver>();

        var templateBasePath = GetTemplateBasePath(resolver);
        templateBasePath.Should().Be(Path.GetFullPath(Path.Combine(_tempRoot, "open-system", "program", "reports")));
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public void AddReportOutputServices_ThrowsWhenDefaultReportsPathDoesNotExist()
    {
        Directory.SetCurrentDirectory(_tempRoot);
        var configuration = BuildConfiguration();

        var services = new ServiceCollection();
        var act = () => services.AddReportOutputServices(configuration);

        act.Should().Throw<DirectoryNotFoundException>()
            .WithMessage("*./open-system/program/reports*");
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?>? values = null)
    {
        var builder = new ConfigurationBuilder();
        if (values is not null)
            builder.AddInMemoryCollection(values);

        return builder.Build();
    }

    private static string GetTemplateBasePath(IReportTemplateResolver resolver)
    {
        resolver.Should().BeOfType<ReportTemplateResolver>();
        var resolverImpl = (ReportTemplateResolver)resolver;

        var field = typeof(ReportTemplateResolver).GetField("_templateBasePath",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        field.Should().NotBeNull();
        return field!.GetValue(resolverImpl) as string ?? string.Empty;
    }
}









