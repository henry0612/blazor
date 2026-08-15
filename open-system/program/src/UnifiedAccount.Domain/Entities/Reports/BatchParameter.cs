using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 引落パラメータ (KOZXPL)
/// </summary>
[Table("TM_BatchParameters")]
public class BatchParameter : BaseEntity
{
    [Required, MaxLength(1)]
    public string ParameterType { get; set; } = string.Empty;

    /// <summary>
    /// 引落日付を取得または設定する。
    /// </summary>
    public DateOnly WithdrawalDate { get; set; }

    /// <summary>
    /// ジョブ実行状態 (M9SYRCH/M9SYRCL/M9SYRUP 相当)
    /// 0=未実行, 1=実行中, 2=完了, 3=失敗
    /// </summary>
    [MaxLength(1)]
    public string JobExecutionStatus { get; set; } = "0";

    // Navigation
    public ICollection<BatchParameterCompany> Companies { get; set; } = [];
}


