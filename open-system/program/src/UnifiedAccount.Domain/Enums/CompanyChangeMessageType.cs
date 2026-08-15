namespace UnifiedAccount.Domain.Enums;

/// <summary>
/// 会社マスタ異動メッセージ区分
/// </summary>
public enum CompanyChangeMessageType
{
    None = 0,
    /// <summary>
    /// 新規
    /// </summary>
    Add = 1,
    /// <summary>
    /// 削除
    /// </summary>
    Delete = 2,
    /// <summary>
    /// 修正前
    /// </summary>
    Update_Before = 3,
    /// <summary>
    /// 修正後
    /// </summary>
    Update_After = 4,
    /// <summary>
    /// ＊項目エラー
    /// </summary>
    Item_Error = 5,
    /// <summary>
    /// ＊ダブりエラー
    /// </summary>
    Duplicate_Error = 6,
    /// <summary>
    /// ＊新規不足
    /// </summary>
    Combination_Or_NotApplicable_Error = 7,
    /// <summary>
    /// ＊マスターあり
    /// </summary>
    Add_Already_Exists = 8,
    /// <summary>
    /// ＊マスターなし
    /// </summary>
    UpdateOrDelete_Not_Exists = 9
}

