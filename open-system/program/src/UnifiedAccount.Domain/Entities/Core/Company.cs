using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;
using UnifiedAccount.Domain.Entities.Changes;

namespace UnifiedAccount.Domain.Entities.Core;

/// <summary>
/// 委託会社マスター (KOZXCM+KOZXCZ+KOZXCF+KOZXCK)
/// </summary>
[Table("TM_Companies")]
[Index(nameof(CompanyCode), IsUnique = true)]
public class Company : BaseEntity
{
    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    [Required, MaxLength(36)]
    public string CompanyNameKana { get; set; } = string.Empty;

    [MaxLength(30)]
    /// <summary>
    /// 会社名称漢字を取得または設定する。
    /// </summary>
    public string? CompanyNameKanji { get; set; }

    [MaxLength(7)]
    /// <summary>
    /// 郵便コードを取得または設定する。
    /// </summary>
    public string? PostalCode { get; set; }

    [MaxLength(9)]
    /// <summary>
    /// 都道府県を取得または設定する。
    /// </summary>
    public string? Prefecture { get; set; }

    [MaxLength(20)]
    /// <summary>
    /// 市区町村を取得または設定する。
    /// </summary>
    public string? City { get; set; }

    [MaxLength(24)]
    /// <summary>
    /// 町域1を取得または設定する。
    /// </summary>
    public string? Town1 { get; set; }

    [MaxLength(24)]
    /// <summary>
    /// 町域2を取得または設定する。
    /// </summary>
    public string? Town2 { get; set; }

    [MaxLength(13)]
    /// <summary>
    /// 電話番号を取得または設定する。
    /// </summary>
    public string? PhoneNumber { get; set; }

    [MaxLength(13)]
    /// <summary>
    /// FAX番号を取得または設定する。
    /// </summary>
    public string? FaxNumber { get; set; }

    [Required, MaxLength(1)]
    public string ZenginLinkFlag { get; set; } = "0";

    [MaxLength(1)] public string ProcessingFlag1 { get; set; } = "0";
    [MaxLength(1)] public string ProcessingFlag2 { get; set; } = "0";
    [MaxLength(1)] public string ProcessingFlag3 { get; set; } = "0";
    [MaxLength(1)] public string ProcessingFlag4 { get; set; } = "0";
    [MaxLength(1)] public string ProcessingFlag5 { get; set; } = "0";
    [MaxLength(1)] public string ProcessingFlag6 { get; set; } = "0";
    [MaxLength(1)] public string ProcessingFlag7 { get; set; } = "0";
    [MaxLength(1)] public string ProcessingFlag8 { get; set; } = "0";
    [MaxLength(1)] public string ProcessingFlag9 { get; set; } = "0";
    [MaxLength(1)] public string ProcessingFlag10 { get; set; } = "0";
    [MaxLength(1)] public string ProcessingFlag11 { get; set; } = "0";
    [MaxLength(1)] public string ProcessingFlag12 { get; set; } = "0";
    [MaxLength(1)] public string SheetFlag1 { get; set; } = "0";
    [MaxLength(1)] public string SheetFlag2 { get; set; } = "0";
    [MaxLength(1)] public string SheetFlag3 { get; set; } = "0";
    [MaxLength(1)] public string SheetFlag4 { get; set; } = "0";
    [MaxLength(1)] public string SheetFlag5 { get; set; } = "0";
    [MaxLength(1)] public string SheetFlag6 { get; set; } = "0";
    [MaxLength(1)] public string SheetFlag7 { get; set; } = "0";
    [MaxLength(1)] public string SheetFlag8 { get; set; } = "0";
    [MaxLength(1)] public string SheetFlag9 { get; set; } = "0";
    [MaxLength(1)] public string SheetFlag10 { get; set; } = "0";
    [MaxLength(1)] public string SheetFlag11 { get; set; } = "0";
    [MaxLength(1)] public string SheetFlag12 { get; set; } = "0";

    /// <summary>
    /// ページ変更キーを取得または設定する。
    /// </summary>
    public short PageChangeKey { get; set; } = 0;

    [Precision(10, 0)]
    /// <summary>
    /// 基本手数料を取得または設定する。
    /// </summary>
    public decimal BasicFee { get; set; } = 0;

    [Precision(10, 0)]
    /// <summary>
    /// 管理手数料を取得または設定する。
    /// </summary>
    public decimal AdminFee { get; set; } = 0;

    [Precision(5, 0)]
    /// <summary>
    /// 新規単位単価を取得または設定する。
    /// </summary>
    public decimal NewUnitPrice { get; set; } = 0;

    [Precision(5, 0)]
    /// <summary>
    /// 変更単位単価を取得または設定する。
    /// </summary>
    public decimal ModifyUnitPrice { get; set; } = 0;

    [Precision(5, 0)]
    /// <summary>
    /// 振替1回目単位単価を取得または設定する。
    /// </summary>
    public decimal Transfer1UnitPrice { get; set; } = 0;

    [Precision(5, 0)]
    /// <summary>
    /// 振替2回目単位単価を取得または設定する。
    /// </summary>
    public decimal Transfer2UnitPrice { get; set; } = 0;

    [Precision(5, 0)]
    /// <summary>
    /// 受領単位単価を取得または設定する。
    /// </summary>
    public decimal ReceiptUnitPrice { get; set; } = 0;

    [MaxLength(4)]
    /// <summary>
    /// 振替1回目銀行コードを取得または設定する。
    /// </summary>
    public string? Transfer1BankCode { get; set; }

    [MaxLength(3)]
    /// <summary>
    /// 振替1回目支店コードを取得または設定する。
    /// </summary>
    public string? Transfer1BranchCode { get; set; }

    [MaxLength(1)]
    /// <summary>
    /// 振替1回目口座種別を取得または設定する。
    /// </summary>
    public string? Transfer1AccountType { get; set; }

    [MaxLength(10)]
    /// <summary>
    /// 振替1回目口座番号を取得または設定する。
    /// </summary>
    public string? Transfer1AccountNo { get; set; }

    [Required, MaxLength(10)]
    public string ConsignorCode { get; set; } = string.Empty;

    [MaxLength(4)]
    /// <summary>
    /// 振替2回目銀行コードを取得または設定する。
    /// </summary>
    public string? Transfer2BankCode { get; set; }

    [MaxLength(3)]
    /// <summary>
    /// 振替2回目支店コードを取得または設定する。
    /// </summary>
    public string? Transfer2BranchCode { get; set; }

    [MaxLength(1)]
    /// <summary>
    /// 振替2回目口座種別を取得または設定する。
    /// </summary>
    public string? Transfer2AccountType { get; set; }

    [MaxLength(10)]
    /// <summary>
    /// 振替2回目口座番号を取得または設定する。
    /// </summary>
    public string? Transfer2AccountNo { get; set; }

    [MaxLength(8)]
    /// <summary>
    /// 通帳コメントを取得または設定する。
    /// </summary>
    public string? PassbookComment { get; set; }

    [MaxLength(6)]
    /// <summary>
    /// 振替コードを取得または設定する。
    /// </summary>
    public string? TransferCode { get; set; }

    /// <summary>
    /// 前回振替日付を取得または設定する。
    /// </summary>
    public DateOnly? LastTransferDate { get; set; }

    [MaxLength(10)]
    /// <summary>
    /// 契約順序番号を取得または設定する。
    /// </summary>
    public string? ContractSeqNo { get; set; }

    [MaxLength(4)]
    /// <summary>
    /// 顧客請求番号を取得または設定する。
    /// </summary>
    public string? CustomerChargeNo { get; set; }

    [MaxLength(1)]
    /// <summary>
    /// 顧客請求副次種別を取得または設定する。
    /// </summary>
    public string? CustomerChargeSubType { get; set; }

    [MaxLength(30)]
    /// <summary>
    /// 口座名義人名称を取得または設定する。
    /// </summary>
    public string? AccountHolderName { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 新規件数を取得または設定する。
    /// </summary>
    public decimal NewCount { get; set; } = 0;

    [Precision(10, 0)]
    /// <summary>
    /// 変更件数を取得または設定する。
    /// </summary>
    public decimal ModifyCount { get; set; } = 0;

    [Precision(10, 0)]
    /// <summary>
    /// 振替1回目件数を取得または設定する。
    /// </summary>
    public decimal Transfer1Count { get; set; } = 0;

    [Precision(10, 0)]
    /// <summary>
    /// 振替2回目件数を取得または設定する。
    /// </summary>
    public decimal Transfer2Count { get; set; } = 0;

    [Precision(10, 0)]
    /// <summary>
    /// 失敗件数を取得または設定する。
    /// </summary>
    public decimal FailureCount { get; set; } = 0;

    [MaxLength(100)]
    /// <summary>
    /// コンビニパラメータを取得または設定する。
    /// </summary>
    public string? ConvenienceParams { get; set; }

    [MaxLength(100)]
    /// <summary>
    /// 郵便振替パラメータを取得または設定する。
    /// </summary>
    public string? PostalTransferParams { get; set; }

    [MaxLength(200)]
    /// <summary>
    /// 共済送金パラメータを取得または設定する。
    /// </summary>
    public string? CoopRemitParams { get; set; }

    [MaxLength(100)]
    /// <summary>
    /// 結果配信パラメータを取得または設定する。
    /// </summary>
    public string? ResultDeliveryParams { get; set; }

    /// <summary>
    /// 利用停止フラグ (0=有効, 1=停止)
    /// </summary>
    [MaxLength(1)]
    public string SuspendFlag { get; set; } = "0";

    /// <summary>
    /// 前回振替回 (COBOL: ACM-ZENKAI相当)
    /// "0"=未処理, "1"=JOB1済, "2"=JOB2済, "3"=JOB3済
    /// JOB2実行可否の判定に使用 (KOZ045 SHORIKU-CHECK)
    /// </summary>
    [MaxLength(1)]
    public string PreviousTransferRound { get; set; } = "0";

    /// <summary>
    /// 今回振替回 (COBOL: ACM-KONKAI相当)
    /// 実行中のジョブ回次を記録する。処理完了後に PreviousTransferRound へ引き継ぐ。
    /// </summary>
    [MaxLength(1)]
    public string CurrentTransferRound { get; set; } = "0";

    /// <summary>
    /// 都道府県（漢字）を取得または設定する。
    /// </summary>
    [MaxLength(9)]
    public string? PrefectureKanji { get; set; }

    /// <summary>
    /// 市区町村（漢字）を取得または設定する。
    /// </summary>
    [MaxLength(9)]
    public string? CityKanji { get; set; }

    /// <summary>
    /// 町名1（漢字）を取得または設定する。
    /// </summary>
    [MaxLength(9)]
    public string? TownKanji1 { get; set; }

    /// <summary>
    /// 町名2（漢字）を取得または設定する。
    /// </summary>
    [MaxLength(9)]
    public string? TownKanji2 { get; set; }

    /// <summary>
    /// 部署名（漢字）を取得または設定する。
    /// </summary>
    [MaxLength(9)]
    public string? DepartmentKanji { get; set; }

    /// <summary>
    /// 担当者名（漢字）を取得または設定する。
    /// </summary>
    [MaxLength(9)]
    public string? PersonInChargeKanji { get; set; }

    // Navigation properties
    public ICollection<CompanyWithdrawalDay> WithdrawalDays { get; set; } = [];
    public ICollection<CompanyType> Types { get; set; } = [];
    public ICollection<Contract> Contracts { get; set; } = [];
    public ICollection<TransferTransaction> TransferTransactions { get; set; } = [];
    public ICollection<NotificationMessage> NotificationMessages { get; set; } = [];
}
