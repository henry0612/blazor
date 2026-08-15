using UnifiedAccount.Application.DTOs;

namespace UnifiedAccount.Application.Services;

/// <summary>
/// 銀行支店マスタ サービスインターフェース
/// </summary>
public interface IBankBranchService
{
    /// <summary>
    /// BankCode・BranchCode の部分一致（AND）で検索し、ページング済み結果を返す。
    /// 両方未指定の場合は全件検索。BankCode・BranchCode 順に並べ、1ページ 50 件。
    /// </summary>
    Task<PagedResult<BankBranchRow>> SearchAsync(
        string? bankCode,
        string? branchCode,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// BankCode + BranchCode の論理キーで1件取得する。存在しない場合は null を返す。
    /// </summary>
    Task<BankBranchDto?> GetByKeyAsync(
        string bankCode,
        string branchCode,
        CancellationToken ct = default);

    /// <summary>
    /// 銀行支店を新規登録する。重複・検証エラー・DB例外を判別可能型で返す。
    /// </summary>
    Task<CreateResult> CreateAsync(
        CreateBankBranchRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// 銀行支店を更新する。OriginalUpdatedAt による楽観ロックを行う。
    /// 対象不存在・競合・DB例外を判別可能型で返す。
    /// </summary>
    Task<UpdateResult> UpdateAsync(
        string bankCode,
        string branchCode,
        UpdateBankBranchRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// 銀行支店を削除する。OriginalUpdatedAt による楽観ロックを行う。
    /// 対象不存在・競合・DB例外を判別可能型で返す。
    /// </summary>
    Task<DeleteResult> DeleteAsync(
        string bankCode,
        string branchCode,
        DeleteBankBranchRequest request,
        CancellationToken ct = default);
}
