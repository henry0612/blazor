using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;
using UnifiedAccount.Domain.Enums;

namespace UnifiedAccount.Domain.Entities.Core;

/// <summary>
/// オンラインユーザーマスター (F-INF-001)
/// </summary>
[Table("TM_Users")]
[Index(nameof(UserId), IsUnique = true)]
public class User : BaseEntity
{
    /// <summary>
    /// ユーザーID (ログイン識別子)
    /// </summary>
    [Required, MaxLength(20)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// パスワードハッシュ (PBKDF2-SHA256)
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 表示名
    /// </summary>
    [Required, MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// ロール
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Operator;

    /// <summary>
    /// 有効フラグ
    /// </summary>
    public bool IsActive { get; set; } = true;
}

