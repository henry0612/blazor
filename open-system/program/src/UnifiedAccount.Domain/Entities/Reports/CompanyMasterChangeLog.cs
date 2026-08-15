using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 会社マスター異動ログ (F-REP-001 帳票出力元データ)
/// </summary>
[Table("TR_CompanyMasterChangeLogs")]
[Index(nameof(JobExecutionId), nameof(CompanyCode), nameof(RequestType), nameof(SequenceNo), IsUnique = true,
    Name = "UQ_CompanyMasterChangeLogs_Key")]
[Index(nameof(JobExecutionId), nameof(CompanyCode), nameof(RequestType),
    Name = "IX_CompanyMasterChangeLogs_BatchRun")]
/// <summary>
/// CompanyMasterChangeLog を表すクラス。
/// </summary>
public class CompanyMasterChangeLog
{
    /// <summary>
    /// IDを取得または設定する。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// バッチ実行相関ID (JOB-{jobId}-{yyyyMMdd}-{6桁})
    /// </summary>
    [Required, MaxLength(50)]
    public string JobExecutionId { get; set; } = string.Empty;

    /// <summary>
    /// 処理日
    /// </summary>
    public DateOnly ProcessingDate { get; set; }

    /// <summary>
    /// 会社コード
    /// </summary>
    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    /// <summary>
    /// 伝票区分 '11'〜'19'
    /// </summary>
    [Required, MaxLength(2), Column(TypeName = "char(2)")]
    public string RequestType { get; set; } = string.Empty;

    /// <summary>
    /// 異動区分 '1'=削除 / '2'=新規 / '3'=修正
    /// </summary>
    [Required, MaxLength(1)]
    public string ChangeAction { get; set; } = string.Empty;

    /// <summary>
    /// メッセージ区分 (MSGTAB X1: 1〜10)
    /// </summary>
    public byte MessageType { get; set; }

    /// <summary>
    /// エラー行フラグ
    /// </summary>
    public bool IsError { get; set; }

    /// <summary>
    /// 処理順連番
    /// </summary>
    public int SequenceNo { get; set; }

    // DENK 11: 会社名・連絡先
    [MaxLength(40)] public string? CompanyNameKana { get; set; }
    [MaxLength(13)] public string? PhoneNumber { get; set; }
    [MaxLength(7)] public string? PostalCode { get; set; }
    [MaxLength(9)] public string? Prefecture { get; set; }

    // DENK 12: 住所
    [MaxLength(20)] public string? City { get; set; }
    [MaxLength(24)] public string? Town1 { get; set; }
    [MaxLength(24)] public string? Town2 { get; set; }

    // DENK 13: 引落日・フラグ
    [MaxLength(2)] public string? WithdrawalDay1 { get; set; }
    [MaxLength(2)] public string? WithdrawalDay2 { get; set; }
    [MaxLength(2)] public string? WithdrawalDay3 { get; set; }
    [MaxLength(2)] public string? WithdrawalDay4 { get; set; }
    [MaxLength(12)] public string? ProcessingFlag { get; set; }
    [MaxLength(12)] public string? SheetFlag { get; set; }

    // DENK 14: 料金・単価
    [MaxLength(2)] public string? ChangeCode { get; set; }
    [Precision(10, 0)] public decimal? BasicFee { get; set; }
    [Precision(10, 0)] public decimal? AdminFee { get; set; }
    [Precision(5, 0)] public decimal? NewUnitPrice { get; set; }
    [Precision(5, 0)] public decimal? ModifyUnitPrice { get; set; }
    [Precision(5, 0)] public decimal? Transfer1UnitPrice { get; set; }
    [Precision(5, 0)] public decimal? Transfer2UnitPrice { get; set; }
    [Precision(5, 0)] public decimal? ReceiptUnitPrice { get; set; }

    // DENK 15: 振替口座・委託者
    [MaxLength(4)] public string? Transfer1BankCode { get; set; }
    [MaxLength(3)] public string? Transfer1BranchCode { get; set; }
    [MaxLength(1)] public string? Transfer1AccountType { get; set; }
    [MaxLength(10)] public string? Transfer1AccountNo { get; set; }
    [MaxLength(10)] public string? ConsignorCode { get; set; }
    [MaxLength(4)] public string? Transfer2BankCode { get; set; }
    [MaxLength(3)] public string? Transfer2BranchCode { get; set; }
    [MaxLength(1)] public string? Transfer2AccountType { get; set; }
    [MaxLength(10)] public string? Transfer2AccountNo { get; set; }
    [MaxLength(8)] public string? PassbookNo { get; set; }

    // DENK 16〜19: 収納・種目・金額
    [MaxLength(1)] public string? CollectionCode { get; set; }
    [MaxLength(2)] public string? Cycle { get; set; }
    [MaxLength(4)] public string? OperatingYear { get; set; }
    [MaxLength(2)] public string? OperatingMonth { get; set; }
    [MaxLength(12)] public string? ItemName { get; set; }
    [Precision(10, 0)] public decimal? Amount { get; set; }

    public DateTime CreatedAt { get; set; } = LocalDateTimeProvider.Now;

    public DateTime UpdatedAt { get; set; } = LocalDateTimeProvider.Now;
}
