using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using UnifiedAccount.Application.DTOs;
using UnifiedAccount.Application.Services;
using UnifiedAccount.Infrastructure.Persistence;
using UnifiedAccount.Infrastructure.Services;

namespace UnifiedAccount.Integration.Tests.Services;

public class BankBranchServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldWriteAuditEntryWithBankBranchTargetKey()
    {
        await using var context = CreateContext();
        var auditLogService = new RecordingAuditLogService();
        var service = new BankBranchService(context, CreateFactory(context), auditLogService);

        var result = await service.CreateAsync(new CreateBankBranchRequest(
            "0001",
            "001",
            "ミズホ",
            "ホンテン",
            "みずほ銀行",
            "本店",
            "user01",
            "correlation-01"));

        result.Should().BeOfType<CreateResult.Success>();
        var entry = auditLogService.Entries.Should().ContainSingle().Which;
        entry.FeatureId.Should().Be("F-ONL-007");
        entry.Action.Should().Be("Save");
        entry.TargetType.Should().Be("BankBranch");
        entry.TargetKey.Should().Be("{\"BankCode\":\"0001\",\"BranchCode\":\"001\"}");
        entry.ActorId.Should().Be("user01");
        entry.CorrelationId.Should().Be("correlation-01");
        entry.BeforeValuesJson.Should().Be("{}");
        entry.AfterValuesJson.Should().Be("{\"BankNameKana\":\"ミズホ\",\"BranchNameKana\":\"ホンテン\"}");
    }

    [Fact]
    public async Task UpdateAsync_ShouldWriteAuditEntryWithBeforeAndAfterKana()
    {
        await using var context = CreateContext();
        var existing = new Domain.Entities.Core.BankBranch
        {
            BankCode = "0001",
            BranchCode = "001",
            BankNameKana = "ミズホ",
            BranchNameKana = "ホンテン",
            KanjiSetFlag = "0"
        };
        context.BankBranches.Add(existing);
        await context.SaveChangesAsync();

        var auditLogService = new RecordingAuditLogService();
        var service = new BankBranchService(context, CreateFactory(context), auditLogService);

        var result = await service.UpdateAsync(
            "0001",
            "001",
            new UpdateBankBranchRequest(
                "ミズホ",
                "シンジュクテン",
                null,
                null,
                existing.UpdatedAt,
                "user01",
                "correlation-01"));

        result.Should().BeOfType<UpdateResult.Success>();
        var entry = auditLogService.Entries.Should().ContainSingle().Which;
        entry.TargetKey.Should().Be("{\"BankCode\":\"0001\",\"BranchCode\":\"001\"}");
        entry.BeforeValuesJson.Should().Be("{\"BankNameKana\":\"ミズホ\",\"BranchNameKana\":\"ホンテン\"}");
        entry.AfterValuesJson.Should().Be("{\"BankNameKana\":\"ミズホ\",\"BranchNameKana\":\"シンジュクテン\"}");
    }

    [Fact]
    public async Task DeleteAsync_ShouldWriteAuditEntryWithBeforeValuesOnly()
    {
        await using var context = CreateContext();
        var existing = new Domain.Entities.Core.BankBranch
        {
            BankCode = "0001",
            BranchCode = "001",
            BankNameKana = "ミズホ",
            BranchNameKana = "ホンテン",
            KanjiSetFlag = "0"
        };
        context.BankBranches.Add(existing);
        await context.SaveChangesAsync();

        var auditLogService = new RecordingAuditLogService();
        var service = new BankBranchService(context, CreateFactory(context), auditLogService);

        var result = await service.DeleteAsync(
            "0001",
            "001",
            new DeleteBankBranchRequest(existing.UpdatedAt, "user01", "correlation-01"));

        result.Should().BeOfType<DeleteResult.Success>();
        var entry = auditLogService.Entries.Should().ContainSingle().Which;
        entry.TargetKey.Should().Be("{\"BankCode\":\"0001\",\"BranchCode\":\"001\"}");
        entry.BeforeValuesJson.Should().Be("{\"BankNameKana\":\"ミズホ\",\"BranchNameKana\":\"ホンテン\"}");
        entry.AfterValuesJson.Should().Be("{}");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnDbErrorWhenAuditWriteFails()
    {
        await using var context = CreateContext();
        var auditLogService = new ThrowingAuditLogService();
        var service = new BankBranchService(context, CreateFactory(context), auditLogService);

        var result = await service.CreateAsync(new CreateBankBranchRequest(
            "0001",
            "001",
            "ミズホ",
            "ホンテン",
            null,
            null,
            "user01",
            "correlation-01"));

        result.Should().BeOfType<CreateResult.DbError>();
    }

    /// <summary>
    /// RecordingAuditLogServiceではなく実装のAuditLogServiceを使い、Task 1で拡張した
    /// TargetKey許可リストが実際に機能して TD_AuditRecords へ到達することを確認する。
    /// </summary>
    [Fact]
    public async Task CreateAsync_WithRealAuditLogService_PersistsAuditRecord()
    {
        await using var context = CreateContext();
        var auditLogService = new AuditLogService(context, NullLogger<AuditLogService>.Instance);
        var service = new BankBranchService(context, CreateFactory(context), auditLogService);

        var result = await service.CreateAsync(new CreateBankBranchRequest(
            "0001",
            "001",
            "ミズホ",
            "ホンテン",
            null,
            null,
            "user01",
            "correlation-01"));

        result.Should().BeOfType<CreateResult.Success>();
        var record = await context.AuditRecords.SingleAsync();
        record.FeatureId.Should().Be("F-ONL-007");
        record.TargetType.Should().Be("BankBranch");
        record.TargetKey.Should().Be("{\"BankCode\":\"0001\",\"BranchCode\":\"001\"}");
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingRows()
    {
        await using var context = CreateContext();
        context.BankBranches.AddRange(
            new Domain.Entities.Core.BankBranch { BankCode = "0001", BranchCode = "001", BankNameKana = "ミズホ", BranchNameKana = "ホンテン", KanjiSetFlag = "0" },
            new Domain.Entities.Core.BankBranch { BankCode = "0002", BranchCode = "001", BankNameKana = "ミツイ", BranchNameKana = "ホンテン", KanjiSetFlag = "0" });
        await context.SaveChangesAsync();

        var service = new BankBranchService(context, CreateFactory(context), new RecordingAuditLogService());

        var result = await service.SearchAsync("0001", null, 1, 50);

        result.Items.Should().ContainSingle(item => item.BankCode == "0001");
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"BankBranchServiceTests_{Guid.NewGuid():N}")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }

    private static IDbContextFactory<AppDbContext> CreateFactory(AppDbContext context)
    {
        return new FixedDbContextFactory(context);
    }

    private sealed class RecordingAuditLogService : IAuditLogService
    {
        public List<AuditLogEntry> Entries { get; } = [];

        public Task WriteAsync(AuditLogEntry entry, CancellationToken ct = default)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingAuditLogService : IAuditLogService
    {
        public Task WriteAsync(AuditLogEntry entry, CancellationToken ct = default) =>
            throw new InvalidOperationException("監査ログの書込みに失敗しました。");
    }

    private sealed class FixedDbContextFactory : IDbContextFactory<AppDbContext>
    {
        private readonly AppDbContext _context;

        public FixedDbContextFactory(AppDbContext context)
        {
            _context = context;
        }

        public AppDbContext CreateDbContext()
        {
            return _context;
        }

        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_context);
        }
    }
}
