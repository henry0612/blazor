namespace UnifiedAccount.Domain.ValueObjects;

/// <summary>
/// コード区分。コード設定マスタ (TM_CodeSettings) の CodeCategory = "00" の行に対応する。
/// </summary>
/// <param name="CategoryCode">区分コード。子行の CodeCategory として使用する。</param>
/// <param name="DisplayText">区分の表示名。</param>
/// <param name="DisplayOrder">区分一覧の表示順。</param>
public sealed record CodeCategoryItem(string CategoryCode, string DisplayText, int DisplayOrder);

/// <summary>
/// コード値。コード設定マスタ (TM_CodeSettings) の子行に対応する。
/// </summary>
/// <param name="CodeValue">区分内のコード値。</param>
/// <param name="DisplayText">画面・帳票表示用の名称。</param>
/// <param name="ChangeValue">変換後値。未設定の場合は null。</param>
/// <param name="DisplayOrder">区分内の表示順。</param>
public sealed record CodeItem(string CodeValue, string DisplayText, string? ChangeValue, int DisplayOrder);
