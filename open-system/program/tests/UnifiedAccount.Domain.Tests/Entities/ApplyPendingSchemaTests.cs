using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;
using UnifiedAccount.Domain.Entities.Apply;
using UnifiedAccount.Domain.Entities.Core;

namespace UnifiedAccount.Domain.Tests.Entities;

public class ApplyPendingSchemaTests
{
    // Contract の業務プロパティのうち、ContractApplyPending が意図的に持たない列。
    // ContractSeq: 確定処理の直後に会社＋個人グループ単位で再採番される値であり、
    // 確定前の Pending では値が定まらないため保持しない。
    private static readonly string[] ExcludedContractPropertyNames =
    {
        "ContractSeq",
    };

    // TransferAmount の業務プロパティのうち、AmountApplyPending が意図的に持たない列。
    // 現時点ではすべての業務列を保持するため、除外対象はない。
    private static readonly string[] ExcludedTransferAmountPropertyNames = Array.Empty<string>();

    // ContractApplyPending 固有の制御列（Contract には存在しない）。
    // ShouldNotHaveColumnsMissingFromContract の逆方向比較で、業務列の対応漏れと誤検知しないための許可リスト。
    private static readonly string[] ContractApplyPendingControlPropertyNames =
    {
        "ApplyRunId", "ApplyRun", "Source", "Operation", "ContractId", "SourceRequestId",
        "ValidationFlags", "IsApplicable",

        // TM_ContractTypes / TM_ContractBillingAmounts（いずれも OCCURS 4 の子テーブル）を
        // ContractApplyPending が横展開した列。対応元は Contract 自身ではなく子テーブルの
        // ContractType / ContractBillingAmount（1種別=1行）であるため、Contract との一致比較の
        // 対象外とする。子テーブルの行の有無を Pending が表現できない点自体は別途の設計判断待ち
        // （本レビューでは対応しないものとして triage 済み）であり、本テストが検証する範囲外。
        "Type1StartYearMonth", "Type1Cycle", "Type1Amount",
        "Type2StartYearMonth", "Type2Cycle", "Type2Amount",
        "Type3StartYearMonth", "Type3Cycle", "Type3Amount",
        "Type4StartYearMonth", "Type4Cycle", "Type4Amount",
        "Billing1Amount", "Billing2Amount", "Billing3Amount", "Billing4Amount",
    };

    // AmountApplyPending 固有の制御列（TransferAmount には存在しない）。
    // ShouldNotHaveColumnsMissingFromTransferAmount の逆方向比較で、業務列の対応漏れと誤検知しないための許可リスト。
    private static readonly string[] AmountApplyPendingControlPropertyNames =
    {
        "ApplyRunId", "ApplyRun", "Operation", "TransferAmountId", "SourceModificationId",
        "ValidationFlags", "IsApplicable",
    };
    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public void ContractApplyPending_ShouldHaveDefaultValues()
    {
        var pending = new ContractApplyPending();

        pending.Source.Should().Be(string.Empty);
        pending.Operation.Should().Be(string.Empty);
        pending.ContractId.Should().BeNull();
        pending.SourceRequestId.Should().BeNull();
        pending.CompanyCode.Should().Be(string.Empty);
        pending.PersonalCode.Should().Be(string.Empty);
        pending.CheckDigit.Should().Be(string.Empty);
        pending.ValidationFlags.Should().Be("000000000000000");
        pending.IsApplicable.Should().BeTrue();
    }

    /// <summary>このテストでは テーブル名が TD_ContractApplyPending である。</summary>
    [Fact]
    public void ContractApplyPending_TableName_ShouldBeTdContractApplyPending()
    {
        var attr = typeof(ContractApplyPending)
            .GetCustomAttributes(typeof(TableAttribute), false)
            .Single() as TableAttribute;

        attr!.Name.Should().Be("TD_ContractApplyPending");
    }

    /// <summary>このテストでは ValidationFlags が 15 桁固定である。</summary>
    [Fact]
    public void ContractApplyPending_ValidationFlags_MaxLength_ShouldBe15()
    {
        var prop = typeof(ContractApplyPending).GetProperty(nameof(ContractApplyPending.ValidationFlags));
        var attr = prop!.GetCustomAttributes(typeof(MaxLengthAttribute), false).Single() as MaxLengthAttribute;

        attr!.Length.Should().Be(15);
    }

    /// <summary>このテストでは 種別と請求額が 4 件ずつ横展開されている。</summary>
    [Fact]
    public void ContractApplyPending_ShouldHaveFourTypeAndBillingSlots()
    {
        var type = typeof(ContractApplyPending);

        type.GetProperty("Type1Amount").Should().NotBeNull();
        type.GetProperty("Type4Amount").Should().NotBeNull();
        type.GetProperty("Billing1Amount").Should().NotBeNull();
        type.GetProperty("Billing4Amount").Should().NotBeNull();
        type.GetProperty("Type5Amount").Should().BeNull();
    }

    /// <summary>このテストでは 更新日時を持たない。作り直される表であるため。</summary>
    [Fact]
    public void ContractApplyPending_ShouldNotBeUpdated()
    {
        typeof(ContractApplyPending)
            .GetProperty("UpdatedAt")!
            .DeclaringType!.Name
            .Should().Be("BaseEntity");
    }

    /// <summary>
    /// このテストでは Contract の業務列が ContractApplyPending に同名・同型・同属性で存在する。
    /// V_ContractsApplied が TM_Contracts と TD_ContractApplyPending を UNION する前提であるため、
    /// Contract 側の列追加や MaxLength/Precision の変更に Pending 側が追随できていることを
    /// リフレクションで検証する。目視確認だけに頼ると追随漏れが静かに壊れるため、この一致を
    /// 自動テストで固定する。
    /// </summary>
    [Fact]
    public void ContractApplyPending_ShouldMirrorContractBusinessColumns()
    {
        var mismatches = GetBusinessColumnMismatches(
            typeof(Contract), typeof(ContractApplyPending), ExcludedContractPropertyNames);

        mismatches.Should().BeEmpty(
            "Contract と ContractApplyPending の業務列は V_ContractsApplied の UNION 前提のため一致している必要がある。" +
            "不一致内容: " + string.Join(" / ", mismatches));
    }

    /// <summary>
    /// このテストでは ContractApplyPending の業務列（制御列を除く）が Contract に存在する。
    /// ShouldMirrorContractBusinessColumns は Contract → ContractApplyPending の一方向しか検証しないため、
    /// Pending 側にだけ余分な列が追加された場合を検出できない。UNION の両辺が対応している必要があるため、
    /// 逆方向もあわせて固定する。
    /// </summary>
    [Fact]
    public void ContractApplyPending_ShouldNotHaveColumnsMissingFromContract()
    {
        var mismatches = GetPendingOnlyPropertyMismatches(
            typeof(Contract), typeof(ContractApplyPending), ContractApplyPendingControlPropertyNames);

        mismatches.Should().BeEmpty(
            "ContractApplyPending の業務列は Contract に存在する必要がある（V_ContractsApplied の UNION 前提）。" +
            "不一致内容: " + string.Join(" / ", mismatches));
    }

    /// <summary>
    /// このテストでは TransferAmount の業務列が AmountApplyPending に同名・同型・同属性で存在する。
    /// 確定フェーズが AmountApplyPending を TD_TransferAmounts へ MERGE する前提であるため、
    /// TransferAmount 側の列追加や MaxLength/Precision の変更に Pending 側が追随できていることを
    /// リフレクションで検証する。ContractApplyPending と同じ理由で固定する。
    /// </summary>
    [Fact]
    public void AmountApplyPending_ShouldMirrorTransferAmountBusinessColumns()
    {
        var mismatches = GetBusinessColumnMismatches(
            typeof(TransferAmount), typeof(AmountApplyPending), ExcludedTransferAmountPropertyNames);

        mismatches.Should().BeEmpty(
            "TransferAmount と AmountApplyPending の業務列は確定フェーズの MERGE 前提のため一致している必要がある。" +
            "不一致内容: " + string.Join(" / ", mismatches));
    }

    /// <summary>
    /// このテストでは AmountApplyPending の業務列（制御列を除く）が TransferAmount に存在する。
    /// ContractApplyPending と同じ理由で、確定フェーズの MERGE の両辺が対応していることを逆方向でも固定する。
    /// </summary>
    [Fact]
    public void AmountApplyPending_ShouldNotHaveColumnsMissingFromTransferAmount()
    {
        var mismatches = GetPendingOnlyPropertyMismatches(
            typeof(TransferAmount), typeof(AmountApplyPending), AmountApplyPendingControlPropertyNames);

        mismatches.Should().BeEmpty(
            "AmountApplyPending の業務列は TransferAmount に存在する必要がある（確定フェーズの MERGE 前提）。" +
            "不一致内容: " + string.Join(" / ", mismatches));
    }

    /// <summary>
    /// 対応元エンティティ（Contract / TransferAmount）の業務列が、Pending 側エンティティに
    /// 同名・同型・同属性で存在するかを比較し、不一致の一覧を返す。
    /// 呼び出し元のテストが対応元とPending側の組み合わせごとに失敗メッセージへ反映するため、
    /// ここでは差分の収集のみを行う。
    /// </summary>
    private static List<string> GetBusinessColumnMismatches(
        Type sourceType, Type pendingType, string[] excludedPropertyNames)
    {
        var mismatches = new List<string>();

        foreach (var sourceProperty in GetBusinessColumns(sourceType, excludedPropertyNames))
        {
            var pendingProperty = pendingType.GetProperty(sourceProperty.Name);
            if (pendingProperty is null)
            {
                mismatches.Add($"{sourceProperty.Name}: {pendingType.Name} に存在しない");
                continue;
            }

            CompareTypeSignature(sourceProperty, pendingProperty, mismatches);
            CompareMaxLength(sourceProperty, pendingProperty, mismatches);
            ComparePrecision(sourceProperty, pendingProperty, mismatches);
        }

        return mismatches;
    }

    /// <summary>
    /// pendingType の業務プロパティ（制御列を除く）のうち、sourceType に同名のプロパティが
    /// 存在しないものの一覧を返す。GetBusinessColumnMismatches は sourceType → pendingType の
    /// 一方向しか見ないため、Pending 側にだけ追加された余分な列を検出できない。UNION/MERGE は
    /// 両辺の列対応が前提のため、この逆方向チェックとあわせて初めて保証が成立する。
    /// </summary>
    private static List<string> GetPendingOnlyPropertyMismatches(
        Type sourceType, Type pendingType, string[] pendingControlPropertyNames)
    {
        var mismatches = new List<string>();

        foreach (var pendingProperty in GetBusinessColumns(pendingType, pendingControlPropertyNames))
        {
            if (sourceType.GetProperty(pendingProperty.Name) is null)
            {
                mismatches.Add($"{pendingProperty.Name}: {sourceType.Name} に存在しない");
            }
        }

        return mismatches;
    }

    /// <summary>
    /// 対応元エンティティの業務プロパティ一覧を取得する。
    /// BaseEntity 由来の Id/CreatedAt/UpdatedAt（全エンティティ共通の監査列）と、
    /// ナビゲーションプロパティ（列ではなくエンティティ参照）は対象外とする。
    /// </summary>
    private static IEnumerable<PropertyInfo> GetBusinessColumns(Type sourceType, string[] excludedPropertyNames)
    {
        return sourceType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.DeclaringType != typeof(BaseEntity))
            .Where(p => !IsNavigationProperty(p))
            .Where(p => !excludedPropertyNames.Contains(p.Name));
    }

    /// <summary>
    /// プロパティがナビゲーションプロパティ（コレクション参照またはエンティティ参照）かどうかを判定する。
    /// </summary>
    /// <remarks>
    /// 固定の型リストでスカラー型を許可する方式（旧 IsScalarColumnType）は、Guid/TimeOnly/TimeSpan/
    /// DateTimeOffset/byte[] のような未知のスカラー型を将来 Contract 等へ追加したときに、
    /// ナビゲーションプロパティと誤認して静かに比較対象から除外してしまう。この方法では逆に
    /// 除外対象（ナビゲーションプロパティ）だけを明示的に判定し、それ以外はすべて業務列とみなす。
    /// </remarks>
    private static bool IsNavigationProperty(PropertyInfo property)
    {
        var type = property.PropertyType;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>))
        {
            return true;
        }

        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        return underlyingType.Namespace is not null
            && underlyingType.Namespace.StartsWith("UnifiedAccount.Domain.Entities", StringComparison.Ordinal);
    }

    /// <summary>
    /// 型（null許容を含む）が一致しているかを比較し、不一致があれば mismatches に追加する。
    /// エンティティ名をラベルに含め、Contract/ContractApplyPending と TransferAmount/AmountApplyPending の
    /// どちらの組み合わせで検出された不一致かを判別できるようにする。
    /// </summary>
    private static void CompareTypeSignature(
        PropertyInfo sourceProperty, PropertyInfo pendingProperty, List<string> mismatches)
    {
        var sourceLabel = sourceProperty.DeclaringType!.Name;
        var pendingLabel = pendingProperty.DeclaringType!.Name;

        if (sourceProperty.PropertyType != pendingProperty.PropertyType)
        {
            mismatches.Add(
                $"{sourceProperty.Name}: 型が不一致 ({sourceLabel}={sourceProperty.PropertyType}, " +
                $"{pendingLabel}={pendingProperty.PropertyType})");
            return;
        }

        var context = new NullabilityInfoContext();
        var sourceNullable = context.Create(sourceProperty).ReadState == NullabilityState.Nullable;
        var pendingNullable = context.Create(pendingProperty).ReadState == NullabilityState.Nullable;

        if (sourceNullable != pendingNullable)
        {
            mismatches.Add(
                $"{sourceProperty.Name}: null許容が不一致 ({sourceLabel}={sourceNullable}, " +
                $"{pendingLabel}={pendingNullable})");
        }
    }

    /// <summary>
    /// MaxLength 属性の値が一致しているかを比較し、不一致があれば mismatches に追加する。
    /// </summary>
    private static void CompareMaxLength(
        PropertyInfo sourceProperty, PropertyInfo pendingProperty, List<string> mismatches)
    {
        var sourceLabel = sourceProperty.DeclaringType!.Name;
        var pendingLabel = pendingProperty.DeclaringType!.Name;
        var sourceMaxLength = sourceProperty.GetCustomAttribute<MaxLengthAttribute>()?.Length;
        var pendingMaxLength = pendingProperty.GetCustomAttribute<MaxLengthAttribute>()?.Length;

        if (sourceMaxLength != pendingMaxLength)
        {
            mismatches.Add(
                $"{sourceProperty.Name}: MaxLength が不一致 ({sourceLabel}={Describe(sourceMaxLength)}, " +
                $"{pendingLabel}={Describe(pendingMaxLength)})");
        }
    }

    /// <summary>
    /// Precision 属性の精度・スケールが一致しているかを比較し、不一致があれば mismatches に追加する。
    /// </summary>
    private static void ComparePrecision(
        PropertyInfo sourceProperty, PropertyInfo pendingProperty, List<string> mismatches)
    {
        var sourceLabel = sourceProperty.DeclaringType!.Name;
        var pendingLabel = pendingProperty.DeclaringType!.Name;
        var sourcePrecision = sourceProperty.GetCustomAttribute<PrecisionAttribute>();
        var pendingPrecision = pendingProperty.GetCustomAttribute<PrecisionAttribute>();

        var sourceValue = sourcePrecision is null ? null : $"({sourcePrecision.Precision},{sourcePrecision.Scale})";
        var pendingValue = pendingPrecision is null ? null : $"({pendingPrecision.Precision},{pendingPrecision.Scale})";

        if (sourceValue != pendingValue)
        {
            mismatches.Add(
                $"{sourceProperty.Name}: Precision が不一致 ({sourceLabel}={Describe(sourceValue)}, " +
                $"{pendingLabel}={Describe(pendingValue)})");
        }
    }

    private static string Describe(object? value) => value?.ToString() ?? "なし";

    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public void AmountApplyPending_ShouldHaveDefaultValues()
    {
        var pending = new AmountApplyPending();

        pending.Operation.Should().Be(string.Empty);
        pending.TransferAmountId.Should().BeNull();
        pending.SourceModificationId.Should().BeNull();
        pending.CompanyCode.Should().Be(string.Empty);
        pending.BatchNo.Should().Be(string.Empty);
        pending.PersonalCode.Should().Be(string.Empty);
        pending.ValidationFlags.Should().Be("000000000000000");
        pending.IsApplicable.Should().BeTrue();
        pending.ErrorFlag1.Should().Be("0");
        pending.ErrorFlag12.Should().Be("0");
    }

    /// <summary>このテストでは テーブル名が TD_AmountApplyPending である。</summary>
    [Fact]
    public void AmountApplyPending_TableName_ShouldBeTdAmountApplyPending()
    {
        var attr = typeof(AmountApplyPending)
            .GetCustomAttributes(typeof(TableAttribute), false)
            .Single() as TableAttribute;

        attr!.Name.Should().Be("TD_AmountApplyPending");
    }

    /// <summary>このテストでは 反映元を区別する列を持たない。金額の反映元は 1 系統であるため。</summary>
    [Fact]
    public void AmountApplyPending_ShouldNotHaveSource()
    {
        typeof(AmountApplyPending).GetProperty("Source").Should().BeNull();
    }
}
