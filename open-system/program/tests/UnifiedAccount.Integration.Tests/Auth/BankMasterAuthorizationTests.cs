using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UnifiedAccount.Domain.Enums;
using UnifiedAccount.Web.Api;
using UnifiedAccount.Integration.Tests.Api;

namespace UnifiedAccount.Integration.Tests.Auth;

public sealed class BankMasterAuthorizationTests : IClassFixture<BankBranchEndpointsWebApplicationFactory>
{
    private readonly BankBranchEndpointsWebApplicationFactory _factory;

    public BankMasterAuthorizationTests(BankBranchEndpointsWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData(UserRole.SystemAdmin)]
    [InlineData(UserRole.OperationManager)]
    [InlineData(UserRole.OperationStaff)]
    [InlineData(UserRole.Operator)]
    public async Task BankBranchRead_ShouldAllowAllAuthenticatedRoles(UserRole role)
    {
        using var client = CreateClient(role);

        var response = await client.GetAsync("/api/bank-branches/");

        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// 223 F-ONL-007 v1.4「画面は認可を独自に処理せず、FallbackPolicyだけに従う」を検証する。
    /// メニューから到達できる認証済み利用者であれば、ロールによらず登録できる。
    /// </summary>
    [Theory]
    [InlineData(UserRole.SystemAdmin, "0090", "900")]
    [InlineData(UserRole.OperationManager, "0091", "901")]
    [InlineData(UserRole.OperationStaff, "0092", "902")]
    [InlineData(UserRole.Operator, "0093", "903")]
    public async Task BankBranchWrite_ShouldAllowAllAuthenticatedRoles(UserRole role, string bankCode, string branchCode)
    {
        using var client = CreateClient(role);

        var response = await client.PostAsJsonAsync(
            "/api/bank-branches/",
            new BankBranchEndpoints.CreateBankBranchRequest(
                bankCode,
                branchCode,
                "ﾐｽﾞﾎ",
                "ﾎﾝﾃﾝ",
                null,
                null));

        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// このテストでは、BankEdit が画面固有の認可ポリシーを宣言していないことを検証する
    /// （223 F-ONL-007 v1.4、FallbackPolicyだけに従う）。
    /// </summary>
    [Fact]
    public void BankEdit_ShouldNotDeclareItsOwnAuthorizationPolicy()
    {
        var attributes = typeof(UnifiedAccount.Web.Components.Pages.Bank.BankEdit)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), inherit: true);

        attributes.Should().BeEmpty(
            "F-ONL-007は認可を独自に処理せず、FallbackPolicy（認証必須）だけに従うため。");
    }

    private HttpClient CreateClient(UserRole role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", role.ToString());
        return client;
    }
}
