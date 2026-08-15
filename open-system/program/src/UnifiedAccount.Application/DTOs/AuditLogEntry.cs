namespace UnifiedAccount.Application.DTOs;

/// <summary>
/// 操作監査ログの登録情報を表す。
/// </summary>
public record AuditLogEntry
{
    /// <summary>発生日時を取得または設定する。</summary>
    public DateTimeOffset OccurredAt { get; init; }
    /// <summary>操作利用者識別子を取得または設定する。</summary>
    public string ActorId { get; init; } = string.Empty;
    /// <summary>機能識別子を取得または設定する。</summary>
    public string FeatureId { get; init; } = string.Empty;
    /// <summary>操作種別を取得または設定する。</summary>
    public string Action { get; init; } = string.Empty;
    /// <summary>対象種別を取得または設定する。</summary>
    public string TargetType { get; init; } = string.Empty;
    /// <summary>対象キーJSONを取得または設定する。</summary>
    public string TargetKey { get; init; } = string.Empty;
    /// <summary>操作結果を取得または設定する。</summary>
    public string Result { get; init; } = string.Empty;
    /// <summary>相関IDを取得または設定する。</summary>
    public string CorrelationId { get; init; } = string.Empty;
    /// <summary>変更前値JSONを取得または設定する。</summary>
    public string? BeforeValuesJson { get; init; }
    /// <summary>変更後値JSONを取得または設定する。</summary>
    public string? AfterValuesJson { get; init; }
    /// <summary>エラーコードを取得または設定する。</summary>
    public string? ErrorCode { get; init; }
}
