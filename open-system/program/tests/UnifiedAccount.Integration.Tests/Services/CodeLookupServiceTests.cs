using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Infrastructure.Persistence;
using UnifiedAccount.Infrastructure.Services;

namespace UnifiedAccount.Integration.Tests.Services;

public class CodeLookupServiceTests
{
    /// <summary>このテストでは 未ロードの区分を指定したとき、null が返る。</summary>
    [Fact]
    public async Task LookupAsync_NotLoaded_ShouldReturnNull()
    {
        await using var context = CreateContext();
        var service = new CodeLookupService(context, new CodeLookupCache());

        var result = await service.LookupAsync("RESULT", "0");

        result.Should().BeNull();
    }

    /// <summary>このテストでは LoadCodes 後に区分とコード値を指定したとき、表示名が返る。</summary>
    [Fact]
    public async Task LookupAsync_AfterLoad_ShouldReturnValue()
    {
        await using var context = CreateContext();
        var service = new CodeLookupService(context, new CodeLookupCache());
        service.LoadCodes("RESULT", new Dictionary<string, string>
        {
            { "0", "正常" },
            { "1", "残高不足" },
            { "2", "口座なし" }
        });

        var result = await service.LookupAsync("RESULT", "0");

        result.Should().Be("正常");
    }

    /// <summary>このテストでは 未ロードの区分を指定したとき、空辞書が返る。</summary>
    [Fact]
    public async Task GetAllCodesAsync_NotLoaded_ShouldReturnEmpty()
    {
        await using var context = CreateContext();
        var service = new CodeLookupService(context, new CodeLookupCache());

        var result = await service.GetAllCodesAsync("UNKNOWN");

        result.Should().BeEmpty();
    }

    /// <summary>このテストでは LoadCodes 後に区分を指定したとき、全コードが返る。</summary>
    [Fact]
    public async Task GetAllCodesAsync_AfterLoad_ShouldReturnAllCodes()
    {
        await using var context = CreateContext();
        var service = new CodeLookupService(context, new CodeLookupCache());
        service.LoadCodes("BANK", new Dictionary<string, string>
        {
            { "0001", "みずほ銀行" },
            { "0005", "三菱UFJ銀行" },
            { "0009", "三井住友銀行" }
        });

        var result = await service.GetAllCodesAsync("BANK");

        result.Should().HaveCount(3);
        result["0001"].Should().Be("みずほ銀行");
    }

    /// <summary>このテストでは コード設定マスタを参照したとき、区分一覧が表示順で返る。</summary>
    [Fact]
    public async Task GetCategoriesAsync_ShouldReturnCategoryMasterRowsInDisplayOrder()
    {
        await using var context = CreateContext();
        context.CodeSettings.AddRange(
            new CodeSetting { CodeCategory = "00", CodeValue = "TransferMethod", DisplayText = "振替方法", DisplayOrder = 20 },
            new CodeSetting { CodeCategory = "00", CodeValue = "AccountType", DisplayText = "口座種目", DisplayOrder = 10 },
            new CodeSetting { CodeCategory = "AccountType", CodeValue = "1", DisplayText = "普通預金", DisplayOrder = 1 });
        await context.SaveChangesAsync();

        var service = new CodeLookupService(context, new CodeLookupCache());

        var result = await service.GetCategoriesAsync();

        result.Should().HaveCount(2);
        result.Select(item => item.CategoryCode).Should().ContainInOrder("AccountType", "TransferMethod");
        result[0].DisplayText.Should().Be("口座種目");
    }

    /// <summary>このテストでは 区分を指定したとき、コード値が表示順で返り変換後値も取得できる。</summary>
    [Fact]
    public async Task GetCodesAsync_ShouldReturnCodesInDisplayOrderWithChangeValue()
    {
        await using var context = CreateContext();
        context.CodeSettings.AddRange(
            new CodeSetting { CodeCategory = "AccountType", CodeValue = "2", DisplayText = "当座預金", DisplayOrder = 2 },
            new CodeSetting { CodeCategory = "AccountType", CodeValue = "1", DisplayText = "普通預金", ChangeValue = "F", DisplayOrder = 1 },
            new CodeSetting { CodeCategory = "TransferMethod", CodeValue = "1", DisplayText = "全銀", DisplayOrder = 1 });
        await context.SaveChangesAsync();

        var service = new CodeLookupService(context, new CodeLookupCache());

        var result = await service.GetCodesAsync("AccountType");

        result.Should().HaveCount(2);
        result.Select(item => item.CodeValue).Should().ContainInOrder("1", "2");
        result[0].ChangeValue.Should().Be("F");
        result[1].ChangeValue.Should().BeNull();
    }

    /// <summary>このテストでは 存在しない区分を指定したとき、空リストが返る。</summary>
    [Fact]
    public async Task GetCodesAsync_UnknownCategory_ShouldReturnEmpty()
    {
        await using var context = CreateContext();
        var service = new CodeLookupService(context, new CodeLookupCache());

        var result = await service.GetCodesAsync("UNKNOWN");

        result.Should().BeEmpty();
    }

    /// <summary>このテストでは 空文字の区分を指定したとき、ArgumentException となる。</summary>
    [Fact]
    public async Task GetCodesAsync_BlankCategory_ShouldThrow()
    {
        await using var context = CreateContext();
        var service = new CodeLookupService(context, new CodeLookupCache());

        var act = async () => await service.GetCodesAsync("  ");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    /// <summary>このテストでは コード設定マスタを参照したとき、区分とコード値からマスタの表示名が返る。</summary>
    [Fact]
    public async Task LookupAsync_ShouldReadFromCodeSettingsMaster()
    {
        await using var context = CreateContext();
        context.CodeSettings.Add(
            new CodeSetting { CodeCategory = "AccountType", CodeValue = "1", DisplayText = "普通預金", DisplayOrder = 1 });
        await context.SaveChangesAsync();

        var service = new CodeLookupService(context, new CodeLookupCache());

        (await service.LookupAsync("AccountType", "1")).Should().Be("普通預金");
        (await service.LookupAsync("AccountType", "99")).Should().BeNull();
    }

    /// <summary>このテストでは キャッシュ済みの状態で Invalidate したとき、マスタから再読込される。</summary>
    [Fact]
    public async Task Invalidate_ShouldForceReloadFromMaster()
    {
        await using var context = CreateContext();
        var service = new CodeLookupService(context, new CodeLookupCache());

        (await service.GetCodesAsync("AccountType")).Should().BeEmpty();

        context.CodeSettings.Add(
            new CodeSetting { CodeCategory = "AccountType", CodeValue = "1", DisplayText = "普通預金", DisplayOrder = 1 });
        await context.SaveChangesAsync();

        // Invalidate 前は 0 件キャッシュが有効。
        (await service.GetCodesAsync("AccountType")).Should().BeEmpty();

        service.Invalidate();

        (await service.GetCodesAsync("AccountType")).Should().ContainSingle(item => item.CodeValue == "1");
    }

    /// <summary>このテストでは キャッシュを共有したとき、別インスタンスの結果が再利用される。</summary>
    [Fact]
    public async Task GetCodesAsync_ShouldShareCacheAcrossInstances()
    {
        await using var context = CreateContext();
        var cache = new CodeLookupCache();
        context.CodeSettings.Add(
            new CodeSetting { CodeCategory = "AccountType", CodeValue = "1", DisplayText = "普通預金", DisplayOrder = 1 });
        await context.SaveChangesAsync();

        await new CodeLookupService(context, cache).GetCodesAsync("AccountType");

        cache.GetCodes("AccountType").Should().NotBeNull();
        var second = await new CodeLookupService(context, cache).GetCodesAsync("AccountType");
        second.Should().ContainSingle(item => item.DisplayText == "普通預金");
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"CodeLookupServiceTests_{Guid.NewGuid():N}")
            .Options;

        return new AppDbContext(options);
    }
}
