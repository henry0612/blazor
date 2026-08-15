namespace UnifiedAccount.Application.DTOs;

// ─────────────────────────────────────────────
// Request 型
// ─────────────────────────────────────────────

/// <summary>
/// 新規登録リクエスト。ActorId・CorrelationIdは223 §6.4.1の監査記録に使用する。
/// </summary>
public record CreateBankBranchRequest(
    string BankCode,
    string BranchCode,
    string BankNameKana,
    string BranchNameKana,
    string? BankNameKanji,
    string? BranchNameKanji,
    string ActorId,
    string CorrelationId);

/// <summary>
/// 更新リクエスト。OriginalUpdatedAt で楽観ロックを行う。ActorId・CorrelationIdは223 §6.4.1の監査記録に使用する。
/// </summary>
public record UpdateBankBranchRequest(
    string BankNameKana,
    string BranchNameKana,
    string? BankNameKanji,
    string? BranchNameKanji,
    DateTime OriginalUpdatedAt,
    string ActorId,
    string CorrelationId);

/// <summary>
/// 削除リクエスト。OriginalUpdatedAt で楽観ロックを行う。ActorId・CorrelationIdは223 §6.4.1の監査記録に使用する。
/// </summary>
public record DeleteBankBranchRequest(
    DateTime OriginalUpdatedAt,
    string ActorId,
    string CorrelationId);

// ─────────────────────────────────────────────
// 判別可能 Result 型
// ─────────────────────────────────────────────

/// <summary>
/// 新規登録結果
/// </summary>
public abstract record CreateResult
{
    private CreateResult() { }

    //登録成功
    public sealed record Success(BankBranchDto Created) : CreateResult;

    //金融機関コード＋支店コードの重複
    public sealed record Duplicate : CreateResult;

    //入力検証エラー
    public sealed record ValidationError(IReadOnlyList<string> Errors) : CreateResult;

    //DB 例外
    public sealed record DbError(Exception Exception) : CreateResult;
}

/// <summary>
/// 更新結果
/// </summary>
public abstract record UpdateResult
{
    private UpdateResult() { }

    //更新成功
    public sealed record Success(BankBranchDto Updated) : UpdateResult;

    //対象レコードが存在しない
    public sealed record NotFound : UpdateResult;

    //楽観ロック競合（他ユーザーが更新済み）
    public sealed record Conflict : UpdateResult;

    //DB 例外
    public sealed record DbError(Exception Exception) : UpdateResult;
}

/// <summary>
/// 削除結果
/// </summary>
public abstract record DeleteResult
{
    private DeleteResult() { }

    //削除成功
    public sealed record Success : DeleteResult;

    //対象レコードが存在しない
    public sealed record NotFound : DeleteResult;

    //楽観ロック競合（他ユーザーが更新済み）
    public sealed record Conflict : DeleteResult;

    //DB 例外
    public sealed record DbError(Exception Exception) : DeleteResult;
}
