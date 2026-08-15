using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Core;

[Table("TM_CodeSettings")]
[Index(nameof(CodeCategory), nameof(CodeValue), IsUnique = true)]
[Index(nameof(CodeCategory), nameof(DisplayOrder), nameof(CodeValue))]
public class CodeSetting : BaseEntity
{
    [ConcurrencyCheck]
    public override DateTime UpdatedAt
    {
        get => base.UpdatedAt;
        set => base.UpdatedAt = value;
    }

    [Required, MaxLength(20)]
    public string CodeCategory { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string CodeValue { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string DisplayText { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ChangeValue { get; set; }

    public int DisplayOrder { get; set; }
}
