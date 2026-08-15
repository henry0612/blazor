using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Infrastructure.Services.Reports;
using UnifiedAccount.Application.Services.Reports;

namespace UnifiedAccount.Integration.Tests.Reporting;

/// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
/// FileSystemReportOutputDestination テスト（ステップ5: 出力ルーティング）
/// </summary>
public class FileSystemReportOutputDestinationTests : IDisposable
{
    private readonly string _testOutputDir;
    private readonly FileSystemReportOutputDestination _destination;
    private readonly IServiceProvider _serviceProvider;

    public FileSystemReportOutputDestinationTests()
    {
        _testOutputDir = Path.Combine(Path.GetTempPath(), $"report_output_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testOutputDir);

        var services = new ServiceCollection().AddLogging(b => b.AddDebug());
        _serviceProvider = services.BuildServiceProvider();
        var logger = _serviceProvider.GetRequiredService<ILogger<FileSystemReportOutputDestination>>();

        _destination = new FileSystemReportOutputDestination(logger);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testOutputDir))
        {
            Directory.Delete(_testOutputDir, true);
        }
    }

    /// <summary>このテストでは 対象条件を指定したとき、成功する。</summary>
    [Fact]
    public async Task SendAsync_WithValidFiles_ShouldCopySuccessfully()
    {
        // Arrange: テスト用ソースファイルを作成
        var sourceFile = Path.Combine(_testOutputDir, "source.pdf");
        var content = "Test PDF Content"u8.ToArray();
        await File.WriteAllBytesAsync(sourceFile, content);

        var outputFile = Path.Combine(_testOutputDir, "output", "report.pdf");

        // Act
        var result = await _destination.SendAsync(sourceFile, outputFile);

        // Assert
        result.Should().BeTrue();
        File.Exists(outputFile).Should().BeTrue();
        var outputContent = await File.ReadAllBytesAsync(outputFile);
        outputContent.Should().Equal(content);
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public async Task SendAsync_OutputDirectoryNotExist_ShouldCreateDirectory()
    {
        // Arrange
        var sourceFile = Path.Combine(_testOutputDir, "source.pdf");
        await File.WriteAllTextAsync(sourceFile, "Test content");

        var nestedDir = Path.Combine(_testOutputDir, "nested", "deep", "output");
        var outputFile = Path.Combine(nestedDir, "report.pdf");

        // Act
        var result = await _destination.SendAsync(sourceFile, outputFile);

        // Assert
        result.Should().BeTrue();
        Directory.Exists(nestedDir).Should().BeTrue();
        File.Exists(outputFile).Should().BeTrue();
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public async Task SendAsync_ExistingFile_ShouldOverwrite()
    {
        // Arrange
        var sourceFile = Path.Combine(_testOutputDir, "source.pdf");
        await File.WriteAllTextAsync(sourceFile, "New content");

        var outputFile = Path.Combine(_testOutputDir, "report.pdf");
        await File.WriteAllTextAsync(outputFile, "Old content");

        // Act
        var result = await _destination.SendAsync(sourceFile, outputFile);

        // Assert
        result.Should().BeTrue();
        var content = await File.ReadAllTextAsync(outputFile);
        content.Should().Be("New content");
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public async Task SendAsync_SourceFileNotExist_ShouldThrow()
    {
        // Arrange
        var sourceFile = Path.Combine(_testOutputDir, "nonexistent.pdf");
        var outputFile = Path.Combine(_testOutputDir, "output.pdf");

        // Act & Assert
        await FluentActions.Invoking(() =>
            _destination.SendAsync(sourceFile, outputFile))
            .Should()
            .ThrowAsync<FileNotFoundException>();
    }

    /// <summary>このテストでは 出力パスを生成したとき、失敗する。</summary>
    [Fact]
    public async Task SendAsync_EmptySourcePath_ShouldReturnFalse()
    {
        // Arrange
        var outputFile = Path.Combine(_testOutputDir, "output.pdf");

        // Act
        var result = await _destination.SendAsync(string.Empty, outputFile);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>このテストでは 出力先を指定したとき、失敗する。</summary>
    [Fact]
    public async Task SendAsync_EmptyOutputPath_ShouldReturnFalse()
    {
        // Arrange
        var sourceFile = Path.Combine(_testOutputDir, "source.pdf");
        await File.WriteAllTextAsync(sourceFile, "Test");

        // Act
        var result = await _destination.SendAsync(sourceFile, string.Empty);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public async Task ValidateOutputPathAsync_ValidPath_ShouldSucceed()
    {
        // Arrange
        var outputPath = Path.Combine(_testOutputDir, "valid", "report.pdf");

        // Act & Assert
        await FluentActions.Invoking(() =>
            _destination.ValidateOutputPathAsync(outputPath))
            .Should()
            .NotThrowAsync();
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public async Task ValidateOutputPathAsync_InvalidPath_ShouldThrow()
    {
        // Arrange: 無効な文字を含むパス
        var invalidPath = Path.Combine(_testOutputDir, "invalid\0path", "report.pdf");

        // Act & Assert
        await FluentActions.Invoking(() =>
            _destination.ValidateOutputPathAsync(invalidPath))
            .Should()
            .ThrowAsync<ArgumentException>();
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public async Task ValidateOutputPathAsync_EmptyPath_ShouldThrow()
    {
        // Act & Assert
        await FluentActions.Invoking(() =>
            _destination.ValidateOutputPathAsync(string.Empty))
            .Should()
            .ThrowAsync<ArgumentException>();
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public async Task ValidateOutputPathAsync_NoDirectorySpecified_ShouldThrow()
    {
        // Arrange
        var pathWithoutDir = "report.pdf";

        // Act & Assert
        await FluentActions.Invoking(() =>
            _destination.ValidateOutputPathAsync(pathWithoutDir))
            .Should()
            .ThrowAsync<ArgumentException>();
    }
}










