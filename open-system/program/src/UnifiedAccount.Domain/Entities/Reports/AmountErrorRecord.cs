using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 金額エラーレコード (F-REP-034用中間テーブル)
/// </summary>
[Table("TR_AmountErrorRecords")]
[Index(nameof(JobExecutionId), nameof(CompanyCode), nameof(BatchNo), nameof(SequenceNo))]
public class AmountErrorRecord : BaseEntity
{
    [Required, MaxLength(50)]
    public string JobExecutionId { get; set; } = string.Empty;

    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string BatchNo { get; set; } = string.Empty;

    [Required, MaxLength(12)]
    public string PersonalCode { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string CheckDigit { get; set; } = string.Empty;

    /// <summary>
    /// エラー種別: Item / Duplicate / Total
    /// </summary>
    [Required, MaxLength(20)]
    public string ErrorType { get; set; } = string.Empty;

    /// <summary>
    /// エラー会社コードを取得または設定する。
    /// </summary>
    public bool IsErrorCompanyCode { get; set; }
    /// <summary>
    /// エラー個人コードを取得または設定する。
    /// </summary>
    public bool IsErrorPersonalCode { get; set; }
    /// <summary>
    /// エラーチェック桁を取得または設定する。
    /// </summary>
    public bool IsErrorCheckDigit { get; set; }
    /// <summary>
    /// エラー金額1を取得または設定する。
    /// </summary>
    public bool IsErrorAmount1 { get; set; }
    /// <summary>
    /// エラー金額2を取得または設定する。
    /// </summary>
    public bool IsErrorAmount2 { get; set; }
    /// <summary>
    /// エラー金額3を取得または設定する。
    /// </summary>
    public bool IsErrorAmount3 { get; set; }
    /// <summary>
    /// エラー金額4を取得または設定する。
    /// </summary>
    public bool IsErrorAmount4 { get; set; }
    /// <summary>
    /// エラー金額5を取得または設定する。
    /// </summary>
    public bool IsErrorAmount5 { get; set; }

    [Precision(10, 0)] public decimal Amount1 { get; set; }
    [Precision(10, 0)] public decimal Amount2 { get; set; }
    [Precision(10, 0)] public decimal Amount3 { get; set; }
    [Precision(10, 0)] public decimal Amount4 { get; set; }
    [Precision(10, 0)] public decimal Amount5 { get; set; }

    /// <summary>
    /// グループ内連番
    /// </summary>
    public int SequenceNo { get; set; }
}


