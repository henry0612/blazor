using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Apply;

/// <summary>
/// 金額データ確定前差分 (新規)
/// </summary>
/// <remarks>
/// 現行 KD065 に相当する。適用フェーズが変更行のみを生成し、確定フェーズが
/// TD_TransferAmounts へ MERGE する。金額データは物理削除を持つため
/// Operation に DELETE を含む。
/// </remarks>
[Table("TD_AmountApplyPending")]
[Index(nameof(ApplyRunId), nameof(CompanyCode), nameof(BatchNo), nameof(PersonalCode))]
public class AmountApplyPending : BaseEntity
{
    /// <summary>
    /// 適用試行IDを取得または設定する。
    /// </summary>
    public long ApplyRunId { get; set; }

    /// <summary>
    /// 適用試行を取得または設定する。
    /// </summary>
    public ApplyRun? ApplyRun { get; set; }

    /// <summary>
    /// 操作を取得または設定する。INSERT / UPDATE / DELETE のいずれか。
    /// </summary>
    [Required, MaxLength(8)]
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// 金額データIDを取得または設定する。UPDATE および DELETE 時に設定する。
    /// </summary>
    public long? TransferAmountId { get; set; }

    /// <summary>
    /// 金額修正IDを取得または設定する。
    /// </summary>
    public long? SourceModificationId { get; set; }

    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    /// <summary>
    /// バッチ番号を取得または設定する。
    /// </summary>
    [Required, MaxLength(3)]
    public string BatchNo { get; set; } = string.Empty;

    [Required, MaxLength(12)]
    public string PersonalCode { get; set; } = string.Empty;

    /// <summary>
    /// 検証結果フラグを取得または設定する。この試行での検証結果を保持する。
    /// </summary>
    /// <remarks>
    /// TD_TransferAmounts 由来の業務列 ErrorFlag1 から ErrorFlag12 とは別物である。
    /// </remarks>
    [Required, MaxLength(15)]
    public string ValidationFlags { get; set; } = "000000000000000";

    /// <summary>
    /// 適用可否を取得または設定する。
    /// </summary>
    public bool IsApplicable { get; set; } = true;

    // ---- 以下、TD_TransferAmounts の業務列（TransferAmount.cs と同名同型）----

    /// <summary>契約IDを取得または設定する。</summary>
    public long? ContractId { get; set; }

    /// <summary>伝票区分を取得または設定する。</summary>
    [Required, MaxLength(2)]
    public string RecordType { get; set; } = string.Empty;

    /// <summary>順序番号を取得または設定する。</summary>
    [Required]
    public int SequenceNo { get; set; }

    /// <summary>チェックデジットを取得または設定する。</summary>
    [Required, MaxLength(1)]
    public string CheckDigit { get; set; } = string.Empty;

    [Precision(10, 0)] public decimal? Amount1 { get; set; }
    [Precision(10, 0)] public decimal? Amount2 { get; set; }
    [Precision(10, 0)] public decimal? Amount3 { get; set; }
    [Precision(10, 0)] public decimal? Amount4 { get; set; }
    [Precision(10, 0)] public decimal? Amount5 { get; set; }

    [Required, MaxLength(1)] public string ErrorFlag1 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag2 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag3 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag4 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag5 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag6 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag7 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag8 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag9 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag10 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag11 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag12 { get; set; } = "0";
}
