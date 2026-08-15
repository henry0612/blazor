using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using UnifiedAccount.Domain.Entities.Core;

namespace UnifiedAccount.Domain.Tests.Entities;

public class CodeSettingTests
{
    [Fact]
    public void CodeSetting_ShouldHaveDefaultValues()
    {
        var setting = new CodeSetting();

        setting.CodeCategory.Should().Be(string.Empty);
        setting.CodeValue.Should().Be(string.Empty);
        setting.DisplayText.Should().Be(string.Empty);
        setting.DisplayOrder.Should().Be(0);
    }

    [Fact]
    public void CodeSetting_ShouldStoreProperties()
    {
        var setting = new CodeSetting
        {
            CodeCategory = "00",
            CodeValue = "BANK",
            DisplayText = "銀行",
            DisplayOrder = 10
        };

        setting.CodeCategory.Should().Be("00");
        setting.CodeValue.Should().Be("BANK");
        setting.DisplayText.Should().Be("銀行");
        setting.DisplayOrder.Should().Be(10);
    }

    [Fact]
    public void UpdatedAt_ShouldBeMarkedAsConcurrencyCheck()
    {
        var property = typeof(CodeSetting).GetProperty(nameof(CodeSetting.UpdatedAt));

        property.Should().NotBeNull();
        property!.IsDefined(typeof(ConcurrencyCheckAttribute), inherit: false).Should().BeTrue();
    }
}
