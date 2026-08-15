using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 引落パラメータ会社 (OCCURS 12)
/// </summary>
[Table("TM_BatchParameterCompanies")]
public class BatchParameterCompany : BaseEntity
{
    /// <summary>
    /// バッチパラメータIDを取得または設定する。
    /// </summary>
    public long BatchParameterId { get; set; }
    public BatchParameter BatchParameter { get; set; } = null!;

    /// <summary>
    /// 枠番号を取得または設定する。
    /// </summary>
    public short SlotNo { get; set; }

    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;
}


