using System.Collections.Concurrent;
using UnifiedAccount.Domain.ValueObjects;

namespace UnifiedAccount.Infrastructure.Services;

/// <summary>
/// コード参照のキャッシュ。
/// コード設定マスタは更新頻度が低く全画面・全バッチから参照されるため、
/// スコープをまたいで共有するシングルトンとして DI 登録する。
/// </summary>
public sealed class CodeLookupCache
{
    private readonly ConcurrentDictionary<string, IReadOnlyList<CodeItem>> _codes = new(StringComparer.Ordinal);

    private volatile IReadOnlyList<CodeCategoryItem>? _categories;

    /// <summary>
    /// 区分一覧を取得する。未キャッシュの場合は null を返す。
    /// </summary>
    public IReadOnlyList<CodeCategoryItem>? GetCategories()
    {
        return _categories;
    }

    /// <summary>
    /// 区分一覧をキャッシュへ格納する。
    /// </summary>
    public void SetCategories(IReadOnlyList<CodeCategoryItem> categories)
    {
        _categories = categories;
    }

    /// <summary>
    /// 指定区分のコード一覧を取得する。未キャッシュの場合は null を返す。
    /// </summary>
    public IReadOnlyList<CodeItem>? GetCodes(string category)
    {
        return _codes.TryGetValue(category, out var codes) ? codes : null;
    }

    /// <summary>
    /// 指定区分のコード一覧をキャッシュへ格納する。
    /// </summary>
    public void SetCodes(string category, IReadOnlyList<CodeItem> codes)
    {
        _codes[category] = codes;
    }

    /// <summary>
    /// キャッシュを全件破棄する。コード設定マスタ更新後に呼び出す。
    /// </summary>
    public void Clear()
    {
        _categories = null;
        _codes.Clear();
    }
}
