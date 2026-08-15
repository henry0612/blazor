using UnifiedAccount.Domain.ValueObjects;

namespace UnifiedAccount.Domain.Interfaces;

/// <summary>
/// コード変換テーブル参照 (KSYMLC相当)
/// コード設定マスタ (TM_CodeSettings) を参照元とする。
/// </summary>
public interface ICodeLookupService
{
    /// <summary>
    /// コード区分の一覧を表示順で取得する。
    /// </summary>
    Task<IReadOnlyList<CodeCategoryItem>> GetCategoriesAsync(CancellationToken ct = default);

    /// <summary>
    /// 指定した区分に属するコード値の一覧を表示順で取得する。該当なしの場合は空リストを返す。
    /// </summary>
    Task<IReadOnlyList<CodeItem>> GetCodesAsync(string category, CancellationToken ct = default);

    /// <summary>
    /// 区分とコード値から表示名を取得する。該当なしの場合は null を返す。
    /// </summary>
    Task<string?> LookupAsync(string category, string code, CancellationToken ct = default);

    /// <summary>
    /// 指定した区分のコード値と表示名の対応を取得する。表示順が必要な場合は <see cref="GetCodesAsync"/> を使用する。
    /// </summary>
    Task<IReadOnlyDictionary<string, string>> GetAllCodesAsync(string category, CancellationToken ct = default);

    /// <summary>
    /// キャッシュを破棄し、次回参照時にコード設定マスタから再読込させる。
    /// </summary>
    void Invalidate();
}
