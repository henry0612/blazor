using FluentAssertions;
using UnifiedAccount.Domain.Entities.Core;

namespace UnifiedAccount.Domain.Tests.Entities;

public class InstitutionEncodingTests
{
    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public void InstitutionEncoding_ShouldHaveShiftJisDefaults()
    {
        var encoding = new InstitutionEncoding();

        encoding.BankCode.Should().Be(string.Empty);
        encoding.TransferDataEncoding.Should().Be("shift_jis");
        encoding.ResultDataEncoding.Should().Be("shift_jis");
        encoding.ReconcileDataEncoding.Should().Be("shift_jis");
    }

    /// <summary>このテストでは 属性を設定したとき、入力値が保持される。</summary>
    [Fact]
    public void InstitutionEncoding_ShouldStoreProperties()
    {
        var encoding = new InstitutionEncoding
        {
            BankCode = "0143",
            BankName = "八十二銀行",
            TransferDataEncoding = "ibm930",
            ResultDataEncoding = "ibm930",
            ReconcileDataEncoding = "ibm939",
            Remarks = "EBCDIC連携"
        };

        encoding.BankCode.Should().Be("0143");
        encoding.BankName.Should().Be("八十二銀行");
        encoding.TransferDataEncoding.Should().Be("ibm930");
        encoding.ResultDataEncoding.Should().Be("ibm930");
        encoding.ReconcileDataEncoding.Should().Be("ibm939");
        encoding.Remarks.Should().Be("EBCDIC連携");
    }

    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public void InstitutionEncoding_OptionalFields_ShouldBeNullByDefault()
    {
        var encoding = new InstitutionEncoding();

        encoding.BankName.Should().BeNull();
        encoding.Remarks.Should().BeNull();
    }

    /// <summary>このテストでは 文字コードを設定したとき、文字コード設定が反映される。</summary>
    [Fact]
    public void InstitutionEncoding_ShouldSupportMixedEncodings()
    {
        var encoding = new InstitutionEncoding
        {
            BankCode = "9900",
            TransferDataEncoding = "ibm930",
            ResultDataEncoding = "shift_jis",
            ReconcileDataEncoding = "ibm939"
        };

        encoding.TransferDataEncoding.Should().Be("ibm930");
        encoding.ResultDataEncoding.Should().Be("shift_jis");
        encoding.ReconcileDataEncoding.Should().Be("ibm939");
    }

    /// <summary>このテストでは 文字コードを設定したとき、文字コード設定が反映される。</summary>
    [Fact]
    public void InstitutionEncoding_BaseEntity_ShouldHaveIdAndTimestamps()
    {
        var encoding = new InstitutionEncoding();

        encoding.Id.Should().Be(0);
        encoding.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        encoding.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }
}









