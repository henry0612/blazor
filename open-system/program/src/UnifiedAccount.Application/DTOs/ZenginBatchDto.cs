namespace UnifiedAccount.Application.DTOs;

/// <summary>
/// 全銀バッチDTO
/// </summary>
public record ZenginBatchDto
{
    /// <summary>
    /// IDを取得または設定する。
    /// </summary>
    public long Id { get; init; }
    public string ConsignorCode { get; init; } = string.Empty;
    /// <summary>
    /// 引落日付を取得または設定する。
    /// </summary>
    public DateOnly WithdrawalDate { get; init; }
    /// <summary>
    /// 合計件数を取得または設定する。
    /// </summary>
    public int? TotalCount { get; init; }
    /// <summary>
    /// 合計金額を取得または設定する。
    /// </summary>
    public decimal? TotalAmount { get; init; }
}


