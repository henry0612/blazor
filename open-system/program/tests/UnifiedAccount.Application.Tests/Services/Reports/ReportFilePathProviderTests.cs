using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UnifiedAccount.Infrastructure.Configuration;
using UnifiedAccount.Infrastructure.Services.Reports;

namespace UnifiedAccount.Application.Tests.Services.Reports;

/// <summary>このテストでは 出力パスを生成したとき、出力先やパスが生成される。</summary>
/// FileSystemReportFilePathProvider のユニットテスト。
/// ディレクトリ作成の副作用は OS のテンポラリフォルダで検証する。
/// </summary>
public class FileSystemReportFilePathProviderTests : IDisposable
{
    private readonly string _tempOutputFolder;
    private readonly Mock<ILogger<FileSystemReportFilePathProvider>> _mockLogger;

    public FileSystemReportFilePathProviderTests()
    {
        _tempOutputFolder = Path.Combine(Path.GetTempPath(), $"report_test_{Guid.NewGuid():N}");
        _mockLogger = new Mock<ILogger<FileSystemReportFilePathProvider>>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempOutputFolder))
            Directory.Delete(_tempOutputFolder, recursive: true);
    }

    private FileSystemReportFilePathProvider CreateProvider(DateTimeOffset fixedTime)
    {
        var fakeTimeProvider = new FakeTimeProvider(fixedTime);
        var settings = Options.Create(new ReportSettings { OutputFolder = _tempOutputFolder });
        return new FileSystemReportFilePathProvider(settings, fakeTimeProvider, _mockLogger.Object);
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public void GetOutputFilePath_ReturnsExpectedPath()
    {
        // Arrange
        var fixedTime = new DateTimeOffset(2026, 6, 30, 18, 45, 00, TimeSpan.Zero);
        var provider = CreateProvider(fixedTime);
        var jobExecutionId = "JOB-KOZ290-20260630-000001";
        var jobId = "KOZ290";

        // Act
        var result = provider.GetOutputFilePath(jobExecutionId, jobId);

        // Assert
        var expected = Path.Combine(
            _tempOutputFolder,
            jobExecutionId,
            $"{jobId}_20260630184500",
            $"{jobExecutionId}_{jobId}_20260630184500.PDF");

        result.Should().Be(expected);
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public void GetOutputFilePath_CreatesOutputDirectory()
    {
        // Arrange
        var fixedTime = new DateTimeOffset(2026, 6, 30, 18, 45, 00, TimeSpan.Zero);
        var provider = CreateProvider(fixedTime);

        // Act
        var filePath = provider.GetOutputFilePath("JOB-KOZ290-20260630-000001", "KOZ290");

        // Assert
        var dir = Path.GetDirectoryName(filePath)!;
        Directory.Exists(dir).Should().BeTrue("呼び出し時にディレクトリが作成される");
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public void GetOutputFilePath_FileExtensionIsPdf()
    {
        // Arrange
        var provider = CreateProvider(DateTimeOffset.UtcNow);

        // Act
        var result = provider.GetOutputFilePath("JOB-KOZ290-20260630-000001", "KOZ290");

        // Assert
        Path.GetExtension(result).Should().Be(".PDF");
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Theory]
    [InlineData("", "KOZ290")]
    [InlineData("  ", "KOZ290")]
    [InlineData("JOB-KOZ290-20260630-000001", "")]
    [InlineData("JOB-KOZ290-20260630-000001", "  ")]
    public void GetOutputFilePath_ThrowsOnBlankArgument(string jobExecutionId, string jobId)
    {
        // Arrange
        var provider = CreateProvider(DateTimeOffset.UtcNow);

        // Act
        var act = () => provider.GetOutputFilePath(jobExecutionId, jobId);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    /// テスト用の固定時刻を返す TimeProvider 実装。
    /// </summary>
    private sealed class FakeTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _fixedUtc;

        public FakeTimeProvider(DateTimeOffset fixedUtc)
        {
            _fixedUtc = fixedUtc;
        }

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
        public override DateTimeOffset GetUtcNow() => _fixedUtc;
    }
}










