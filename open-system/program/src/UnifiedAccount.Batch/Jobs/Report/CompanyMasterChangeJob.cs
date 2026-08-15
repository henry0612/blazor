using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Application.Services.Reports;
using UnifiedAccount.Batch.Framework;
using UnifiedAccount.Infrastructure.Persistence;
using UnifiedAccount.Domain.Entities.Reports;
using System.Reflection;

namespace UnifiedAccount.Batch.Jobs.Report;

/// <summary>
/// REP-KOZ025 会社マスター異動リスト
/// </summary>
/// <remarks>
/// 抽出済み変更ログデータ（TR_CompanyMasterChangeLogs）を入力とし、
/// 会社マスター異動リストを帳票出力します。
///
/// 実行例:
/// UnifiedAccount.Batch.exe --job REP-KOZ025 --date 2026-07-08 --execution-id xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
/// </remarks>
public class CompanyMasterChangeJob : JobBase
{
    public CompanyMasterChangeJob(
        ITransactionCoordinator transactionCoordinator,
        ILoggerFactory loggerFactory,
        IJobRunner jobRunner)
        : base(transactionCoordinator, loggerFactory, jobRunner)
    {
    }

    public override string JobId => "REP-KOZ025";

    public string TemplateId => "KOZ025";

    public override string Description => "会社マスター異動リスト";

    /// <summary>
    /// MessageType から帳票印字用コメントへ変換するテーブル
    /// </summary>
    private static readonly Dictionary<int, string> MessageTable = new()
    {
        { 1,  "　新規" },
        { 2,  "　削除" },
        { 3,  "　修正前" },
        { 4,  "　　　後" },
        { 5,  "＊項目エラー" },
        { 6,  "＊ダブりエラー" },
        { 7,  "＊新規不足" },
        { 8,  "＊マスターあり" },
        { 9,  "＊マスターなし" },
        { 10, "＊削除不可" }
    };

    /// <summary>
    /// ジョブ本体を実行する。
    /// </summary>
    /// <param name="context">ジョブコンテキスト</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>ジョブの実行結果</returns>
    /// <remarks>
    /// トランザクションは <see cref="JobBase"/> が管理するため、本メソッドでは開始も終了もしない。
    /// 本ジョブは読取専用であり更新を行わない。
    /// </remarks>
    protected override async Task<JobResult> ExecuteCoreAsync(JobContext context, CancellationToken ct)
    {
        var logger = Logger as ILogger<CompanyMasterChangeJob> ?? context.Services.GetRequiredService<ILogger<CompanyMasterChangeJob>>();
        var db = context.Services.GetRequiredService<AppDbContext>();
        var reportService = context.Services.GetRequiredService<IReportOutputService>();
        var filePathProvider = context.Services.GetRequiredService<IReportFilePathProvider>();
        var metrics = new JobMetrics();

        logger.LogInformation(
            "[{JobId}] {ExecutionId} {Description}開始 ProcessDate={ProcessDate:yyyy-MM-dd} TemplateId={TemplateId}",
            JobId,
            context.JobExecutionId,
            Description,
            context.ProcessDate,
            TemplateId);

        try
        {
            // データ取得
            // 仕様書: JobExecutionIdで抽出し、CompanyCode, DenkCode, IdokCode 順に読み込む
            var sourceRows = await db.CompanyMasterChangeLogs
                .AsNoTracking()
                .Where(e => e.JobExecutionId == context.JobExecutionId)
                .OrderBy(e => e.Id)
                .ToListAsync(ct);

            var readCount = sourceRows.Count;
            metrics.IncrementRead(readCount);

            if (readCount == 0)
            {
                logger.LogInformation(
                    "[{JobId}] {ExecutionId} {Description} 異動データなし ProcessDate={ProcessDate:yyyy-MM-dd}",
                    JobId,
                    context.JobExecutionId,
                    Description,
                    context.ProcessDate);

                return metrics.ToResult(message: "異動データなし");
            }

            // 帳票データ編集
            // - MessageType を帳票用コメント文字列へ変換
            // - DenkCode が 11～19 以外のレコードはスキップ
            // - 元エンティティ項目は CompanyMasterChangeReport が継承して保持する
            var reportRows = new List<CompanyMasterChangeReport>();
            var skippedCount = 0;

            foreach (var row in sourceRows)
            {
                if (!IsValidRequestType(row.RequestType))
                {
                    skippedCount++;

                    logger.LogWarning(
                        "[{JobId}] {ExecutionId} {Description} 異動区分不正のためスキップ Id={Id} CompanyCode={CompanyCode} RequestType={RequestType} ChangeAction={ChangeAction}",
                        JobId,
                        context.JobExecutionId,
                        Description,
                        row.Id,
                        row.CompanyCode,
                        row.RequestType,
                        row.ChangeAction);

                    continue;
                }

                reportRows.Add(CreateReportRow(row));
            }

            if (skippedCount > 0)
            {
                metrics.IncrementError(skippedCount);
            }

            if (reportRows.Count == 0)
            {
                logger.LogInformation(
                    "[{JobId}] {ExecutionId} {Description} 出力対象データなし ProcessDate={ProcessDate:yyyy-MM-dd} SkipCount={SkipCount}",
                    JobId,
                    context.JobExecutionId,
                    Description,
                    context.ProcessDate,
                    skippedCount);

                return metrics.ToResult(message: "出力対象データなし");
            }

            // 帳票出力設定
            var request = new ReportOutputRequest
            {
                ExecutionId = $"{context.JobExecutionId}-KOZ025",
                JobExecutionId = context.JobExecutionId,
                ProcessDate = context.ProcessDate,
                TemplateId = TemplateId,
                DataRequest = new ReportDataRequest
                {
                    Mode = ReportDataMode.Streamed,
                    SourceData = reportRows.Cast<object>()
                }
            };

            // 帳票出力
            var result = await reportService.OutputAsync(request, ct);

            if (!result.IsSuccessful)
            {
                var message = string.Join(", ", result.ValidationErrors);

                logger.LogWarning(
                    "[{JobId}] {ExecutionId} {Description}出力失敗 ProcessDate={ProcessDate:yyyy-MM-dd} ValidationErrors={ValidationErrors}",
                    JobId,
                    context.JobExecutionId,
                    Description,
                    context.ProcessDate,
                    message);

                return metrics.ToResult(false, $"帳票出力失敗: {message}");
            }

            metrics.IncrementWrite(reportRows.Count);

            logger.LogInformation(
                "[{JobId}] {ExecutionId} {Description}完了 読込件数={ReadCount} 出力件数={WriteCount} スキップ件数={SkipCount} OutputPath={OutputPath}",
                JobId,
                context.JobExecutionId,
                Description,
                readCount,
                reportRows.Count,
                skippedCount,
                result.OutputFilePath);

            return metrics.ToResult(message: $"出力完了: {result.OutputFilePath}");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[{JobId}] {ExecutionId} {Description}異常終了 ProcessDate={ProcessDate:yyyy-MM-dd}",
                JobId,
                context.JobExecutionId,
                Description,
                context.ProcessDate);

            return metrics.ToResult(false, ex.Message);
        }
    }

    /// <summary>
    /// 帳票出力用 DTO を作成する。
    /// 元エンティティのプロパティは継承先へコピーし、帳票編集項目を追加設定する。
    /// </summary>
    private static CompanyMasterChangeReport CreateReportRow(CompanyMasterChangeLog source)
    {
        var report = new CompanyMasterChangeReport();

        CopyEntityProperties(source, report);

        var message = GetMessageText(source.MessageType);
        report.MessageTypeName = message;

        return report;
    }

    /// <summary>
    /// CompanyMasterChangeLog の public set 可能なプロパティをコピーする。
    /// DB項目が多い場合でも、個別に列を列挙せず帳票DTOへ引き継ぐための処理。
    /// </summary>
    private static void CopyEntityProperties(CompanyMasterChangeLog source, CompanyMasterChangeReport destination)
    {
        var properties = typeof(CompanyMasterChangeLog).GetProperties(BindingFlags.Instance | BindingFlags.Public);

        foreach (var property in properties)
        {
            if (!property.CanRead || !property.CanWrite)
            {
                continue;
            }

            if (property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            var value = property.GetValue(source);
            property.SetValue(destination, value);
        }
    }

    /// <summary>
    /// RequestType が 11～19 の範囲か判定する。
    /// </summary>
    private static bool IsValidRequestType(string? requestType)
    {
        return int.TryParse(requestType, out var code) && code >= 11 && code <= 19;
    }

    /// <summary>
    /// MessageType を帳票印字用コメントへ変換する。
    /// int / string / nullable int のどれでも受けられるよう object? で受ける。
    /// </summary>
    private static string GetMessageText(object? messageType)
    {
        if (messageType == null)
        {
            return string.Empty;
        }

        if (!int.TryParse(messageType.ToString(), out var type))
        {
            return string.Empty;
        }

        return MessageTable.TryGetValue(type, out var text)
            ? text
            : string.Empty;
    }
}

/// <summary>
/// REP-KOZ025 会社マスター異動リスト 帳票出力用DTO。
///
/// CompanyMasterChangeLog を継承することで、
/// 既存DB項目を帳票項目としてそのまま使用しつつ、
/// 帳票専用の編集項目だけを追加する。
/// </summary>
public class CompanyMasterChangeReport : CompanyMasterChangeLog
{

    /// <summary>
    /// 帳票の「コメント」印字用。
    /// MessageType を 1:新規、2:削除... の文字列へ変換した値。
    /// </summary>
    public string ProcessingDateText => ProcessingDate.ToString("yyyy.MM.dd");

    /// <summary>
    /// 帳票の「コメント」印字用。
    /// MessageType を 1:新規、2:削除... の文字列へ変換した値。
    /// </summary>
    public string MessageTypeName { get; set; } = string.Empty;

    /// <summary>
    /// 基本料 印字用。
    /// BasicFeeがNULLまたは空白の場合は空白を、それ以外の場合はそのままの値を出力する
    /// </summary>
    public string BasicFeeString => BasicFee?.ToString("#####0") ?? string.Empty;

    /// <summary>
    /// 事務料 印字用。
    /// AdminFee がNULLまたは空白の場合は空白を、それ以外の場合はそのままの値を出力する
    /// </summary>
    public string AdminFeeString => AdminFee?.ToString("#####0") ?? string.Empty;

    /// <summary>
    /// 新規単価 印字用。
    /// NewUnitPrice がNULLまたは空白の場合は空白を、それ以外の場合はそのままの値を出力する
    /// </summary>
    public string NewUnitPriceString => NewUnitPrice?.ToString("###0") ?? string.Empty;

    /// <summary>
    /// 修正単価 印字用。
    /// ModifyUnitPrice がNULLまたは空白の場合は空白を、それ以外の場合はそのままの値を出力する
    /// </summary>
    public string ModifyUnitPriceString => ModifyUnitPrice?.ToString("###0") ?? string.Empty;

    /// <summary>
    /// 振替単価１ 印字用。
    /// Transfer1UnitPrice がNULLまたは空白の場合は空白を、それ以外の場合はそのままの値を出力する
    /// </summary>
    public string Transfer1UnitPriceString => Transfer1UnitPrice?.ToString("###0") ?? string.Empty;

    /// <summary>
    /// 振替単価２ 印字用。
    /// Transfer2UnitPrice がNULLまたは空白の場合は空白を、それ以外の場合はそのままの値を出力する
    /// </summary>
    public string Transfer2UnitPriceString => Transfer2UnitPrice?.ToString("###0") ?? string.Empty;

    /// <summary>
    /// 領収単価 印字用。
    /// ReceiptUnitPrice がNULLまたは空白の場合は空白を、それ以外の場合はそのままの値を出力する
    /// </summary>
    public string ReceiptUnitPriceString => ReceiptUnitPrice?.ToString("###0") ?? string.Empty;

    /// <summary>
    /// 振込先1。
    /// 仕様書: BankCode1, BranchCode1, AccountType1, AccountNo1 を "-" で結合する。
    /// BankCode1が空白の場合 "-" は出力しない
    /// </summary>
    public string Bank1Account => string.IsNullOrWhiteSpace(Transfer1BankCode)
        ? string.Empty
        : JoinWithHyphen(Transfer1BankCode, Transfer1BranchCode, Transfer1AccountType, Transfer1AccountNo);

    /// <summary>
    /// 振込先2。
    /// 仕様書: BankCode2, BranchCode2, AccountType2, AccountNo2 を "-" で結合する。
    /// BankCode2が空白の場合 "-" は出力しない
    /// </summary>
    public string Bank2Account => string.IsNullOrWhiteSpace(Transfer2BankCode)
        ? string.Empty
        : JoinWithHyphen(Transfer2BankCode, Transfer2BranchCode, Transfer2AccountType, Transfer2AccountNo);

    /// <summary>
    /// OP年月。
    /// 仕様書: OperatingYear と OperatingMonth を "." で結合する。
    /// OperatingYear "." は出力しない
    /// </summary>
    public string OperatingYM => string.IsNullOrWhiteSpace(OperatingYear)
        ? string.Empty
        : JoinWithDot(OperatingYear, OperatingMonth);

    private static string JoinWithHyphen(params object?[] values)
    {
        return string.Join("-", values.Select(v => v?.ToString() ?? string.Empty));
    }

    private static string JoinWithDot(params object?[] values)
    {
        return string.Join(".", values.Select(v => v?.ToString() ?? string.Empty));
    }
}
