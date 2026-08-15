using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Entities.Changes;

namespace UnifiedAccount.Domain.Tests.Entities;

public class CompanyChangeRequestSchemaTests
{
    [Fact]
    public void CompanyName_MaxLength_ShouldBe40()
    {
        var prop = typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.CompanyName));
        var attr = prop!.GetCustomAttributes(typeof(MaxLengthAttribute), false).Single() as MaxLengthAttribute;

        attr!.Length.Should().Be(40);
    }

    [Fact]
    public void CompanyId_ShouldNotExist()
    {
        typeof(CompanyChangeRequest)
            .GetProperty("CompanyId")
            .Should()
            .BeNull();
    }

    [Fact]
    public void CompanyCode_ShouldRemainAvailable()
    {
        typeof(CompanyChangeRequest)
            .GetProperty(nameof(CompanyChangeRequest.CompanyCode))
            .Should()
            .NotBeNull();
    }

    [Fact]
    public void PageChangeKey_ShouldBeDecimal()
    {
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.PageChangeKey))!
            .PropertyType.Should().Be(typeof(decimal));
    }

    [Fact]
    public void SelectedStringColumns_ShouldHaveExpectedMaxLength()
    {
        AssertMaxLength(nameof(CompanyChangeRequest.Cban), 1);
        AssertMaxLength(nameof(CompanyChangeRequest.CompanyNameKanji), 30);
        AssertMaxLength(nameof(CompanyChangeRequest.DepartmentKanji), 20);
        AssertMaxLength(nameof(CompanyChangeRequest.PersonInChargeKanji), 20);
        AssertMaxLength(nameof(CompanyChangeRequest.PassbookComment), 8);
        AssertMaxLength(nameof(CompanyChangeRequest.TypeName), 12);
    }

    [Fact]
    public void SelectedDecimalColumns_ShouldHaveExpectedPrecision()
    {
        AssertPrecision(nameof(CompanyChangeRequest.BasicFee), 6, 0);
        AssertPrecision(nameof(CompanyChangeRequest.AdminFee), 6, 0);
        AssertPrecision(nameof(CompanyChangeRequest.PageChangeKey), 2, 0);
        AssertPrecision(nameof(CompanyChangeRequest.Amount), 8, 0);
        AssertPrecision(nameof(CompanyChangeRequest.NewUnitPrice), 4, 0);
        AssertPrecision(nameof(CompanyChangeRequest.ModifyUnitPrice), 4, 0);
        AssertPrecision(nameof(CompanyChangeRequest.Transfer1UnitPrice), 4, 0);
        AssertPrecision(nameof(CompanyChangeRequest.Transfer2UnitPrice), 4, 0);
        AssertPrecision(nameof(CompanyChangeRequest.ReceiptUnitPrice), 4, 0);
    }

    [Fact]
    public void Denku71_Columns_ShouldExist()
    {
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.CompanyNameKanji)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.DepartmentKanji)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.PersonInChargeKanji))
            .Should().NotBeNull();
    }

    [Fact]
    public void Denku16To19_Columns_ShouldExist()
    {
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.TypeCategory)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.Cycle)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.OperatingYear)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.OperatingMonth)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.TypeName)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.Amount)).Should().NotBeNull();
    }

    [Fact]
    public void Denku13_14_15_Columns_ShouldExist()
    {
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.ProcessingFlag1)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.ProcessingFlag12)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.SheetFlag1)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.SheetFlag12)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.PageChangeKey)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.NewUnitPrice)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.ModifyUnitPrice)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.Transfer1UnitPrice)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.Transfer2UnitPrice)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.ReceiptUnitPrice)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.Transfer2BankCode)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.Transfer2BranchCode)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.Transfer2AccountType))
            .Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.Transfer2AccountNo)).Should().NotBeNull();
        typeof(CompanyChangeRequest).GetProperty(nameof(CompanyChangeRequest.PassbookComment)).Should().NotBeNull();
    }

    private static void AssertMaxLength(string propertyName, int expectedLength)
    {
        var prop = typeof(CompanyChangeRequest).GetProperty(propertyName);
        var attr = prop!.GetCustomAttributes(typeof(MaxLengthAttribute), false).Single() as MaxLengthAttribute;

        attr!.Length.Should().Be(expectedLength);
    }

    private static void AssertPrecision(string propertyName, int expectedPrecision, int expectedScale)
    {
        var prop = typeof(CompanyChangeRequest).GetProperty(propertyName);
        var attr = prop!.GetCustomAttributes(typeof(PrecisionAttribute), false).Single() as PrecisionAttribute;

        attr!.Precision.Should().Be(expectedPrecision);
        attr.Scale.Should().Be(expectedScale);
    }
}
