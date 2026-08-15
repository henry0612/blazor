using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Interfaces;
using UnifiedAccount.Domain.ValueObjects;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Infrastructure.Services;

/// <summary>
/// コード変換テーブル参照 (KSYMLC 相当)
/// コード設定マスタ (TM_CodeSettings) を読み込み、区分単位でキャッシュする。
/// </summary>
public class CodeLookupService : ICodeLookupService
{
    /// <summary>
    /// 区分マスタ自体を表す CodeCategory。TM_CodeSettings の二階層構造の親行を指す。
    /// </summary>
    public const string CategoryMasterKey = "00";

    private readonly AppDbContext _context;
    private readonly CodeLookupCache _cache;

    public CodeLookupService(AppDbContext context, CodeLookupCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<IReadOnlyList<CodeCategoryItem>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var cached = _cache.GetCategories();
        if (cached is not null)
        {
            return cached;
        }

        var categories = await _context.CodeSettings
            .AsNoTracking()
            .Where(row => row.CodeCategory == CategoryMasterKey)
            .OrderBy(row => row.DisplayOrder)
            .ThenBy(row => row.CodeValue)
            .Select(row => new CodeCategoryItem(row.CodeValue, row.DisplayText, row.DisplayOrder))
            .ToListAsync(ct);

        _cache.SetCategories(categories);
        return categories;
    }

    public async Task<IReadOnlyList<CodeItem>> GetCodesAsync(string category, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        var cached = _cache.GetCodes(category);
        if (cached is not null)
        {
            return cached;
        }

        var codes = await _context.CodeSettings
            .AsNoTracking()
            .Where(row => row.CodeCategory == category)
            .OrderBy(row => row.DisplayOrder)
            .ThenBy(row => row.CodeValue)
            .Select(row => new CodeItem(row.CodeValue, row.DisplayText, row.ChangeValue, row.DisplayOrder))
            .ToListAsync(ct);

        // 該当0件もキャッシュする。マスタ更新後は Invalidate で明示的に破棄する。
        _cache.SetCodes(category, codes);
        return codes;
    }

    public async Task<string?> LookupAsync(string category, string code, CancellationToken ct = default)
    {
        var codes = await GetCodesAsync(category, ct);
        return codes.FirstOrDefault(item => string.Equals(item.CodeValue, code, StringComparison.Ordinal))?.DisplayText;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetAllCodesAsync(
        string category,
        CancellationToken ct = default)
    {
        var codes = await GetCodesAsync(category, ct);
        return codes.ToDictionary(item => item.CodeValue, item => item.DisplayText, StringComparer.Ordinal);
    }

    public void Invalidate()
    {
        _cache.Clear();
    }

    /// <summary>
    /// カテゴリごとのコードをキャッシュへ直接ロードする。
    /// コード設定マスタを介さない一時的な上書き用。表示順は与えられた並び順とする。
    /// </summary>
    /// <param name="category">区分を指定する。</param>
    /// <param name="codes">コード値と表示名の対応を指定する。</param>
    public void LoadCodes(string category, Dictionary<string, string> codes)
    {
        var items = codes
            .Select((pair, index) => new CodeItem(pair.Key, pair.Value, null, index + 1))
            .ToList();

        _cache.SetCodes(category, items);
    }
}
