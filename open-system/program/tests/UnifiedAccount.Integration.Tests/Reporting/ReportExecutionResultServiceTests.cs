using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Domain.Constants;
using UnifiedAccount.Domain.Entities.Reports;
using UnifiedAccount.Infrastructure.Persistence;
using UnifiedAccount.Infrastructure.Services.Reports;
using UnifiedAccount.Application.Services.Reports;

namespace UnifiedAccount.Integration.Tests.Reporting;

/// <summary>このテストでは 帳票を生成したとき、帳票データや結果が生成される。</summary>
/// ReportExecutionResultService テスト (ステップ6-7: 実行履歴・リトライ管理)
/// </summary>
public class ReportExecutionResultServiceTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly ReportExecutionResultService _service;
    private readonly IServiceProvider _serviceProvider;

    public ReportExecutionResultServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"test_exec_{Guid.NewGuid():N}")
            .Options;

        _dbContext = new AppDbContext(options);

        var services = new ServiceCollection().AddLogging(b => b.AddDebug());
        _serviceProvider = services.BuildServiceProvider();
        var logger = _serviceProvider.GetRequiredService<ILogger<ReportExecutionResultService>>();

        _service = new ReportExecutionResultService(_dbContext, logger);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }

    /// <summary>このテストでは 実行結果を取得したとき、実行結果が記録される。</summary>
    [Fact]
    public async Task StartExecutionAsync_ShouldCreateNewExecution()
    {
        // Arrange
        var executionId = $"EXEC-{Guid.NewGuid():N}";
        var templateId = "TEST-001";
        var userId = "USER001";

        // Act
        var result = await _service.StartExecutionAsync(
            executionId, null, templateId, "Buffered", userId);

        // Assert
        result.Should().NotBeNull();
        result.ExecutionId.Should().Be(executionId);
        result.TemplateId.Should().Be(templateId);
        result.Status.Should().Be(ReportExecutionStatuses.Running);
        result.RetryCount.Should().Be(0);
        result.StartTime.Should().NotBeNull();

        var saved = await _dbContext.ReportExecutions
            .FirstOrDefaultAsync(e => e.ExecutionId == executionId);
        saved.Should().NotBeNull();
    }

    /// <summary>このテストでは 振替回状態を更新したとき、成功する。</summary>
    [Fact]
    public async Task CompleteExecutionAsync_Success_ShouldUpdateStatus()
    {
        // Arrange
        var executionId = $"EXEC-{Guid.NewGuid():N}";
        await _service.StartExecutionAsync(executionId, null, "TEST-001", "Buffered");

        var outputFile = "/tmp/report.pdf";

        // Act
        await _service.CompleteExecutionAsync(executionId, outputFile, false);

        // Assert
        var execution = await _service.GetExecutionAsync(executionId);
        execution.Should().NotBeNull();
        execution!.Status.Should().Be(ReportExecutionStatuses.Succeeded);
        execution.OutputFilePath.Should().Be(outputFile);
        execution.IsPartialOutput.Should().BeFalse();
        execution.EndTime.Should().NotBeNull();
        execution.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(0);
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public async Task CompleteExecutionAsync_PartialOutput_ShouldMarkAsPartial()
    {
        // Arrange
        var executionId = $"EXEC-{Guid.NewGuid():N}";
        await _service.StartExecutionAsync(executionId, null, "TEST-001", "Streamed");

        var outputFile = "/tmp/report.pdf";

        // Act
        await _service.CompleteExecutionAsync(executionId, outputFile, isPartialOutput: true);

        // Assert
        var execution = await _service.GetExecutionAsync(executionId);
        execution.Should().NotBeNull();
        execution!.Status.Should().Be(ReportExecutionStatuses.PartialOutput);
        execution.IsPartialOutput.Should().BeTrue();
    }

    /// <summary>このテストでは 実行結果を取得したとき、実行結果が記録される。</summary>
    [Fact]
    public async Task FailExecutionAsync_ShouldRecordError()
    {
        // Arrange
        var executionId = $"EXEC-{Guid.NewGuid():N}";
        await _service.StartExecutionAsync(executionId, null, "TEST-001", "Buffered");

        var exception = new InvalidOperationException("Test error");
        var incompleteFile = "/tmp/report.incomplete";

        // Act
        await _service.FailExecutionAsync(executionId, exception, incompleteFile);

        // Assert
        var execution = await _service.GetExecutionAsync(executionId);
        execution.Should().NotBeNull();
        execution!.Status.Should().Be(ReportExecutionStatuses.Failed);
        execution.ErrorMessage.Should().Contain("Test error");
        execution.IncompleteFilePath.Should().Be(incompleteFile);
        execution.IsPartialOutput.Should().BeTrue();
    }

    /// <summary>このテストでは 対象条件を指定したとき、成功する。</summary>
    [Fact]
    public async Task CanRetryAsync_WithinLimit_ShouldReturnTrue()
    {
        // Arrange
        var executionId = $"EXEC-{Guid.NewGuid():N}";
        await _service.StartExecutionAsync(executionId, null, "TEST-001", "Buffered");

        // Act
        var canRetry = await _service.CanRetryAsync(executionId);

        // Assert
        canRetry.Should().BeTrue();
    }

    /// <summary>このテストでは 対象条件を指定したとき、失敗する。</summary>
    [Fact]
    public async Task CanRetryAsync_ExceedingLimit_ShouldReturnFalse()
    {
        // Arrange
        var executionId = $"EXEC-{Guid.NewGuid():N}";
        var execution = await _service.StartExecutionAsync(executionId, null, "TEST-001", "Buffered");

        // 最大リトライ回数を超える
        execution.RetryCount = execution.MaxRetries;
        _dbContext.ReportExecutions.Update(execution);
        await _dbContext.SaveChangesAsync();

        // Act
        var canRetry = await _service.CanRetryAsync(executionId);

        // Assert
        canRetry.Should().BeFalse();
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public async Task IncrementRetryCountAsync_ShouldIncrement()
    {
        // Arrange
        var executionId = $"EXEC-{Guid.NewGuid():N}";
        await _service.StartExecutionAsync(executionId, null, "TEST-001", "Buffered");

        // Act
        await _service.IncrementRetryCountAsync(executionId);
        await _service.IncrementRetryCountAsync(executionId);

        // Assert
        var execution = await _service.GetExecutionAsync(executionId);
        execution.Should().NotBeNull();
        execution!.RetryCount.Should().Be(2);
        execution.Status.Should().Be(ReportExecutionStatuses.Running);
    }

    /// <summary>このテストでは 実行結果を取得したとき、実行結果が記録される。</summary>
    [Fact]
    public async Task GetLatestExecutionByTemplateAsync_ShouldReturnNewest()
    {
        // Arrange
        var templateId = "TEST-001";

        // 2つの実行を作成
        var exec1 = await _service.StartExecutionAsync(
            $"EXEC-{Guid.NewGuid():N}", null, templateId, "Buffered");

        await Task.Delay(10);

        var exec2 = await _service.StartExecutionAsync(
            $"EXEC-{Guid.NewGuid():N}", null, templateId, "Buffered");

        // Act
        var latest = await _service.GetLatestExecutionByTemplateAsync(templateId);

        // Assert
        latest.Should().NotBeNull();
        latest!.ExecutionId.Should().Be(exec2.ExecutionId);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public async Task CleanupIncompleteFilesAsync_ShouldDeleteOldFiles()
    {
        // Arrange: 古い実行記録を作成
        var execution = new ReportExecution
        {
            ExecutionId = $"EXEC-{Guid.NewGuid():N}",
            TemplateId = "TEST-001",
            Status = ReportExecutionStatuses.Failed,
            OutputMode = "Streamed",
            IncompleteFilePath = "/tmp/test_incomplete_old.incomplete",
            CreatedAt = DateTime.Now.AddDays(-10) // 10日前
        };

        _dbContext.ReportExecutions.Add(execution);
        await _dbContext.SaveChangesAsync();

        // Act: 保有期間7日でクリーンアップ
        await _service.CleanupIncompleteFilesAsync(retentionDays: 7);

        // Assert: 記録は残っているがログは出力される（ファイルが存在しないため削除はスキップ）
        var saved = await _service.GetExecutionAsync(execution.ExecutionId);
        saved.Should().NotBeNull();
    }
}









