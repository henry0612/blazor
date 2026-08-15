using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 送付状データ (KOZXSO)
/// </summary>
[Table("TR_CoverLetters")]
public class CoverLetter : BaseEntity
{
    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    [MaxLength(36)]
    /// <summary>
    /// 会社名称を取得または設定する。
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// 処理日付を取得または設定する。
    /// </summary>
    public DateOnly ProcessDate { get; set; }
    /// <summary>
    /// 内容テキストを取得または設定する。
    /// </summary>
    public string? ContentText { get; set; }
}


