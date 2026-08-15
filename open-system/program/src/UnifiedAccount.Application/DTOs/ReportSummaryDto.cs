namespace UnifiedAccount.Application.DTOs;

/// <summary>
/// 帳票集計DTO
/// </summary>
public record ReportSummaryDto
{
    /// <summary>
    /// 期間開始を取得または設定する。
    /// </summary>
    public DateOnly PeriodStart { get; init; }
    /// <summary>
    /// 期間終了を取得または設定する。
    /// </summary>
    public DateOnly PeriodEnd { get; init; }
    /// <summary>
    /// 会社コードを取得または設定する。
    /// </summary>
    public string? CompanyCode { get; init; }
    /// <summary>
    /// 合計件数を取得または設定する。
    /// </summary>
    public int TotalCount { get; init; }
    /// <summary>
    /// 合計金額を取得または設定する。
    /// </summary>
    public decimal TotalAmount { get; init; }
    /// <summary>
    /// 成功件数を取得または設定する。
    /// </summary>
    public int SuccessCount { get; init; }
    /// <summary>
    /// 失敗件数を取得または設定する。
    /// </summary>
    public int FailureCount { get; init; }
}


