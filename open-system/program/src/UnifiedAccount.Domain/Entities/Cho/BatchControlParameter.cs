using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Cho;

/// <summary>
/// バッチ制御パラメータ (CHOXCT)
/// </summary>
[Table("TM_BatchControlParameters", Schema = "cho")]
[Index(nameof(ParameterKey), IsUnique = true)]
public class BatchControlParameter : BaseEntity
{
    [Required, MaxLength(50)]
    public string ParameterKey { get; set; } = string.Empty;

    /// <summary>
    /// 処理日付を取得または設定する。
    /// </summary>
    public DateOnly ProcessingDate { get; set; }
    /// <summary>
    /// 振替日付を取得または設定する。
    /// </summary>
    public DateOnly TransferDate { get; set; }
}


