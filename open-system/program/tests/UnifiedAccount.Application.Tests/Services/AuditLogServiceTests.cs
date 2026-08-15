using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using UnifiedAccount.Application.DTOs;
using UnifiedAccount.Infrastructure.Persistence;
using UnifiedAccount.Infrastructure.Services;

namespace UnifiedAccount.Application.Tests.Services;

public class AuditLogServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly AuditLogService _sut;

    public AuditLogServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _sut = new AuditLogService(_db, NullLogger<AuditLogService>.Instance);
    }

    [Fact]
    public async Task WriteAsync_WithValidEntry_PersistsAuditRecord()
    {
        await _sut.WriteAsync(CreateEntry());

        var record = await _db.AuditRecords.SingleAsync();
        record.ActorId.Should().Be("user01");
        record.FeatureId.Should().Be("F-ONL-006");
        record.BeforeValuesJson.Should().Be("{}");
        record.AfterValuesJson.Should().Contain("\"Amount1\"");
        record.UpdatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task WriteAsync_WithMissingRequiredField_ThrowsArgumentException()
    {
        var entry = CreateEntry() with { ActorId = string.Empty };

        var act = () => _sut.WriteAsync(entry);

        await act.Should().ThrowAsync<ArgumentException>();
        (await _db.AuditRecords.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task WriteAsync_WithInvalidJson_ThrowsArgumentException()
    {
        var entry = CreateEntry() with { BeforeValuesJson = "{invalid" };

        var act = () => _sut.WriteAsync(entry);

        await act.Should().ThrowAsync<ArgumentException>();
        (await _db.AuditRecords.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task WriteAsync_WithUnapprovedAuditField_ThrowsArgumentException()
    {
        var entry = CreateEntry() with
        {
            AfterValuesJson = "{\"Password\":{\"Before\":\"x\",\"After\":\"y\"}}"
        };

        var act = () => _sut.WriteAsync(entry);

        await act.Should().ThrowAsync<ArgumentException>();
        (await _db.AuditRecords.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task WriteAsync_WithValueOverMaximumLength_ThrowsArgumentException()
    {
        var entry = CreateEntry() with { ActorId = new string('a', 101) };

        var act = () => _sut.WriteAsync(entry);

        await act.Should().ThrowAsync<ArgumentException>();
        (await _db.AuditRecords.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task WriteAsync_WithInvalidTargetKeyFormat_ThrowsArgumentException()
    {
        var entry = CreateEntry() with { TargetKey = "CompanyCode=000001;BatchNo=001;SequenceNo=1" };

        var act = () => _sut.WriteAsync(entry);

        await act.Should().ThrowAsync<ArgumentException>();
        (await _db.AuditRecords.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task WriteAsync_WithCodeSettingTargetKey_PersistsAuditRecord()
    {
        var entry = CreateEntry() with
        {
            FeatureId = "F-ONL-100",
            TargetType = "CodeSetting",
            TargetKey = "{\"CodeCategory\":\"BANK\",\"CodeValue\":\"0001\"}",
            BeforeValuesJson = "{}",
            AfterValuesJson = "{}"
        };

        await _sut.WriteAsync(entry);

        var record = await _db.AuditRecords.SingleAsync();
        record.FeatureId.Should().Be("F-ONL-100");
        record.TargetType.Should().Be("CodeSetting");
        record.TargetKey.Should().Be("{\"CodeCategory\":\"BANK\",\"CodeValue\":\"0001\"}");
    }

    [Fact]
    public async Task WriteAsync_WithBankBranchTargetKey_PersistsAuditRecord()
    {
        var entry = CreateEntry() with
        {
            FeatureId = "F-ONL-007",
            TargetType = "BankBranch",
            TargetKey = "{\"BankCode\":\"0001\",\"BranchCode\":\"001\"}",
            BeforeValuesJson = "{}",
            AfterValuesJson = "{\"BankNameKana\":\"ミズホ\",\"BranchNameKana\":\"ホンテン\"}"
        };

        await _sut.WriteAsync(entry);

        var record = await _db.AuditRecords.SingleAsync();
        record.FeatureId.Should().Be("F-ONL-007");
        record.TargetType.Should().Be("BankBranch");
        record.TargetKey.Should().Be("{\"BankCode\":\"0001\",\"BranchCode\":\"001\"}");
        record.AfterValuesJson.Should().Contain("\"BankNameKana\"");
    }

    /// <summary>
    /// 222 F-INF-003 §3.2 は `OccurredAt` に `DateTimeOffset.Now`（JST）を設定すると定めている。
    /// 格納先は DATETIMEOFFSET(3) でオフセットごと保持するため、JST を受け付けてそのまま記録すること。
    /// </summary>
    [Fact]
    public async Task WriteAsync_WithJstOccurredAt_RecordsOffsetAsIs()
    {
        var jst = new DateTimeOffset(2026, 7, 27, 12, 0, 0, TimeSpan.FromHours(9));
        var entry = CreateEntry() with { OccurredAt = jst };

        await _sut.WriteAsync(entry);

        var record = await _db.AuditRecords.SingleAsync();
        record.OccurredAt.Should().Be(jst);
        record.OccurredAt.Offset.Should().Be(TimeSpan.FromHours(9));
    }

    [Fact]
    public async Task WriteAsync_WithUnsetOccurredAt_ThrowsArgumentException()
    {
        var entry = CreateEntry() with { OccurredAt = default };

        var act = () => _sut.WriteAsync(entry);

        await act.Should().ThrowAsync<ArgumentException>();
        (await _db.AuditRecords.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task WriteAsync_WithCanceledToken_PropagatesCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => _sut.WriteAsync(CreateEntry(), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    /// <summary>
    /// 222 F-INF-003 §9 AUD-UT-004 に対応する。`Result` は §4.1 の固定値以外を許可しないため、
    /// 未定義の結果コードを指定すると `Validate` が検証例外を送出し、DB へ追加しないことを確認する。
    /// </summary>
    [Fact]
    public async Task WriteAsync_WithUndefinedResult_ThrowsArgumentException()
    {
        var entry = CreateEntry() with { Result = "Undefined" };

        var act = () => _sut.WriteAsync(entry);

        await act.Should().ThrowAsync<ArgumentException>();
        (await _db.AuditRecords.CountAsync()).Should().Be(0);
    }

    /// <summary>
    /// 222 F-INF-003 §9 AUD-UT-010 に対応する。`CorrelationId` は §3.2 で必須かつ空文字を許可しないため、
    /// CorrelationContext 未設定相当の空値を指定すると検証例外を送出し、DB へ追加しないことを確認する。
    /// </summary>
    [Fact]
    public async Task WriteAsync_WithEmptyCorrelationId_ThrowsArgumentException()
    {
        var entry = CreateEntry() with { CorrelationId = string.Empty };

        var act = () => _sut.WriteAsync(entry);

        await act.Should().ThrowAsync<ArgumentException>();
        (await _db.AuditRecords.CountAsync()).Should().Be(0);
    }

    /// <summary>
    /// 222 F-INF-003 §9 AUD-UT-011 に対応する。§8.1 が定める「検索0件」「入力または業務検証エラー」
    /// 「楽観的排他競合」「DB等の依存先障害」「予期しない例外」の `Result`／`ErrorCode` の組を指定すると、
    /// `WriteAsync` が例外を送出せず、指定どおりの値で `TD_AuditRecords` へ保存されることを確認する。
    /// `AuditLogService.Validate` は `Result` を §4.1 の固定値集合に含まれるかのみ検証し、`Result` と
    /// `ErrorCode` の対応関係そのものは検証しない（呼出し元が §8.1 に従って設定する）。
    /// </summary>
    [Theory]
    [InlineData("Success", "NO_DATA")]
    [InlineData("ValidationError", "VALIDATION_ERROR")]
    [InlineData("ConcurrencyConflict", "CONCURRENCY_CONFLICT")]
    [InlineData("DependencyError", "DB_ERROR")]
    [InlineData("Failure", "UNEXPECTED_ERROR")]
    public async Task WriteAsync_WithResultAndErrorCodeFromResultCodeTable_PersistsBothAsSpecified(
        string result,
        string errorCode)
    {
        var entry = CreateEntry() with { Result = result, ErrorCode = errorCode };

        await _sut.WriteAsync(entry);

        var record = await _db.AuditRecords.SingleAsync();
        record.Result.Should().Be(result);
        record.ErrorCode.Should().Be(errorCode);
    }

    private static AuditLogEntry CreateEntry()
    {
        return new AuditLogEntry
        {
            OccurredAt = DateTimeOffset.UtcNow,
            ActorId = "user01",
            FeatureId = "F-ONL-006",
            Action = "Save",
            TargetType = "TransferAmount",
            TargetKey = "{\"CompanyCode\":\"000001\",\"BatchNo\":\"001\",\"SequenceNo\":\"1\"}",
            Result = "Success",
            CorrelationId = "correlation-01",
            BeforeValuesJson = null,
            AfterValuesJson = "{\"Amount1\":{\"Before\":1000,\"After\":2000}}"
        };
    }

    public void Dispose() => _db.Dispose();
}
