using FluentAssertions;
using UnifiedAccount.Domain.Enums;

namespace UnifiedAccount.Domain.Tests.Entities;

public class UserRoleTests
{
    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void UserRole_ShouldDefine4Roles()
    {
        Enum.GetValues<UserRole>().Should().HaveCount(4);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void UserRole_ShouldHaveExpectedValues()
    {
        ((int)UserRole.SystemAdmin).Should().Be(0);
        ((int)UserRole.OperationManager).Should().Be(1);
        ((int)UserRole.OperationStaff).Should().Be(2);
        ((int)UserRole.Operator).Should().Be(3);
    }
}










