using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Application.DTOs;
using UnifiedAccount.Application.Services;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Infrastructure.Services;

/// <summary>
/// 操作監査ログを `TD_AuditRecords` へ記録する共通プログラム。
/// 機能ID: F-INF-003。契約は 222 F-INF-003 操作監査ログ 共通プログラム定義書に従う。
/// </summary>
public sealed class AuditLogService : IAuditLogService
{
    private static readonly string[] AllowedActions = ["Search", "Save"];
    private static readonly string[] AllowedResults =
        ["Success", "ValidationError", "ConcurrencyConflict", "DependencyError", "Failure"];
    private static readonly string[] AllowedTargetKeyProperties =
        ["CompanyCode", "BatchNo", "SequenceNo"];
    private static readonly string[] AllowedCodeSettingTargetKeyProperties =
        ["CodeCategory", "CodeValue"];
    // 223 F-ONL-007 §6.4.1: BankCode→BranchCode の固定順で TargetKey を許可する。
    private static readonly string[] AllowedBankBranchTargetKeyProperties =
        ["BankCode", "BranchCode"];
    private static readonly string[] AllowedTransferAmountProperties =
        ["Amount1", "Amount2", "Amount3", "Amount4", "Amount5"];

    private readonly AppDbContext _db;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(AppDbContext db, ILogger<AuditLogService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 監査記録を追加し、呼出し元の業務トランザクションへ参加した状態で書き込む。
    /// </summary>
    /// <param name="entry">監査記録。必須項目と許可値は 222 §3.2 に従う。</param>
    /// <param name="ct">キャンセルトークン。</param>
    /// <exception cref="ArgumentNullException">entry が null のとき。</exception>
    /// <exception cref="ArgumentException">必須項目の欠落、許可外の値、または不正なJSONを指定したとき。</exception>
    /// <exception cref="DbUpdateException">DB書込みに失敗したとき。EventId `F-INF-003-E001` でログを出力したのち伝播する。</exception>
    public async Task WriteAsync(AuditLogEntry entry, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        Validate(entry);

        var record = new AuditRecord
        {
            OccurredAt = entry.OccurredAt,
            ActorId = entry.ActorId,
            FeatureId = entry.FeatureId,
            Action = entry.Action,
            TargetType = entry.TargetType,
            TargetKey = entry.TargetKey,
            Result = entry.Result,
            CorrelationId = entry.CorrelationId,
            BeforeValuesJson = NormalizeJson(entry.BeforeValuesJson),
            AfterValuesJson = NormalizeJson(entry.AfterValuesJson),
            ErrorCode = entry.ErrorCode
        };

        _db.AuditRecords.Add(record);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(
                // EventId の名前は 226-メッセージ一覧表 §3.4 に登録したメッセージ ID を使用する
                // （155-コーディング規約書 §9.5）。数値は連番3桁部分を用いる。
                new EventId(1, "F-INF-003-E001"),
                exception,
                "操作監査ログの登録に失敗しました。 FeatureId={FeatureId}, Action={Action}, TargetType={TargetType}, TargetKey={TargetKey}, Result={Result}, CorrelationId={CorrelationId}, ErrorCode={ErrorCode}",
                entry.FeatureId,
                entry.Action,
                entry.TargetType,
                entry.TargetKey,
                entry.Result,
                entry.CorrelationId,
                entry.ErrorCode);
            throw;
        }

        // 222 §4.2: 登録完了ログは DB 書込みの成功後に出力する。
        // 本サービスは呼出し元の業務トランザクションを共有するため（CodeSettingService.SaveBatchAsync 等が
        // BeginTransactionAsync の後に本メソッドを呼び出し、例外が発生しなければ CommitAsync する構成）、
        // この時点で保証されるのは書込みの成功までであり、コミットの完了ではない。
        _logger.LogInformation(
            "操作監査ログを登録しました: FeatureId={FeatureId}, Action={Action}, TargetType={TargetType}",
            entry.FeatureId,
            entry.Action,
            entry.TargetType);
    }

    private static void Validate(AuditLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        ValidateRequired(entry.ActorId, nameof(entry.ActorId));
        ValidateRequired(entry.FeatureId, nameof(entry.FeatureId));
        ValidateRequired(entry.Action, nameof(entry.Action));
        ValidateRequired(entry.TargetType, nameof(entry.TargetType));
        ValidateRequired(entry.TargetKey, nameof(entry.TargetKey));
        ValidateRequired(entry.Result, nameof(entry.Result));
        ValidateRequired(entry.CorrelationId, nameof(entry.CorrelationId));
        ValidateLength(entry.ActorId, 100, nameof(entry.ActorId));
        ValidateLength(entry.FeatureId, 20, nameof(entry.FeatureId));
        ValidateLength(entry.Action, 20, nameof(entry.Action));
        ValidateLength(entry.TargetType, 100, nameof(entry.TargetType));
        ValidateLength(entry.TargetKey, 1000, nameof(entry.TargetKey));
        ValidateLength(entry.Result, 32, nameof(entry.Result));
        ValidateLength(entry.CorrelationId, 100, nameof(entry.CorrelationId));
        ValidateLength(entry.ErrorCode, 50, nameof(entry.ErrorCode));

        // OccurredAt は 222 F-INF-003 §3.2 のとおり `DateTimeOffset.Now`（JST）を受け取る。
        // 格納先 TD_AuditRecords.OccurredAt は DATETIMEOFFSET(3) でオフセットごと保持するため UTC へ寄せない。
        // 未設定（既定値）のみを不正として弾く。
        if (entry.OccurredAt == default)
        {
            throw new ArgumentException("OccurredAt is required.", nameof(entry.OccurredAt));
        }

        if (!AllowedActions.Contains(entry.Action, StringComparer.Ordinal))
        {
            throw new ArgumentException("Action is invalid.", nameof(entry.Action));
        }

        if (!AllowedResults.Contains(entry.Result, StringComparer.Ordinal))
        {
            throw new ArgumentException("Result is invalid.", nameof(entry.Result));
        }

        ValidateJson(entry.BeforeValuesJson, nameof(entry.BeforeValuesJson));
        ValidateJson(entry.AfterValuesJson, nameof(entry.AfterValuesJson));
        ValidateTargetKey(entry.TargetKey);
        ValidateTransferAmountJson(entry.TargetType, entry.BeforeValuesJson, nameof(entry.BeforeValuesJson));
        ValidateTransferAmountJson(entry.TargetType, entry.AfterValuesJson, nameof(entry.AfterValuesJson));
    }

    private static void ValidateRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A required value is missing.", parameterName);
        }
    }

    private static void ValidateLength(string? value, int maxLength, string parameterName)
    {
        if (value is not null && value.Length > maxLength)
        {
            throw new ArgumentException($"The value must be no longer than {maxLength} characters.", parameterName);
        }
    }

    private static void ValidateJson(string? value, string parameterName)
    {
        if (value is not null)
        {
            try
            {
                using var document = JsonDocument.Parse(value);
            }
            catch (JsonException exception)
            {
                throw new ArgumentException("The value must be valid JSON.", parameterName, exception);
            }
        }
    }

    private static void ValidateTargetKey(string targetKey)
    {
        try
        {
            using var document = JsonDocument.Parse(targetKey);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException("TargetKey must be the approved JSON object format.", nameof(targetKey));
            }

            var properties = document.RootElement.EnumerateObject().ToArray();
            var propertyNames = properties.Select(property => property.Name).ToArray();
            var isApprovedTransferKey = propertyNames.SequenceEqual(AllowedTargetKeyProperties)
                && properties.All(property => property.Value.ValueKind == JsonValueKind.String);
            var isApprovedCodeSettingKey = propertyNames.SequenceEqual(AllowedCodeSettingTargetKeyProperties)
                && properties.All(property => property.Value.ValueKind == JsonValueKind.String);
            var isApprovedBankBranchKey = propertyNames.SequenceEqual(AllowedBankBranchTargetKeyProperties)
                && properties.All(property => property.Value.ValueKind == JsonValueKind.String);

            if (!isApprovedTransferKey && !isApprovedCodeSettingKey && !isApprovedBankBranchKey)
            {
                throw new ArgumentException("TargetKey must be the approved JSON object format.", nameof(targetKey));
            }
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("TargetKey must be valid JSON.", nameof(targetKey), exception);
        }
    }

    private static void ValidateTransferAmountJson(string targetType, string? value, string parameterName)
    {
        if (value is null || targetType != "TransferAmount")
        {
            return;
        }

        using var document = JsonDocument.Parse(value);
        if (document.RootElement.ValueKind != JsonValueKind.Object ||
            document.RootElement.EnumerateObject().Any(property =>
                !AllowedTransferAmountProperties.Contains(property.Name, StringComparer.Ordinal) ||
                property.Value.ValueKind != JsonValueKind.Object ||
                property.Value.EnumerateObject().Select(item => item.Name)
                    .Except(["Before", "After"], StringComparer.Ordinal).Any()))
        {
            throw new ArgumentException(
                "TransferAmount audit JSON contains a field outside the approved allow-list.",
                parameterName);
        }
    }

    private static string NormalizeJson(string? value) => string.IsNullOrWhiteSpace(value) ? "{}" : value;
}
