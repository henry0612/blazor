using System.Linq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Entities.Changes;
using UnifiedAccount.Domain.Entities.Core;

namespace UnifiedAccount.Domain.Tests.Entities;

public class ContractApplySchemaTests
{
    /// <summary>このテストでは 契約連番を保持できる。</summary>
    [Fact]
    public void Contract_ShouldHaveContractSeq()
    {
        var contract = new Contract { ContractSeq = 3 };

        contract.ContractSeq.Should().Be((short)3);
    }

    /// <summary>このテストでは 契約連番の既定値が 0 である。</summary>
    [Fact]
    public void Contract_ContractSeq_DefaultShouldBeZero()
    {
        new Contract().ContractSeq.Should().Be((short)0);
    }

    /// <summary>
    /// このテストでは 業務キーの索引が一意でない。
    /// KOZ085 が検出し KOZ090 が通すダブリ行が実データに混入しうるため。
    /// </summary>
    [Fact]
    public void Contract_BusinessKeyIndex_ShouldNotBeUnique()
    {
        var index = typeof(Contract)
            .GetCustomAttributes(typeof(IndexAttribute), false)
            .Cast<IndexAttribute>()
            .Single(a => a.PropertyNames.SequenceEqual(
                new[] { nameof(Contract.CompanyCode), nameof(Contract.PersonalCode), nameof(Contract.CheckDigit) }));

        index.IsUnique.Should().BeFalse();
    }

    /// <summary>このテストでは 会社、個人、契約連番の索引が存在する。</summary>
    [Fact]
    public void Contract_ShouldHaveCompanyPersonSeqIndex()
    {
        var indexes = typeof(Contract)
            .GetCustomAttributes(typeof(IndexAttribute), false)
            .Cast<IndexAttribute>();

        indexes.Should().Contain(a => a.PropertyNames.SequenceEqual(
            new[] { nameof(Contract.CompanyCode), nameof(Contract.PersonalCode), nameof(Contract.ContractSeq) }));
    }

    /// <summary>このテストでは 契約IDを保持できる。新規登録では null とする。</summary>
    [Fact]
    public void ContractChangeRequest_ShouldHaveNullableContractId()
    {
        var request = new ContractChangeRequest();

        request.ContractId.Should().BeNull();

        request.ContractId = 42L;
        request.ContractId.Should().Be(42L);
    }

    /// <summary>このテストでは 振替日を保持できる。抽出条件に使う。</summary>
    [Fact]
    public void ContractChangeRequest_ShouldHaveTransferDate()
    {
        var request = new ContractChangeRequest { TransferDate = new DateOnly(2026, 8, 12) };

        request.TransferDate.Should().Be(new DateOnly(2026, 8, 12));
    }
}
