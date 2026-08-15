using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Core;

/// <summary>
/// 会社引落日 (CM-HIKIBI OCCURS 4)
/// </summary>
[Table("TM_CompanyWithdrawalDays")]
public class CompanyWithdrawalDay : BaseEntity
{
    /// <summary>
    /// 会社IDを取得または設定する。
    /// </summary>
    public long CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    /// <summary>
    /// 枠番号を取得または設定する。
    /// </summary>
    public short SlotNo { get; set; }

    [Required, MaxLength(2)]
    public string WithdrawalDay { get; set; } = "00";

    [Required, MaxLength(1)]
    public string PreviousProcessingType { get; set; } = "0";

    [Required, MaxLength(1)]
    public string CurrentProcessingType { get; set; } = "0";
}

