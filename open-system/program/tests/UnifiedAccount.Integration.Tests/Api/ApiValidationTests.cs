using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace UnifiedAccount.Integration.Tests.Api;

public class ApiValidationTests : IClassFixture<ApiValidationWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiValidationTests(ApiValidationWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public async Task CreateCompany_InvalidPayload_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/companies", new
        {
            CompanyCode = "TOO-LONG",
            CompanyNameKana = "",
            ConsignorCode = "",
            BasicFee = -1,
            AdminFee = 100
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("CompanyCode");
        body.Should().Contain("CompanyNameKana");
        body.Should().Contain("ConsignorCode");
        body.Should().Contain("BasicFee");
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public async Task CreateContract_InvalidPayload_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/contracts", new
        {
            CompanyCode = "CMP001",
            PersonalCode = "1234567890123",
            CheckDigit = "12",
            DepositorNameKana = "",
            BankCode = "12A4",
            BranchCode = "1",
            AccountType = "12",
            AccountNo = "ABC",
            WithdrawalDay = 0,
            CurrentBillingAmount = -5
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("PersonalCode");
        body.Should().Contain("CheckDigit");
        body.Should().Contain("DepositorNameKana");
        body.Should().Contain("BankCode");
        body.Should().Contain("BranchCode");
        body.Should().Contain("AccountType");
        body.Should().Contain("AccountNo");
        body.Should().Contain("WithdrawalDay");
        body.Should().Contain("CurrentBillingAmount");
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public async Task ApplyCalendarTemplate_InvalidMonth_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/calendars/template", new
        {
            Year = 2026,
            Month = 13,
            TemplateType = "MONTHLY"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Month");
    }

    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public async Task ZenginSend_DefaultDate_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/zengin/send", new
        {
            WithdrawalDate = "0001-01-01",
            ConsignorCode = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("WithdrawalDate");
        body.Should().Contain("ConsignorCode");
    }
}
