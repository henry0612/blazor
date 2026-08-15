using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Changes;

/// <summary>
/// メッセージ異動リクエスト (伝区72-CBAN 01-13)
/// </summary>
[Table("TD_MessageChangeRequests")]
[Index(nameof(RequestType), nameof(Cban))]
[Index(nameof(CompanyCode))]
[Index(nameof(JobExecutionId), nameof(BatchStatus))]
public class MessageChangeRequest : BaseEntity
{
    [Required, MaxLength(2)]
    public string RequestType { get; set; } = string.Empty;

    [Required, MaxLength(2)]
    public string Cban { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string ChangeAction { get; set; } = string.Empty;

    [MaxLength(6)]
    /// <summary>
    /// 会社コードを取得または設定する。
    /// </summary>
    public string? CompanyCode { get; set; }

    [MaxLength(1)]
    /// <summary>
    /// 摘要区分を取得または設定する。
    /// </summary>
    public string? TekiyoKubun { get; set; }

    /// <summary>
    /// 開始年を取得または設定する。
    /// </summary>
    public int? StartYear { get; set; }
    /// <summary>
    /// 開始月を取得または設定する。
    /// </summary>
    public int? StartMonth { get; set; }
    /// <summary>
    /// 開始日を取得または設定する。
    /// </summary>
    public int? StartDay { get; set; }
    /// <summary>
    /// 終了年を取得または設定する。
    /// </summary>
    public int? EndYear { get; set; }
    /// <summary>
    /// 終了月を取得または設定する。
    /// </summary>
    public int? EndMonth { get; set; }
    /// <summary>
    /// 終了日を取得または設定する。
    /// </summary>
    public int? EndDay { get; set; }

    [MaxLength(17)]
    /// <summary>
    /// 見出しを取得または設定する。
    /// </summary>
    public string? Midashi { get; set; }

    [MaxLength(24)]
    /// <summary>
    /// 見出し1を取得または設定する。
    /// </summary>
    public string? Midashi1 { get; set; }

    [MaxLength(24)]
    /// <summary>
    /// 見出し2を取得または設定する。
    /// </summary>
    public string? Midashi2 { get; set; }

    [MaxLength(2)]
    /// <summary>
    /// アビスコードを取得または設定する。
    /// </summary>
    public string? AvisCode { get; set; }

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


