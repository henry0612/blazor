using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UnifiedAccount.Application.DTOs;
using UnifiedAccount.Application.Services;
using UnifiedAccount.Infrastructure.Persistence;
using UnifiedAccount.Infrastructure.Services;

namespace UnifiedAccount.Integration.Tests.Services;

public class CodeSettingServiceTests
{
    [Fact]
    public void CodeSettingUpdatedAt_ShouldBeConfiguredAsConcurrencyToken()
    {
        using var context = CreateContext();

        var property = context.Model
            .FindEntityType(typeof(Domain.Entities.Core.CodeSetting))!
            .FindProperty(nameof(Domain.Entities.Core.CodeSetting.UpdatedAt));

        property.Should().NotBeNull();
        property!.IsConcurrencyToken.Should().BeTrue();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldSetAuditTimestampsToLocalTime()
    {
        await using var context = CreateContext();
        var row = new Domain.Entities.Core.CodeSetting
        {
            CodeCategory = "BANK",
            CodeValue = "0001",
            DisplayText = "みずほ",
            DisplayOrder = 1
        };

        context.CodeSettings.Add(row);
        await context.SaveChangesAsync();

        row.CreatedAt.Kind.Should().Be(DateTimeKind.Local);
        row.UpdatedAt.Kind.Should().Be(DateTimeKind.Local);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnOnlyMatchingCategoryRows()
    {
        await using var context = CreateContext();
        context.CodeSettings.AddRange(
            new Domain.Entities.Core.CodeSetting { CodeCategory = "00", CodeValue = "BANK", DisplayText = "銀行", DisplayOrder = 10 },
            new Domain.Entities.Core.CodeSetting { CodeCategory = "00", CodeValue = "COMPANY", DisplayText = "会社", DisplayOrder = 20 },
            new Domain.Entities.Core.CodeSetting { CodeCategory = "BANK", CodeValue = "0001", DisplayText = "みずほ", DisplayOrder = 1 });
        await context.SaveChangesAsync();

        var service = new CodeSettingService(context);

        var categoryResult = await service.SearchAsync("00", null, 1, 20);
        var childResult = await service.SearchAsync("BANK", null, 1, 20);

        categoryResult.Items.Should().ContainSingle(item => item.CodeValue == "BANK");
        categoryResult.Items.Should().ContainSingle(item => item.CodeValue == "COMPANY");
        childResult.Items.Should().ContainSingle(item => item.CodeValue == "0001");
    }

    [Fact]
    public async Task SaveAsync_ShouldCreateAndUpdateRows()
    {
        await using var context = CreateContext();
        var service = new CodeSettingService(context);

        var created = await service.SaveAsync(new UnifiedAccount.Application.DTOs.CodeSettingEditRequest
        {
            CodeCategory = "BANK",
            CodeValue = "0001",
            DisplayText = "みずほ",
            DisplayOrder = 1
        });

        created.CodeValue.Should().Be("0001");

        var updated = await service.SaveAsync(new UnifiedAccount.Application.DTOs.CodeSettingEditRequest
        {
            Id = created.Id,
            CodeCategory = "BANK",
            CodeValue = "0001",
            DisplayText = "みずほ銀行",
            DisplayOrder = 2,
            OriginalUpdatedAt = created.UpdatedAt
        });

        updated.DisplayText.Should().Be("みずほ銀行");
        updated.DisplayOrder.Should().Be(2);

        var persisted = await context.CodeSettings.SingleAsync(x => x.Id == created.Id);
        persisted.DisplayText.Should().Be("みずほ銀行");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveRow()
    {
        await using var context = CreateContext();
        var row = new Domain.Entities.Core.CodeSetting { CodeCategory = "BANK", CodeValue = "0001", DisplayText = "みずほ", DisplayOrder = 1 };
        context.CodeSettings.Add(row);
        await context.SaveChangesAsync();

        var service = new CodeSettingService(context);
        await service.DeleteAsync(row.Id);

        (await context.CodeSettings.AnyAsync(x => x.Id == row.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task SaveBatchAsync_ShouldDeleteBeforeInsertingSameLogicalKey()
    {
        await using var context = CreateContext();
        var existing = new Domain.Entities.Core.CodeSetting
        {
            CodeCategory = "BANK",
            CodeValue = "0001",
            DisplayText = "旧",
            DisplayOrder = 1
        };
        context.CodeSettings.Add(existing);
        await context.SaveChangesAsync();

        var service = new CodeSettingService(context);
        await service.SaveBatchAsync(
            new[]
            {
                new CodeSettingEditRequest
                {
                    CodeCategory = "BANK",
                    CodeValue = "0001",
                    DisplayText = "新",
                    DisplayOrder = 2
                }
            },
            new[]
            {
                new CodeSettingDeleteRequest
                {
                    Id = existing.Id,
                    OriginalUpdatedAt = existing.UpdatedAt
                }
            });

        (await context.CodeSettings.SingleAsync()).DisplayText.Should().Be("新");
    }

    [Fact]
    public async Task SaveBatchAsync_ShouldRejectOptimisticConcurrencyConflict()
    {
        await using var context = CreateContext();
        var row = new Domain.Entities.Core.CodeSetting
        {
            CodeCategory = "BANK",
            CodeValue = "0001",
            DisplayText = "旧",
            DisplayOrder = 1
        };
        context.CodeSettings.Add(row);
        await context.SaveChangesAsync();

        var service = new CodeSettingService(context);
        var action = () => service.SaveBatchAsync(
            new[]
            {
                new CodeSettingEditRequest
                {
                    Id = row.Id,
                    CodeCategory = "BANK",
                    CodeValue = "0001",
                    DisplayText = "自分",
                    DisplayOrder = 1,
                    OriginalUpdatedAt = row.UpdatedAt.AddSeconds(-1)
                }
            },
            Array.Empty<CodeSettingDeleteRequest>());

        var exception = await action.Should().ThrowAsync<DbUpdateConcurrencyException>();
        exception.Which.Entries.Should().ContainSingle();
        await context.Entry(row).ReloadAsync();
        row.DisplayText.Should().Be("旧");
    }

    [Fact]
    public async Task SaveAsync_ShouldRaiseEfConcurrencyExceptionWhenOriginalUpdatedAtIsStale()
    {
        await using var context = CreateContext();
        var row = new Domain.Entities.Core.CodeSetting
        {
            CodeCategory = "BANK",
            CodeValue = "0001",
            DisplayText = "旧",
            DisplayOrder = 1
        };
        context.CodeSettings.Add(row);
        await context.SaveChangesAsync();

        var service = new CodeSettingService(context);
        var action = () => service.SaveAsync(new CodeSettingEditRequest
        {
            Id = row.Id,
            CodeCategory = "BANK",
            CodeValue = "0001",
            DisplayText = "自分",
            DisplayOrder = 1,
            OriginalUpdatedAt = row.UpdatedAt.AddSeconds(-1)
        });

        var exception = await action.Should().ThrowAsync<DbUpdateConcurrencyException>();
        exception.Which.Entries.Should().ContainSingle();
        await context.Entry(row).ReloadAsync();
        row.DisplayText.Should().Be("旧");
    }

    [Fact]
    public async Task SaveBatchAsync_ShouldRejectDeleteWhenUpdatedAtIsStale()
    {
        await using var context = CreateContext();
        var row = new Domain.Entities.Core.CodeSetting
        {
            CodeCategory = "BANK",
            CodeValue = "0001",
            DisplayText = "旧",
            DisplayOrder = 1
        };
        context.CodeSettings.Add(row);
        await context.SaveChangesAsync();

        var service = new CodeSettingService(context);
        var action = () => service.SaveBatchAsync(
            Array.Empty<CodeSettingEditRequest>(),
            new[]
            {
                new CodeSettingDeleteRequest
                {
                    Id = row.Id,
                    OriginalUpdatedAt = row.UpdatedAt.AddSeconds(-1)
                }
            });

        var exception = await action.Should().ThrowAsync<DbUpdateConcurrencyException>();
        exception.Which.Entries.Should().ContainSingle();
        (await context.CodeSettings.AnyAsync(item => item.Id == row.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task SaveBatchAsync_ShouldRejectExistingRowWithoutOriginalUpdatedAt()
    {
        await using var context = CreateContext();
        var row = new Domain.Entities.Core.CodeSetting
        {
            CodeCategory = "BANK",
            CodeValue = "0001",
            DisplayText = "旧",
            DisplayOrder = 1
        };
        context.CodeSettings.Add(row);
        await context.SaveChangesAsync();

        var service = new CodeSettingService(context);
        var action = () => service.SaveBatchAsync(
            new[]
            {
                new CodeSettingEditRequest
                {
                    Id = row.Id,
                    CodeCategory = "BANK",
                    CodeValue = "0001",
                    DisplayText = "自分",
                    DisplayOrder = 1
                }
            },
            Array.Empty<CodeSettingDeleteRequest>());

        await action.Should().ThrowAsync<ArgumentException>();
        (await context.CodeSettings.SingleAsync()).DisplayText.Should().Be("旧");
    }

    [Fact]
    public async Task SaveBatchAsync_ShouldRejectDeleteRequestWithoutOriginalUpdatedAt()
    {
        await using var context = CreateContext();
        var service = new CodeSettingService(context);

        var action = () => service.SaveBatchAsync(
            Array.Empty<CodeSettingEditRequest>(),
            new[]
            {
                new CodeSettingDeleteRequest
                {
                    Id = 10
                }
            });

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SaveBatchAsync_ShouldWriteAuditEntriesBeforeCommit()
    {
        await using var context = CreateContext();
        var auditLogService = new RecordingAuditLogService();
        var service = new CodeSettingService(context, auditLogService);
        var auditEntry = new AuditLogEntry
        {
            OccurredAt = DateTimeOffset.UtcNow,
            ActorId = "admin01",
            FeatureId = "F-ONL-100",
            Action = "Save",
            TargetType = "CodeSetting",
            TargetKey = "{\"CodeCategory\":\"BANK\",\"CodeValue\":\"0001\"}",
            Result = "Success",
            CorrelationId = "correlation-01",
            BeforeValuesJson = "{}",
            AfterValuesJson = "{}"
        };

        await service.SaveBatchAsync(
            new[]
            {
                new CodeSettingEditRequest
                {
                    CodeCategory = "BANK",
                    CodeValue = "0001",
                    DisplayText = "みずほ",
                    DisplayOrder = 1
                }
            },
            Array.Empty<CodeSettingDeleteRequest>(),
            auditEntries: [auditEntry]);

        auditLogService.Entries.Should().ContainSingle().Which.Should().Be(auditEntry);
    }

    /// <summary>このテストでは 空のマスタを検索したとき、行が登録されない。</summary>
    [Fact]
    public async Task SearchAsync_EmptyMaster_ShouldNotInsertAnyRow()
    {
        await using var context = CreateContext();
        var service = new CodeSettingService(context);

        var result = await service.SearchAsync("00", null, 1, 20);

        result.Items.Should().BeEmpty();
        (await context.CodeSettings.CountAsync()).Should().Be(0);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"CodeSettingServiceTests_{Guid.NewGuid():N}")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
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
}
