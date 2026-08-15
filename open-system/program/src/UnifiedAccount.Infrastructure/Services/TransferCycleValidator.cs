using UnifiedAccount.Domain.Enums;

namespace UnifiedAccount.Infrastructure.Services;

/// <summary>
/// 振替回サイクル検証サービス (KOZ045 SHORIKU-CHECK 相当)
/// 現行JCL/COBOLの JOB1/JOB2/JOB3 実行順序ルールを判定する。
/// </summary>
public class TransferCycleValidator
{
    /// <summary>
    /// 振替回と前回振替回の組み合わせが有効かを判定する。
    /// </summary>
    /// <param name="transferRound">振替回次を指定する。</param>
    /// <param name="previousTransferRound">前回振替回次を指定する。</param>
    public bool IsValid(TransferRound transferRound, string? previousTransferRound)
    {
        EnsureRoundDefined(transferRound);

        var previousRound = NormalizePreviousRound(previousTransferRound);

        return transferRound switch
        {
            TransferRound.Round1 => true,
            TransferRound.Round2 => previousRound is "0" or "3",
            TransferRound.Round3 => previousRound == "2",
            _ => throw new ArgumentOutOfRangeException(nameof(transferRound), transferRound, "不正な振替回です。")
        };
    }

    /// <summary>
    /// 振替回の実行可否を検証し、不正な順序の場合は例外を送出する。
    /// </summary>
    /// <param name="transferRound">振替回次を指定する。</param>
    /// <param name="previousTransferRound">前回振替回次を指定する。</param>
    /// <param name="companyCode">会社コードを指定する。</param>
    public void EnsureValid(TransferRound transferRound, string? previousTransferRound, string? companyCode = null)
    {
        EnsureRoundDefined(transferRound);

        if (IsValid(transferRound, previousTransferRound))
            return;

        var company = string.IsNullOrWhiteSpace(companyCode) ? "(会社コード不明)" : companyCode;
        var previousRound = NormalizePreviousRound(previousTransferRound);

        throw new InvalidOperationException(
            $"振替回の順序が不正です。Company={company}, Round={(int)transferRound}, Previous={previousRound}");
    }

    private static void EnsureRoundDefined(TransferRound transferRound)
    {
        if (!Enum.IsDefined(transferRound))
            throw new ArgumentOutOfRangeException(nameof(transferRound), transferRound, "不正な振替回です。1(JOB1), 2(JOB2), 3(JOB3) を指定してください。");
    }

    private static string NormalizePreviousRound(string? previousTransferRound)
    {
        return string.IsNullOrWhiteSpace(previousTransferRound)
            ? "0"
            : previousTransferRound.Trim();
    }
}


