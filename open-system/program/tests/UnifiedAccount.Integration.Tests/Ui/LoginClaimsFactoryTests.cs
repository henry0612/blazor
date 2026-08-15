using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.Cookies;
using UnifiedAccount.Application.DTOs;
using UnifiedAccount.Domain.Enums;
using UnifiedAccount.Web.Components.Pages;

namespace UnifiedAccount.Integration.Tests.Ui;

/// <summary>
/// 222 F-INF-001 §7「認証成功時の Claims を正しく設定する」に対応するWeb層連携テスト。
/// </summary>
public class LoginClaimsFactoryTests
{
    /// <summary>
    /// このテストでは、認証成功結果から F-INF-001 §6 のマッピングどおりに
    /// NameIdentifier・Name・Role の3つのClaimが設定されることを検証する。
    /// </summary>
    [Fact]
    public void CreateIdentity_WithSuccessResult_MapsLoginResultToDefinedClaims()
    {
        var result = new LoginResult(true, "user01", "テストユーザー", UserRole.OperationManager);

        var identity = LoginClaimsFactory.CreateIdentity(
            result,
            CookieAuthenticationDefaults.AuthenticationScheme);

        identity.AuthenticationType.Should().Be(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.IsAuthenticated.Should().BeTrue();
        identity.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("user01");
        identity.FindFirst(ClaimTypes.Name)!.Value.Should().Be("テストユーザー");
        identity.FindFirst(ClaimTypes.Role)!.Value.Should().Be(nameof(UserRole.OperationManager));
        identity.Claims.Should().HaveCount(3);
    }

    /// <summary>
    /// このテストでは、Role Claim に列挙子の名称が設定され、
    /// Program.cs のロール要求ポリシー（F-INF-001 §6.1）と一致することを検証する。
    /// </summary>
    [Theory]
    [InlineData(UserRole.SystemAdmin)]
    [InlineData(UserRole.OperationManager)]
    [InlineData(UserRole.OperationStaff)]
    [InlineData(UserRole.Operator)]
    public void CreateIdentity_WithEachRole_SetsRoleClaimAsEnumName(UserRole role)
    {
        var result = new LoginResult(true, "user01", "テストユーザー", role);

        var identity = LoginClaimsFactory.CreateIdentity(
            result,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        identity.FindFirst(ClaimTypes.Role)!.Value.Should().Be(role.ToString());
        principal.IsInRole(role.ToString()).Should().BeTrue();
    }

    /// <summary>
    /// このテストでは、認証失敗結果からはClaimsを生成しないことを検証する
    /// （F-INF-001 §6「Claims は IsSuccess = true の場合だけ生成する」）。
    /// </summary>
    [Fact]
    public void CreateIdentity_WithFailureResult_ThrowsAndCreatesNoClaims()
    {
        var result = new LoginResult(false, ErrorMessage: "ユーザーIDまたはパスワードが正しくありません。");

        var act = () => LoginClaimsFactory.CreateIdentity(
            result,
            CookieAuthenticationDefaults.AuthenticationScheme);

        act.Should().Throw<InvalidOperationException>();
    }
}
