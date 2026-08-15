using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using UnifiedAccount.Infrastructure.Services.Reports;

namespace UnifiedAccount.Integration.Tests.Reporting;

public class ReportTemplateResolverTests : IDisposable
{
    private readonly string _tempDir;

    public ReportTemplateResolverTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"template_resolver_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    /// <summary>このテストでは .cfx/.dcx が揃っているとき解決できる。</summary>
    [Fact]
    public async Task ResolveAsync_WithDcxAndCfx_ShouldResolveTemplate()
    {
        var dcxPath = Path.Combine(_tempDir, "SAMPLE.dcx");
        var cfxPath = Path.Combine(_tempDir, "SAMPLE.cfx");
        await File.WriteAllTextAsync(dcxPath, "<root />");
        await File.WriteAllTextAsync(cfxPath, "dummy-form");

        var resolver = new ReportTemplateResolver(NullLogger<ReportTemplateResolver>.Instance, _tempDir);

        var template = await resolver.ResolveAsync("SAMPLE");

        template.Should().NotBeNull();
        template!.TemplateId.Should().Be("SAMPLE");
        template.TemplateFilePath.Should().Be(dcxPath);
    }

    /// <summary>このテストでは .cfx が欠けているとき例外になる。</summary>
    [Fact]
    public async Task ResolveAsync_WithoutCfx_ShouldThrowFileNotFoundException()
    {
        var dcxPath = Path.Combine(_tempDir, "SAMPLE.dcx");
        await File.WriteAllTextAsync(dcxPath, "<root />");

        var resolver = new ReportTemplateResolver(NullLogger<ReportTemplateResolver>.Instance, _tempDir);
        var act = () => resolver.ResolveAsync("SAMPLE");

        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("*.cfx*");
    }
}

