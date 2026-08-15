using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;
using UnifiedAccount.Domain.Entities.Core;

namespace UnifiedAccount.Domain.Entities.Changes;

/// <summary>
/// 契約者異動リクエスト (KOZXIK)
/// </summary>
[Table("TD_ContractChangeRequests")]
// SequenceNo は振替回（TransferDate）ごとに1から採番し直す方針のため、一意範囲に TransferDate を含める。
// 現行 KOZR05 が振替回ごとに ID030 から採番し直しているのと同じ意味論。
[Index(nameof(CompanyCode), nameof(PersonalCode), nameof(TransferDate), nameof(SequenceNo), IsUnique = true)]
public class ContractChangeRequest : BaseEntity
{
    /// <summary>
    /// 契約IDを取得または設定する。
    /// </summary>
    /// <remarks>
    /// 取込時または画面登録時に解決する。新規登録の異動データでは null とする。
    /// 外部取込の会社は C/D をチェックディジットとして使用しているため、
    /// 会社コード + 個人コード で一意に解決できる。
    /// </remarks>
    public long? ContractId { get; set; }

    /// <summary>
    /// 契約を取得または設定する。
    /// </summary>
    public Contract? Contract { get; set; }

    [Required, MaxLength(2)]
    public string RequestType { get; set; } = string.Empty;

    [MaxLength(1)] public string? ChangeAction { get; set; }

    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    [Required, MaxLength(12)]
    public string PersonalCode { get; set; } = string.Empty;

    /// <summary>
    /// 順序番号を取得または設定する。
    /// </summary>
    /// <remarks>
    /// 振替回（<see cref="TransferDate"/>）ごとに1から採番し直す。現行 KOZR05 が振替回ごとに
    /// ID030 から採番し直しているのと同じ意味論であり、振替回をまたいで通番にはしない。
    /// </remarks>
    public short SequenceNo { get; set; }

    /// <summary>
    /// 振替日を取得または設定する。
    /// </summary>
    /// <remarks>
    /// 適用フェーズの抽出条件に使う。その振替回に適用すべき異動データを識別する。
    /// </remarks>
    public DateOnly TransferDate { get; set; }

    [MaxLength(36)] public string? CompanyName { get; set; }
    /// <summary>
    /// メッセージ番号を取得または設定する。
    /// </summary>
    public short? MessageNo { get; set; }

    [MaxLength(32)] public string? ContractorName { get; set; }
    [MaxLength(32)] public string? DepositorName { get; set; }
    [MaxLength(4)] public string? BankCode { get; set; }
    [MaxLength(3)] public string? BranchCode { get; set; }
    [MaxLength(1)] public string? AccountType { get; set; }
    [MaxLength(10)] public string? AccountNo { get; set; }
    /// <summary>
    /// 引落日を取得または設定する。
    /// </summary>
    public short? WithdrawalDay { get; set; }
    [MaxLength(6)] public string? StartYearMonth { get; set; }
    [MaxLength(1)] public string? SuspendFlag { get; set; }
    [MaxLength(7)] public string? PostalCode { get; set; }
    [MaxLength(9)] public string? Prefecture { get; set; }
    [MaxLength(20)] public string? City { get; set; }
    [MaxLength(24)] public string? Town1 { get; set; }
    [MaxLength(24)] public string? Town2 { get; set; }
    [MaxLength(7)] public string? PhoneNumber { get; set; }
    [MaxLength(1)] public string? DischargeType { get; set; }
    /// <summary>
    /// 種別番号を取得または設定する。
    /// </summary>
    public short? TypeNo { get; set; }
    [Precision(10, 0)] public decimal? Amount { get; set; }
    [MaxLength(6)] public string? NextTransferYearMonth { get; set; }
    [MaxLength(10)] public string? CreditProductName { get; set; }
    [Precision(10, 0)] public decimal? CreditTotalAmount { get; set; }
    /// <summary>
    /// 入金合計件数を取得または設定する。
    /// </summary>
    public short? CreditTotalCount { get; set; }
    [Precision(10, 0)] public decimal? CreditPayment1 { get; set; }
    [Precision(10, 0)] public decimal? CreditPayment2 { get; set; }
    [Precision(10, 0)] public decimal? CreditSpecialAddition { get; set; }

    /// <summary>
    /// エラーフラグを取得または設定する。
    /// </summary>
    /// <remarks>
    /// 最新の適用試行における検証結果を保持する。試行のたびに上書きする。
    /// 適用済みの状態は本列に持たせない。適用実績は TD_ContractApplyPending と
    /// TD_ApplyRuns から追跡する。
    /// </remarks>
    [Required, MaxLength(15)]
    public string ErrorFlags { get; set; } = "000000000000000";

    [Required, MaxLength(50)]
    public string JobExecutionId { get; set; } = string.Empty;

    [Required, MaxLength(16)]
    public string BatchStatus { get; set; } = "SUCCESS";

    [MaxLength(20)]
    /// <summary>
    /// 失敗実行者プログラムを取得または設定する。
    /// </summary>
    public string? FailedByProgram { get; set; }

    [MaxLength(4)]
    /// <summary>
    /// 失敗理由コードを取得または設定する。
    /// </summary>
    public string? FailedReasonCode { get; set; }

    /// <summary>
    /// 失敗日時を取得または設定する。
    /// </summary>
    public DateTime? FailedAt { get; set; }

    public int Version { get; set; } = 1;
}
