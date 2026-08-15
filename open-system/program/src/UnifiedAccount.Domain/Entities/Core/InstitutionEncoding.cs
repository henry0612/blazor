using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Core;

/// <summary>
/// 金融機関別エンコーディング設定
/// 口座振替データ・結果データ・照合データの文字コードを金融機関ごとに管理する。
/// 
/// ■ 対応エンコーディング:
///   "shift_jis" ? JIS X 0201 / Shift_JIS互換 (デフォルト)
///   "ibm930"    ? IBM EBCDIC (日本語カタカナ拡張)
///   "ibm939"    ? IBM EBCDIC (日本語英小文字拡張)
/// 
/// ■ 用途:
///   メインフレーム運用が残る金融機関とのEBCDIC連携、
///   およびオープン系に移行済み金融機関とのShift_JIS連携を共存させる。
/// </summary>
[Table("TM_InstitutionEncodings")]
[Index(nameof(BankCode), IsUnique = true)]
public class InstitutionEncoding : BaseEntity
{
    /// <summary>
    /// 金融機関コード (4桁)
    /// </summary>
    [Required, MaxLength(4)]
    public string BankCode { get; set; } = string.Empty;

    /// <summary>
    /// 金融機関名 (表示用)
    /// </summary>
    [MaxLength(30)]
    public string? BankName { get; set; }

    /// <summary>
    /// 口座振替データ用エンコーディング名
    /// (例: "shift_jis", "ibm930")
    /// </summary>
    [Required, MaxLength(20)]
    public string TransferDataEncoding { get; set; } = "shift_jis";

    /// <summary>
    /// 口座振替結果データ用エンコーディング名
    /// (例: "shift_jis", "ibm930")
    /// </summary>
    [Required, MaxLength(20)]
    public string ResultDataEncoding { get; set; } = "shift_jis";

    /// <summary>
    /// 照合データ用エンコーディング名
    /// (例: "shift_jis", "ibm930")
    /// </summary>
    [Required, MaxLength(20)]
    public string ReconcileDataEncoding { get; set; } = "shift_jis";

    /// <summary>
    /// 備考
    /// </summary>
    [MaxLength(200)]
    public string? Remarks { get; set; }
}
