namespace UnifiedAccount.Domain.Common;

/// <summary>
/// 全エンティティの共通基底クラス
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// サロゲートキー (BIGINT IDENTITY)
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 登録日時
    /// </summary>
    public DateTime CreatedAt { get; set; } = LocalDateTimeProvider.Now;

    /// <summary>
    /// 更新日時
    /// </summary>
    public virtual DateTime UpdatedAt { get; set; } = LocalDateTimeProvider.Now;
}
