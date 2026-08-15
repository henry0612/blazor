using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using UnifiedAccount.Web.Components.Pages.Transfer;

namespace UnifiedAccount.Integration.Tests.Auth;

/// <summary>
/// 223 F-ONL-006 v1.9「画面は認可を独自に処理せず、FallbackPolicyだけに従う」を検証する。
/// FallbackPolicy による未認証アクセスの拒否は AuthorizationTests で検証済みのため、
/// ここでは AmountEdit が画面固有の認可ポリシーを持たないことだけを確認する。
/// </summary>
public sealed class TransferAmountAuthorizationTests
{
    [Fact]
    public void AmountEdit_ShouldNotDeclareItsOwnAuthorizationPolicy()
    {
        var attributes = typeof(AmountEdit)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true);

        attributes.Should().BeEmpty(
            "F-ONL-006は認可を独自に処理せず、FallbackPolicy（認証必須）だけに従うため。");
    }
}
