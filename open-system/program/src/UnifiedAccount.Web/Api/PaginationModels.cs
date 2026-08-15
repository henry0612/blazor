namespace UnifiedAccount.Web.Api;

/// <summary>
/// ページネーションクエリ (PF7/PF8 → クエリパラメータ)
/// </summary>
public record PaginationQuery(int Page = 1, int PageSize = 20, string? Search = null);

/// <summary>
/// 銀行支店一覧検索条件。
/// </summary>
public record BankBranchListQuery(
    int Page = 1,
    int PageSize = 50,
    string? BankCode = null,
    string? BranchCode = null);

/// <summary>
/// 銀行支店一覧応答。
/// </summary>
public record BankBranchListResponse(
    IReadOnlyList<BankBranchRow> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasPrevious,
    bool HasNext);

/// <summary>
/// 銀行支店一覧行。
/// </summary>
public record BankBranchRow(
    long Id,
    string BankCode,
    string BranchCode,
    string BankNameKana,
    string BranchNameKana,
    string? BankNameKanji,
    string? BranchNameKanji,
    DateTime UpdatedAt);

/// <summary>
/// ページネーション結果
/// </summary>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    /// <summary>
    /// 総ページ数を取得する。
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}



