using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Web.Api;

namespace UnifiedAccount.Integration.Tests.Api;

public sealed class BankBranchEndpointsTests : IClassFixture<BankBranchEndpointsWebApplicationFactory>
{
    private readonly BankBranchEndpointsWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public BankBranchEndpointsTests(BankBranchEndpointsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Test-Role", "SystemAdmin");
    }

    [Fact]
    public async Task GetAll_ShouldApplyBankAndBranchFiltersWithAndSemantics()
    {
        await _factory.SeedAsync(
            new BankBranch
            {
                BankCode = "0001",
                BranchCode = "001",
                BankNameKana = "ﾐｽﾞﾎ",
                BranchNameKana = "ﾎﾝﾃﾝ"
            },
            new BankBranch
            {
                BankCode = "0001",
                BranchCode = "002",
                BankNameKana = "ﾐｽﾞﾎ",
                BranchNameKana = "ｼﾃﾝ"
            });

        var response = await _client.GetFromJsonAsync<BankBranchListResponse>(
            "/api/bank-branches/?bankCode=00&branchCode=001&pageSize=50");

        response.Should().NotBeNull();
        response!.Items.Should().OnlyContain(item =>
            item.BankCode.Contains("00") && item.BranchCode.Contains("001"));
        response.PageSize.Should().Be(50);
    }

    [Fact]
    public async Task Create_ShouldNormalizeKana()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/bank-branches/",
            new BankBranchEndpoints.CreateBankBranchRequest(
                "0099",
                "999",
                "ガ",
                "パ",
                null,
                null));

        response.StatusCode.Should().Be(
            HttpStatusCode.Created,
            await response.Content.ReadAsStringAsync());
        var created = await response.Content.ReadFromJsonAsync<BankBranch>();
        created!.BankNameKana.Should().Be("ｶﾞ");
        created.BranchNameKana.Should().Be("ﾊﾟ");
    }

    [Fact]
    public async Task Create_ShouldRejectInvalidKana()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/bank-branches/",
            new
            {
                bankCode = "0098",
                branchCode = "998",
                bankNameKana = "Ａ",
                branchNameKana = "ﾎﾝﾃﾝ",
                bankNameKanji = (string?)null,
                branchNameKanji = (string?)null
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("BankNameKana");
    }

    [Fact]
    public async Task Update_ShouldReturnConflictWhenUpdatedAtDiffers()
    {
        await _factory.SeedAsync(new BankBranch
        {
            BankCode = "0097",
            BranchCode = "997",
            BankNameKana = "ﾐｽﾞﾎ",
            BranchNameKana = "ﾎﾝﾃﾝ"
        });
        var current = await GetAsync("0097", "997");

        var response = await _client.PutAsJsonAsync(
            "/api/bank-branches/0097/997",
            new BankBranchEndpoints.UpdateBankBranchRequest(
                "ﾐｽﾞﾎ",
                "ｼﾃﾝ",
                null,
                null,
                current.UpdatedAt.AddMinutes(-1)));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Update_ShouldClearKanjiWhenNullIsSent()
    {
        await _factory.SeedAsync(new BankBranch
        {
            BankCode = "0096",
            BranchCode = "996",
            BankNameKana = "ﾐｽﾞﾎ",
            BranchNameKana = "ﾎﾝﾃﾝ",
            BankNameKanji = "銀行",
            BranchNameKanji = "本店",
            KanjiSetFlag = "1"
        });
        var current = await GetAsync("0096", "996");

        var response = await _client.PutAsJsonAsync(
            "/api/bank-branches/0096/996",
            new BankBranchEndpoints.UpdateBankBranchRequest(
                "ﾐｽﾞﾎ",
                "ﾎﾝﾃﾝ",
                null,
                null,
                current.UpdatedAt));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updated = await GetAsync("0096", "996");
        updated.BankNameKanji.Should().BeNull();
        updated.BranchNameKanji.Should().BeNull();
        updated.KanjiSetFlag.Should().Be("0");
    }

    [Fact]
    public async Task Delete_ShouldRemoveTheRowWhenUpdatedAtMatches()
    {
        await _factory.SeedAsync(new BankBranch
        {
            BankCode = "0095",
            BranchCode = "995",
            BankNameKana = "ﾐｽﾞﾎ",
            BranchNameKana = "ﾎﾝﾃﾝ"
        });
        var current = await GetAsync("0095", "995");

        var response = await _client.SendAsync(new HttpRequestMessage(
            HttpMethod.Delete,
            "/api/bank-branches/0095/995")
        {
            Content = JsonContent.Create(
                new BankBranchEndpoints.DeleteBankBranchRequest(current.UpdatedAt))
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await _client.GetAsync("/api/bank-branches/0095/995"))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<BankBranch> GetAsync(string bankCode, string branchCode)
    {
        var response = await _client.GetAsync($"/api/bank-branches/{bankCode}/{branchCode}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<BankBranch>())!;
    }
}
