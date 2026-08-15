using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Batch.Framework;
using UnifiedAccount.Domain.Enums;
using UnifiedAccount.Domain.Entities.Changes;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Domain.Entities.Reports;
using UnifiedAccount.Infrastructure.Persistence;

/// <summary>
/// 会社マスタ登録のバッチジョブクラス
/// </summary>
public class CompanyMasterRegistrationJob : JobBase
{
    public CompanyMasterRegistrationJob(ITransactionCoordinator transactionCoordinator, ILoggerFactory loggerFactory, IJobRunner jobRunner)
        : base(transactionCoordinator, loggerFactory, jobRunner)
    {
    }

    public override string JobId => "BAT-KOZ024";
    public override string Description => "会社マスタ登録";

    /// <summary>
    /// ジョブ本体を実行します。
    /// </summary>
    /// <param name="context">ジョブコンテキスト</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>ジョブの実行結果</returns>
    /// <remarks>
    /// トランザクションは <see cref="JobBase"/> が管理するため、本メソッドでは開始も終了もしない。
    /// 223 §6.4.6 は「マスタ反映は実行ID単位の全件一括コミット」と定めており、ジョブ単位の境界がこれに対応する。
    /// 同 §6.4.6 が定める「帳票出力元データはロールバック対象外」は本トランザクションでは満たせないため、
    /// 別 DbContext での永続化が必要である（F-KOZ-001 コードレビュー記録 K1-006）。
    /// </remarks>
    protected override async Task<JobResult> ExecuteCoreAsync(JobContext context, CancellationToken ct)
    {
        var logger = Logger as ILogger<CompanyMasterRegistrationJob> ?? context.Services.GetRequiredService<ILogger<CompanyMasterRegistrationJob>>();
        var db = context.Services.GetRequiredService<AppDbContext>();
        // Job内のカウントに使用するメトリクスの初期化
        var metrics = new JobMetrics();

        logger.LogInformation(
            "[{JobId}] {ExecutionId} {Description}開始 ProcessDate={ProcessDate:yyyy-MM-dd}",
            JobId,
            context.JobExecutionId,
            Description,
            context.ProcessDate);

        try
        {
            // 取得条件設定(変更リクエスト)
            IQueryable<CompanyChangeRequest> targetChangeRequests = db.CompanyChangeRequests
                .Where(e => e.JobExecutionId == context.JobExecutionId);

            // 件数を取得
            var readCount = await targetChangeRequests.CountAsync(ct);
            // メトリクスに読込件数を記録
            metrics.IncrementRead(readCount);

            if (readCount > 1000000)
            {
                logger.LogWarning(
                    "[{JobId}] {ExecutionId} {Description}業務失敗終了 ProcessDate={ProcessDate:yyyy-MM-dd} FailureReason={FailureReason}",
                    JobId,
                    context.JobExecutionId,
                    Description,
                    context.ProcessDate,
                    "読込件数上限超過");
                return metrics.ToResult(false, "読込件数上限超過");
            }

            if (readCount == 0)
            {
                logger.LogInformation(
                    "[{JobId}] {ExecutionId} {Description}正常終了 読込件数={ReadCount} 更新件数={WriteCount} エラー件数={ErrorCount}",
                    JobId,
                    context.JobExecutionId,
                    Description,
                    readCount,
                    0,
                    0);
                return metrics.ToResult(message: "対象データなし");
            }

            // データ取得(変更リクエスト)
            List<CompanyChangeRequest> changeRequestList = await targetChangeRequests
                .OrderBy(e => e.CompanyCode)
                .ThenBy(e => e.RequestType)
                .ThenBy(e => e.ChangeAction)
                .ToListAsync(ct);

            // データ取得(委託会社マスタ)
            List<Company> targetCompanies = await db.Companies
                .Where(company =>
                    targetChangeRequests.Any(request =>
                        request.CompanyCode == company.CompanyCode))
                .Include(c => c.WithdrawalDays) // ← CompanyWithdrawalDay を一緒に取得
                .Include(c => c.Types) // ← CompanyType を一緒に取得
                .AsSplitQuery()
                .ToListAsync(ct);

            // 会社マスタ検索用のDictionary
            Dictionary<string, Company> companiesByCode = targetCompanies
            .Where(company => !string.IsNullOrWhiteSpace(company.CompanyCode))
            .ToDictionary(company => company.CompanyCode);


            HashSet<string?> countExceededErrorCompanyCodes = GetCountExceededErrorCompanyCodes(changeRequestList);
            HashSet<string?> combinationErrorCompanyCodes = GetCombinationOrNotApplicableErrorCompanyCodes(changeRequestList);

            // ----------------------------------------
            // 変更リクエスト単体のエラーチェック
            // ----------------------------------------
            foreach (CompanyChangeRequest changeRequestItem in changeRequestList)
            {

                // 必須・書式エラー
                changeRequestItem.IsRequiredOrFormatError =
                    changeRequestItem.CheckIsRequiredOrFormatError();

                // 項目別数値・年月エラー
                changeRequestItem.IsUnitPriceOrOperatingYearMonthError =
                    changeRequestItem.CheckIsUnitPriceOrOperatingYearMonthError();

                changeRequestItem.IsModifyUnitPriceOrAmountError =
                    changeRequestItem.CheckIsModifyUnitPriceOrAmountError();

                changeRequestItem.IsTransfer1UnitPriceError =
                    changeRequestItem.CheckIsTransfer1UnitPriceError();

                changeRequestItem.IsTransfer2UnitPriceError =
                    changeRequestItem.CheckIsTransfer2UnitPriceError();

                changeRequestItem.IsReceiptUnitPriceError =
                    changeRequestItem.CheckIsReceiptUnitPriceError();

                // 組合せ・適用不可エラー
                if (changeRequestItem.CompanyCode == null
                    || combinationErrorCompanyCodes.Contains(changeRequestItem.CompanyCode))
                {
                    changeRequestItem.IsCombinationOrNotApplicableError = true;
                }
                // ダブりエラー
                if (changeRequestItem.CompanyCode != null
                    && countExceededErrorCompanyCodes.Contains(changeRequestItem.CompanyCode))
                {
                    changeRequestItem.IsCountExceededError = true;
                }

                // 異動区分エラー
                changeRequestItem.IsChangeActionError =
                    changeRequestItem.CheckIsChangeActionError();
            }

            // ----------------------------------------
            // 会社コード・異動区分単位で処理
            // ----------------------------------------
            var requestGroups = changeRequestList.GroupBy(request => new
            {
                request.CompanyCode,
                request.ChangeAction
            });
            var createdChangeLogKeys = new HashSet<(string? CompanyCode, string? RequestType)>();
            var createdKanjiChangeLogKeys = new HashSet<(string? CompanyCode, string? RequestType, string? Cban)>();
            foreach (var requestGroup in requestGroups)
            {

                List<CompanyChangeRequest> requests = requestGroup.ToList();
                CompanyChangeRequest.ChangeActionType? changeActionType =
                    requests[0].GetChangeActionType();

                Company? company = null;
                if (!string.IsNullOrWhiteSpace(requestGroup.Key.CompanyCode))
                {
                    companiesByCode.TryGetValue(
                        requestGroup.Key.CompanyCode,
                        out company);
                }

                // ----------------------------------------
                // マスタ存在状態による適用可否チェック
                // ----------------------------------------
                if ((changeActionType == CompanyChangeRequest.ChangeActionType.Add && company != null)
                    ||
                    (changeActionType is
                        CompanyChangeRequest.ChangeActionType.Update
                        or CompanyChangeRequest.ChangeActionType.Delete
                    && company == null))
                {
                    foreach (CompanyChangeRequest request in requests)
                    {
                        request.IsCombinationOrNotApplicableError = true;
                    }
                }

                // グループ内に1件でもエラーがあれば、会社を追加しない。
                if (changeActionType == CompanyChangeRequest.ChangeActionType.Add
                    && requests.Any(request => request.IsError()))
                {
                    foreach (CompanyChangeRequest request in requests)
                    {
                        if (!request.IsError())
                        {
                            request.IsCombinationOrNotApplicableError = true;
                        }
                    }
                }

                // ----------------------------------------
                // 会社マスタ追加
                // ----------------------------------------
                if (changeActionType == CompanyChangeRequest.ChangeActionType.Add
                    && company == null
                    && requests.All(request => !request.IsError()))
                {
                    company = CreateCompany(requests);
                    db.Companies.Add(company);

                    // 同一処理内の後続データからも参照できるようにする
                    companiesByCode.Add(company.CompanyCode, company);
                }

                // ----------------------------------------
                // 各変更リクエストの反映・ログ登録
                // ----------------------------------------
                foreach (CompanyChangeRequest request in requests)
                {
                    if (!request.IsError())
                    {
                        switch (changeActionType)
                        {
                            case CompanyChangeRequest.ChangeActionType.Delete:
                                company!.SuspendFlag = CompanyStatus.Suspended.ToString("D");
                                break;
                            case CompanyChangeRequest.ChangeActionType.Update:
                                // RequestTypeに応じてCompanyへ値を反映
                                ApplyRequestToCompany(company!, request);
                                break;
                            default:
                                break;
                        }
                    }
                    AddChangeLog(context, db, request, company, createdChangeLogKeys, createdKanjiChangeLogKeys);
                    request.IsApplied = true;
                }
            }

            // Company更新件数を取得
            int writecount = db.ChangeTracker
            .Entries<Company>()
            .Count(x => x.State is EntityState.Added or
                        EntityState.Modified or
                        EntityState.Deleted);
            // CompanyChangeRequestのエラー件数を取得
            int errorcount = changeRequestList.Count(x => x.IsError());

            // データ更新を保存する。コミットは JobBase が管理するトランザクション境界で行われる。
            await db.SaveChangesAsync(ct);

            // メトリクスに件数を記録
            metrics.IncrementWrite(writecount);
            metrics.IncrementError(errorcount);

            logger.LogInformation(
                "[{JobId}] {ExecutionId} {Description}正常終了 読込件数={ReadCount} 更新件数={WriteCount} エラー件数={ErrorCount}",
                JobId,
                context.JobExecutionId,
                Description,
                metrics.ReadCount,
                metrics.WriteCount,
                metrics.ErrorCount);

            return metrics.ToResult(message: $"処理完了: 読込件数={metrics.ReadCount}件 更新件数={metrics.WriteCount}件 エラー件数={metrics.ErrorCount}件");
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

    private HashSet<string?> GetCombinationOrNotApplicableErrorCompanyCodes(List<CompanyChangeRequest> entityItems)
    {
        var allRequiredRequestTypes = new[] { "11", "12", "14", "15" }; // 全て必須
        var anyRequiredRequestTypes = new[] { "13", "16", "17", "18", "19" }; // いずれか必要

        return entityItems
            .GroupBy(x => x.CompanyCode)
            .Where(group =>
            {
                var changeActions = group
                    .Select(x => x.GetChangeActionType())
                    .Distinct()
                    .ToList();

                // 同一会社でChangeActionが混在
                if (changeActions.Count > 1)
                {
                    return true;
                }

                var changeAction = changeActions.SingleOrDefault();

                if (changeAction
                    == CompanyChangeRequest.ChangeActionType.Add)
                {
                    var hasAllRequiredRequestTypes =
                        allRequiredRequestTypes.All(requiredType =>
                            group.Any(x => x.RequestType == requiredType));

                    var hasAnyRequiredRequestType =
                        group.Any(x =>
                            anyRequiredRequestTypes.Contains(x.RequestType));

                    return !hasAllRequiredRequestTypes
                        || !hasAnyRequiredRequestType;
                }

                return false;
            })
            .Select(group => group.Key)
            .ToHashSet();
    }

    private HashSet<string?> GetCountExceededErrorCompanyCodes(List<CompanyChangeRequest> entityItems)
    {

        return entityItems
            .GroupBy(x => x.CompanyCode)
            .Where(group =>
            {

                // 11～19は同一RequestTypeの複数件を許可しない
                var hasDuplicateRequestTypes = group
                    .Where(x => x.RequestType != "71")
                    .GroupBy(x => x.RequestType)
                    .Any(x => x.Count() > 1);

                // 71は同一Cbanの複数件を許可しない
                var hasDuplicateKanjiRequestTypes = group
                    .Where(x => x.RequestType == "71")
                    .GroupBy(x => x.Cban)
                    .Any(x => x.Count() > 1);

                if (hasDuplicateRequestTypes
                    || hasDuplicateKanjiRequestTypes)
                {
                    return true;
                }

                return false;
            })
            .Select(group => group.Key)
            .ToHashSet();
    }

    private void ApplyRequestToCompany(
        Company company, CompanyChangeRequest changeRequest)
    {
        CompanyChangeRequestRoute route =
            changeRequest.GetDenpyoKubun();

        switch (route)
        {
            case CompanyChangeRequestRoute.CompanyBasic11:
                ApplyCompanyBasic11(company, changeRequest);
                break;
            case CompanyChangeRequestRoute.CompanyAddress12:
                ApplyCompanyAddress12(company, changeRequest);
                break;
            case CompanyChangeRequestRoute.CompanyProcessing13:
                ApplyCompanyProcessing13(company, changeRequest);
                break;
            case CompanyChangeRequestRoute.CompanyFee14:
                ApplyCompanyFee14(company, changeRequest);
                break;
            case CompanyChangeRequestRoute.CompanyTransfer15:
                ApplyCompanyTransfer15(company, changeRequest);
                break;
            case CompanyChangeRequestRoute.CompanyType16To19:
                ApplyCompanyType16To19(company, changeRequest);
                break;
            case CompanyChangeRequestRoute.CompanyKanjiName71:
                ApplyCompanyKanjiName71(company, changeRequest);
                break;
            case CompanyChangeRequestRoute.CompanyKanjiAddress71:
                ApplyCompanyKanjiAddress71(company, changeRequest);
                break;
            case CompanyChangeRequestRoute.CompanyKanjiContact71:
                ApplyCompanyKanjiContact71(company, changeRequest);
                break;

            default:
                break;
        }
    }

    private Company CreateCompany(IReadOnlyCollection<CompanyChangeRequest> changeRequests)
    {
        string companyCode = changeRequests
        .Select(request => request.CompanyCode)
        .FirstOrDefault(code => !string.IsNullOrWhiteSpace(code))
        ?? throw new InvalidOperationException(
            "会社コードが設定されていない変更リクエストから会社マスタを作成しようとしました。");

        var company = new Company
        {
            CompanyCode = companyCode,
        };

        // 各RequestTypeごとに会社マスタのプロパティを設定する。
        foreach (CompanyChangeRequest changeRequest in changeRequests)
        {
            ApplyRequestToCompany(company, changeRequest);
        }

        return company;
    }

    private void ApplyCompanyBasic11(Company company, CompanyChangeRequest changeRequest)
    {
        SetIfNotEmpty(changeRequest.CompanyName, x => company.CompanyNameKana = x);
        SetIfNotEmpty(changeRequest.PostalCode, x => company.PostalCode = x);
        SetIfNotEmpty(changeRequest.Prefecture, x => company.Prefecture = x);
        SetIfNotEmpty(changeRequest.PhoneNumber, x => company.PhoneNumber = x);
    }

    private void ApplyCompanyAddress12(Company company, CompanyChangeRequest changeRequest)
    {
        SetIfNotEmpty(changeRequest.City, x => company.City = x);
        SetIfNotEmpty(changeRequest.Town1, x => company.Town1 = x);
        SetIfNotEmpty(changeRequest.Town2, x => company.Town2 = x);
    }

    private void ApplyCompanyProcessing13(Company company, CompanyChangeRequest changeRequest)
    {
        SetIfNotEmpty(changeRequest.ProcessingFlag1, x => company.ProcessingFlag1 = x);
        SetIfNotEmpty(changeRequest.ProcessingFlag2, x => company.ProcessingFlag2 = x);
        SetIfNotEmpty(changeRequest.ProcessingFlag3, x => company.ProcessingFlag3 = x);
        SetIfNotEmpty(changeRequest.ProcessingFlag4, x => company.ProcessingFlag4 = x);
        SetIfNotEmpty(changeRequest.ProcessingFlag5, x => company.ProcessingFlag5 = x);
        SetIfNotEmpty(changeRequest.ProcessingFlag6, x => company.ProcessingFlag6 = x);
        SetIfNotEmpty(changeRequest.ProcessingFlag7, x => company.ProcessingFlag7 = x);
        SetIfNotEmpty(changeRequest.ProcessingFlag8, x => company.ProcessingFlag8 = x);
        SetIfNotEmpty(changeRequest.ProcessingFlag9, x => company.ProcessingFlag9 = x);
        SetIfNotEmpty(changeRequest.ProcessingFlag10, x => company.ProcessingFlag10 = x);
        SetIfNotEmpty(changeRequest.ProcessingFlag11, x => company.ProcessingFlag11 = x);
        SetIfNotEmpty(changeRequest.ProcessingFlag12, x => company.ProcessingFlag12 = x);
        SetIfNotEmpty(changeRequest.SheetFlag1, x => company.SheetFlag1 = x);
        SetIfNotEmpty(changeRequest.SheetFlag2, x => company.SheetFlag2 = x);
        SetIfNotEmpty(changeRequest.SheetFlag3, x => company.SheetFlag3 = x);
        SetIfNotEmpty(changeRequest.SheetFlag4, x => company.SheetFlag4 = x);
        SetIfNotEmpty(changeRequest.SheetFlag5, x => company.SheetFlag5 = x);
        SetIfNotEmpty(changeRequest.SheetFlag6, x => company.SheetFlag6 = x);
        SetIfNotEmpty(changeRequest.SheetFlag7, x => company.SheetFlag7 = x);
        SetIfNotEmpty(changeRequest.SheetFlag8, x => company.SheetFlag8 = x);
        SetIfNotEmpty(changeRequest.SheetFlag9, x => company.SheetFlag9 = x);
        SetIfNotEmpty(changeRequest.SheetFlag10, x => company.SheetFlag10 = x);
        SetIfNotEmpty(changeRequest.SheetFlag11, x => company.SheetFlag11 = x);
        SetIfNotEmpty(changeRequest.SheetFlag12, x => company.SheetFlag12 = x);

        for (short slotNo = 1; slotNo <= 4; slotNo++)
        {
            string? withdrawalDayValue = slotNo switch
            {
                1 => changeRequest.WithdrawalDay1,
                2 => changeRequest.WithdrawalDay2,
                3 => changeRequest.WithdrawalDay3,
                4 => changeRequest.WithdrawalDay4,
                _ => string.Empty
            };
            if (string.IsNullOrEmpty(withdrawalDayValue))
            {
                continue;
            }
            var withdrawalDay = company.WithdrawalDays.FirstOrDefault(x => x.SlotNo == slotNo);
            if (withdrawalDay == null)
            {
                withdrawalDay = new CompanyWithdrawalDay
                {
                    CompanyId = company.Id,
                    SlotNo = slotNo,
                };
                company.WithdrawalDays.Add(withdrawalDay);
            }
            withdrawalDay.WithdrawalDay = withdrawalDayValue;
        }
    }

    private void ApplyCompanyFee14(Company company, CompanyChangeRequest changeRequest)
    {
        company.PageChangeKey = (short)changeRequest.PageChangeKey;
        SetIfHasValue(changeRequest.BasicFee, x => company.BasicFee = x);
        SetIfHasValue(changeRequest.AdminFee, x => company.AdminFee = x);
        SetIfHasValue(changeRequest.NewUnitPrice, x => company.NewUnitPrice = x);
        SetIfHasValue(changeRequest.ModifyUnitPrice, x => company.ModifyUnitPrice = x);
        SetIfHasValue(changeRequest.Transfer1UnitPrice, x => company.Transfer1UnitPrice = x);
        SetIfHasValue(changeRequest.Transfer2UnitPrice, x => company.Transfer2UnitPrice = x);
        SetIfHasValue(changeRequest.ReceiptUnitPrice, x => company.ReceiptUnitPrice = x);

    }

    private void ApplyCompanyTransfer15(Company company, CompanyChangeRequest changeRequest)
    {
        SetIfNotEmpty(changeRequest.Transfer1BankCode, x => company.Transfer1BankCode = x);
        SetIfNotEmpty(changeRequest.Transfer1BranchCode, x => company.Transfer1BranchCode = x);
        SetIfNotEmpty(changeRequest.Transfer1AccountType, x => company.Transfer1AccountType = x);
        SetIfNotEmpty(changeRequest.Transfer1AccountNo, x => company.Transfer1AccountNo = x);
        SetIfNotEmpty(changeRequest.ConsignorCode, x => company.ConsignorCode = x);
        SetIfNotEmpty(changeRequest.Transfer2BankCode, x => company.Transfer2BankCode = x);
        SetIfNotEmpty(changeRequest.Transfer2BranchCode, x => company.Transfer2BranchCode = x);
        SetIfNotEmpty(changeRequest.Transfer2AccountType, x => company.Transfer2AccountType = x);
        SetIfNotEmpty(changeRequest.Transfer2AccountNo, x => company.Transfer2AccountNo = x);
        SetIfNotEmpty(changeRequest.PassbookComment, x => company.PassbookComment = x);
    }

    private void ApplyCompanyType16To19(Company company, CompanyChangeRequest changeRequest)
    {
        int typeNo = changeRequest.ConvertRequestTypeToTypeNo();
        if (!company.Types.Any(t => t.TypeNo == typeNo))
        {
            CompanyType type = new CompanyType
            {
                CompanyId = company.Id,
                TypeNo = (short)typeNo
            };
            company.Types.Add(type);
        }
        CompanyType companyType = company.Types.First(t => t.TypeNo == typeNo);
        SetIfNotEmpty(changeRequest.TypeCategory, x => companyType.TypeCode = x);
        SetIfNotEmpty(changeRequest.Cycle, x => companyType.Cycle = x);
        SetIfNotEmpty(changeRequest.OperatingYear + changeRequest.OperatingMonth, x => companyType.StartYearMonth = x);
        SetIfNotEmpty(changeRequest.TypeName, x => companyType.TypeName = x);
        SetIfHasValue(changeRequest.Amount, x => companyType.Amount = x);
    }

    private void ApplyCompanyKanjiName71(Company company, CompanyChangeRequest changeRequest)
    {
        SetIfNotEmpty(changeRequest.CompanyNameKanji, x => company.CompanyNameKanji = x);
    }

    private void ApplyCompanyKanjiAddress71(Company company, CompanyChangeRequest changeRequest)
    {
        SetIfNotEmpty(changeRequest.PrefectureKanji, x => company.PrefectureKanji = x);
        SetIfNotEmpty(changeRequest.CityKanji, x => company.CityKanji = x);
        SetIfNotEmpty(changeRequest.TownKanji1, x => company.TownKanji1 = x);
        SetIfNotEmpty(changeRequest.TownKanji2, x => company.TownKanji2 = x);
    }

    private void ApplyCompanyKanjiContact71(Company company, CompanyChangeRequest changeRequest)
    {
        SetIfNotEmpty(changeRequest.DepartmentKanji, x => company.DepartmentKanji = x);
        SetIfNotEmpty(changeRequest.PersonInChargeKanji, x => company.PersonInChargeKanji = x);
    }

    private void AddChangeLog(
        JobContext context, AppDbContext db, CompanyChangeRequest changeRequest, Company? company,
        ISet<(string? CompanyCode, string? RequestType)> createdChangeLogKeys,
        ISet<(string? CompanyCode, string? RequestType, string? Cban)> createdKanjiChangeLogKeys)
    {
        CompanyChangeRequestRoute route = changeRequest.GetDenpyoKubun();
        switch (route)
        {
            case CompanyChangeRequestRoute.CompanyBasic11:
            case CompanyChangeRequestRoute.CompanyAddress12:
            case CompanyChangeRequestRoute.CompanyProcessing13:
            case CompanyChangeRequestRoute.CompanyFee14:
            case CompanyChangeRequestRoute.CompanyTransfer15:
            case CompanyChangeRequestRoute.CompanyType16To19:
                if (!createdChangeLogKeys.Add((changeRequest.CompanyCode, changeRequest.RequestType)))
                {
                    // 既に同一会社コード・同一RequestTypeのログが作成済みの場合は、追加しない。
                    break;
                }
                if (changeRequest.IsError())
                {
                    CompanyChangeMessageType errorType = GetErrorType(changeRequest, company);
                    db.CompanyMasterChangeLogs.Add(
                        CreateCompanyMasterChangeLog_Error(context, changeRequest, errorType));
                }
                else
                {
                    switch (changeRequest.GetChangeActionType())
                    {
                        case CompanyChangeRequest.ChangeActionType.Delete:
                            db.CompanyMasterChangeLogs.Add(
                                CreateCompanyMasterChangeLog_Before(context, db, changeRequest, company, CompanyChangeMessageType.Delete));
                            break;
                        case CompanyChangeRequest.ChangeActionType.Add:
                            db.CompanyMasterChangeLogs.Add(
                                CreateCompanyMasterChangeLog_After(context, changeRequest, company, CompanyChangeMessageType.Add));
                            break;
                        case CompanyChangeRequest.ChangeActionType.Update:
                            db.CompanyMasterChangeLogs.Add(
                                CreateCompanyMasterChangeLog_Before(context, db, changeRequest, company, CompanyChangeMessageType.Update_Before));
                            db.CompanyMasterChangeLogs.Add(
                                CreateCompanyMasterChangeLog_After(context, changeRequest, company, CompanyChangeMessageType.Update_After));
                            break;
                        default:
                            break;
                    }
                }
                break;
            case CompanyChangeRequestRoute.CompanyKanjiName71:
            case CompanyChangeRequestRoute.CompanyKanjiAddress71:
            case CompanyChangeRequestRoute.CompanyKanjiContact71:
                if (!createdKanjiChangeLogKeys.Add((changeRequest.CompanyCode, changeRequest.RequestType, changeRequest.Cban)))
                {
                    // 既に同一会社コード・同一RequestType・同一Cbanのログが作成済みの場合は、追加しない。
                    break;
                }
                if (changeRequest.IsError())
                {
                    CompanyChangeMessageType errorType = GetErrorType(changeRequest, company);
                    db.KanjiCompanyMasterChangeLogs.Add(
                        CreateKanjiCompanyMasterChangeLog_Common(context, changeRequest, errorType));
                }
                else
                {
                    switch (changeRequest.GetChangeActionType())
                    {
                        case CompanyChangeRequest.ChangeActionType.Delete:
                            db.KanjiCompanyMasterChangeLogs.Add(
                                CreateKanjiCompanyMasterChangeLog_Common(context, changeRequest, CompanyChangeMessageType.Delete));
                            break;
                        case CompanyChangeRequest.ChangeActionType.Add:
                            db.KanjiCompanyMasterChangeLogs.Add(
                                CreateKanjiCompanyMasterChangeLog_After(context, changeRequest, company, CompanyChangeMessageType.Add));
                            break;
                        case CompanyChangeRequest.ChangeActionType.Update:
                            db.KanjiCompanyMasterChangeLogs.Add(
                                CreateKanjiCompanyMasterChangeLog_After(context, changeRequest, company, CompanyChangeMessageType.Update_After));
                            break;
                        default:
                            break;
                    }
                }
                break;
            default:
                break;
        }
    }

    private CompanyChangeMessageType GetErrorType(CompanyChangeRequest changeRequest, Company? company)
    {
        if (changeRequest.IsCountExceededError)
        {
            return CompanyChangeMessageType.Duplicate_Error;
        }
        CompanyChangeRequest.ChangeActionType? changeActionType = changeRequest.GetChangeActionType();
        if (changeActionType == CompanyChangeRequest.ChangeActionType.Add && company != null)
        {
            return CompanyChangeMessageType.Add_Already_Exists;
        }
        if ((changeActionType is CompanyChangeRequest.ChangeActionType.Update or CompanyChangeRequest.ChangeActionType.Delete) && company == null)
        {
            return CompanyChangeMessageType.UpdateOrDelete_Not_Exists;
        }
        if (changeRequest.IsCombinationOrNotApplicableError)
        {
            return CompanyChangeMessageType.Combination_Or_NotApplicable_Error;
        }
        if (changeRequest.IsError())
        {
            return CompanyChangeMessageType.Item_Error;
        }
        return CompanyChangeMessageType.None;
    }

    private CompanyMasterChangeLog CreateCompanyMasterChangeLog_Common(JobContext context, CompanyChangeRequest changeRequest, CompanyChangeMessageType messageType)
    {
        return new CompanyMasterChangeLog
        {
            JobExecutionId = changeRequest.JobExecutionId,
            ProcessingDate = context.ProcessDate,
            CompanyCode = changeRequest.CompanyCode ?? string.Empty,
            RequestType = changeRequest.RequestType ?? string.Empty,
            ChangeAction = changeRequest.ChangeAction ?? string.Empty,
            MessageType = (byte)messageType
        };
    }

    private CompanyMasterChangeLog CreateCompanyMasterChangeLog_Error(
        JobContext context, CompanyChangeRequest changeRequest, CompanyChangeMessageType messageType)
    {
        CompanyMasterChangeLog changeLog = CreateCompanyMasterChangeLog_Common(context, changeRequest, messageType);
        changeLog.IsError = true;
        changeLog.SequenceNo = 1;
        return changeLog;
    }

    private CompanyMasterChangeLog CreateCompanyMasterChangeLog_Before(
        JobContext context, AppDbContext db, CompanyChangeRequest changeRequest, Company? company, CompanyChangeMessageType messageType)
    {
        if (company == null)
        {
            throw new ArgumentNullException(nameof(company), "会社マスタが存在しないため、変更前のログを作成できません。");
        }
        Company company_org = (Company)db.Entry(company).OriginalValues.ToObject();

        CompanyMasterChangeLog changeLog = CreateCompanyMasterChangeLog_Common(context, changeRequest, messageType);
        changeLog.IsError = false;
        changeLog.SequenceNo = 1;


        CompanyChangeRequestRoute route = changeRequest.GetDenpyoKubun();
        if (route == CompanyChangeRequestRoute.CompanyBasic11)
        {
            changeLog.CompanyNameKana = company_org.CompanyNameKana;
            changeLog.PhoneNumber = company_org.PhoneNumber;
            changeLog.PostalCode = company_org.PostalCode;
            changeLog.Prefecture = company_org.Prefecture;
        }
        if (route == CompanyChangeRequestRoute.CompanyAddress12)
        {
            changeLog.City = company_org.City;
            changeLog.Town1 = company_org.Town1;
            changeLog.Town2 = company_org.Town2;
        }
        if (route == CompanyChangeRequestRoute.CompanyProcessing13)
        {
            CompanyWithdrawalDay? companyWithdrawalDay1_org = GetCompanyWithdrawalDay_Org(db, company, 1);
            CompanyWithdrawalDay? companyWithdrawalDay2_org = GetCompanyWithdrawalDay_Org(db, company, 2);
            CompanyWithdrawalDay? companyWithdrawalDay3_org = GetCompanyWithdrawalDay_Org(db, company, 3);
            CompanyWithdrawalDay? companyWithdrawalDay4_org = GetCompanyWithdrawalDay_Org(db, company, 4);
            changeLog.WithdrawalDay1 = companyWithdrawalDay1_org?.WithdrawalDay;
            changeLog.WithdrawalDay2 = companyWithdrawalDay2_org?.WithdrawalDay;
            changeLog.WithdrawalDay3 = companyWithdrawalDay3_org?.WithdrawalDay;
            changeLog.WithdrawalDay4 = companyWithdrawalDay4_org?.WithdrawalDay;
            changeLog.ProcessingFlag = JoinFlags(
                company_org.ProcessingFlag1,
                company_org.ProcessingFlag2,
                company_org.ProcessingFlag3,
                company_org.ProcessingFlag4,
                company_org.ProcessingFlag5,
                company_org.ProcessingFlag6,
                company_org.ProcessingFlag7,
                company_org.ProcessingFlag8,
                company_org.ProcessingFlag9,
                company_org.ProcessingFlag10,
                company_org.ProcessingFlag11,
                company_org.ProcessingFlag12
            );
            changeLog.SheetFlag = JoinFlags(
                company_org.SheetFlag1,
                company_org.SheetFlag2,
                company_org.SheetFlag3,
                company_org.SheetFlag4,
                company_org.SheetFlag5,
                company_org.SheetFlag6,
                company_org.SheetFlag7,
                company_org.SheetFlag8,
                company_org.SheetFlag9,
                company_org.SheetFlag10,
                company_org.SheetFlag11,
                company_org.SheetFlag12
            );
        }
        if (route == CompanyChangeRequestRoute.CompanyFee14)
        {
            changeLog.ChangeCode = company_org.PageChangeKey.ToString();
            changeLog.BasicFee = company_org.BasicFee;
            changeLog.AdminFee = company_org.AdminFee;
            changeLog.NewUnitPrice = company_org.NewUnitPrice;
            changeLog.ModifyUnitPrice = company_org.ModifyUnitPrice;
            changeLog.Transfer1UnitPrice = company_org.Transfer1UnitPrice;
            changeLog.Transfer2UnitPrice = company_org.Transfer2UnitPrice;
            changeLog.ReceiptUnitPrice = company_org.ReceiptUnitPrice;
        }
        if (route == CompanyChangeRequestRoute.CompanyTransfer15)
        {
            changeLog.Transfer1BankCode = company_org.Transfer1BankCode;
            changeLog.Transfer1BranchCode = company_org.Transfer1BranchCode;
            changeLog.Transfer1AccountType = company_org.Transfer1AccountType;
            changeLog.Transfer1AccountNo = company_org.Transfer1AccountNo;
            changeLog.ConsignorCode = company_org.ConsignorCode;
            changeLog.Transfer2BankCode = company_org.Transfer2BankCode;
            changeLog.Transfer2BranchCode = company_org.Transfer2BranchCode;
            changeLog.Transfer2AccountType = company_org.Transfer2AccountType;
            changeLog.Transfer2AccountNo = company_org.Transfer2AccountNo;
            changeLog.PassbookNo = company_org.PassbookComment;
        }
        if (route == CompanyChangeRequestRoute.CompanyType16To19)
        {
            int typeNo = changeRequest.ConvertRequestTypeToTypeNo();
            CompanyType? companyType_org = GetCompanyType_Org(db, company, typeNo);
            changeLog.CollectionCode = companyType_org?.TypeNo.ToString();
            changeLog.Cycle = companyType_org?.Cycle;
            if (companyType_org != null && !string.IsNullOrWhiteSpace(companyType_org.StartYearMonth) && companyType_org.StartYearMonth.Length >= 6)
            {
                changeLog.OperatingYear = companyType_org.StartYearMonth.Substring(0, 4);
                changeLog.OperatingMonth = companyType_org.StartYearMonth.Substring(4, 2);
            }
            changeLog.ItemName = companyType_org?.TypeName;
            changeLog.Amount = companyType_org?.Amount;
        }
        return changeLog;
    }

    private static CompanyWithdrawalDay? GetCompanyWithdrawalDay_Org(
        AppDbContext db, Company company, short slotNo)
    {
        // 変更前のCompanyWithdrawalDayを取得するために、EntityStateがAddedでないものを検索
        var currentEntity = company.WithdrawalDays.FirstOrDefault(
            x => x.SlotNo == slotNo
            && db.Entry(x).State != EntityState.Added);
        if (currentEntity is null)
        {
            return null;
        }
        return (CompanyWithdrawalDay)db.Entry(currentEntity).OriginalValues.ToObject();
    }

    private static CompanyType? GetCompanyType_Org(
        AppDbContext db, Company company, int typeNo)
    {
        // 変更前のCompanyTypeを取得するために、EntityStateがAddedでないものを検索
        var currentEntity = company.Types.FirstOrDefault(
            x => x.TypeNo == typeNo
            && db.Entry(x).State != EntityState.Added);
        if (currentEntity is null)
        {
            return null;
        }
        return (CompanyType)db.Entry(currentEntity).OriginalValues.ToObject();
    }

    private CompanyMasterChangeLog CreateCompanyMasterChangeLog_After(
        JobContext context, CompanyChangeRequest changeRequest, Company? company, CompanyChangeMessageType messageType)
    {
        if (company == null)
        {
            throw new ArgumentNullException(nameof(company), "会社マスタが存在しないため、更新後のログを作成できません。");
        }

        CompanyMasterChangeLog changeLog = CreateCompanyMasterChangeLog_Common(context, changeRequest, messageType);
        changeLog.IsError = false;
        changeLog.SequenceNo = messageType == CompanyChangeMessageType.Update_After ? 2 : 1;

        CompanyChangeRequestRoute route = changeRequest.GetDenpyoKubun();

        if (route == CompanyChangeRequestRoute.CompanyBasic11)
        {
            changeLog.CompanyNameKana = company.CompanyNameKana;
            changeLog.PhoneNumber = company.PhoneNumber;
            changeLog.PostalCode = company.PostalCode;
            changeLog.Prefecture = company.Prefecture;
        }
        if (route == CompanyChangeRequestRoute.CompanyAddress12)
        {
            changeLog.City = company.City;
            changeLog.Town1 = company.Town1;
            changeLog.Town2 = company.Town2;
        }
        if (route == CompanyChangeRequestRoute.CompanyProcessing13)
        {
            CompanyWithdrawalDay? companyWithdrawalDay1 = company.WithdrawalDays.FirstOrDefault(wd => wd.SlotNo == 1);
            CompanyWithdrawalDay? companyWithdrawalDay2 = company.WithdrawalDays.FirstOrDefault(wd => wd.SlotNo == 2);
            CompanyWithdrawalDay? companyWithdrawalDay3 = company.WithdrawalDays.FirstOrDefault(wd => wd.SlotNo == 3);
            CompanyWithdrawalDay? companyWithdrawalDay4 = company.WithdrawalDays.FirstOrDefault(wd => wd.SlotNo == 4);
            changeLog.WithdrawalDay1 = companyWithdrawalDay1?.WithdrawalDay;
            changeLog.WithdrawalDay2 = companyWithdrawalDay2?.WithdrawalDay;
            changeLog.WithdrawalDay3 = companyWithdrawalDay3?.WithdrawalDay;
            changeLog.WithdrawalDay4 = companyWithdrawalDay4?.WithdrawalDay;
            changeLog.ProcessingFlag = JoinFlags(
                company.ProcessingFlag1,
                company.ProcessingFlag2,
                company.ProcessingFlag3,
                company.ProcessingFlag4,
                company.ProcessingFlag5,
                company.ProcessingFlag6,
                company.ProcessingFlag7,
                company.ProcessingFlag8,
                company.ProcessingFlag9,
                company.ProcessingFlag10,
                company.ProcessingFlag11,
                company.ProcessingFlag12
                );
            changeLog.SheetFlag = JoinFlags(
                company.SheetFlag1,
                company.SheetFlag2,
                company.SheetFlag3,
                company.SheetFlag4,
                company.SheetFlag5,
                company.SheetFlag6,
                company.SheetFlag7,
                company.SheetFlag8,
                company.SheetFlag9,
                company.SheetFlag10,
                company.SheetFlag11,
                company.SheetFlag12
                );
        }
        if (route == CompanyChangeRequestRoute.CompanyFee14)
        {
            changeLog.ChangeCode = company.PageChangeKey.ToString();
            changeLog.BasicFee = company.BasicFee;
            changeLog.AdminFee = company.AdminFee;
            changeLog.NewUnitPrice = company.NewUnitPrice;
            changeLog.ModifyUnitPrice = company.ModifyUnitPrice;
            changeLog.Transfer1UnitPrice = company.Transfer1UnitPrice;
            changeLog.Transfer2UnitPrice = company.Transfer2UnitPrice;
            changeLog.ReceiptUnitPrice = company.ReceiptUnitPrice;
        }
        if (route == CompanyChangeRequestRoute.CompanyTransfer15)
        {
            changeLog.Transfer1BankCode = company.Transfer1BankCode;
            changeLog.Transfer1BranchCode = company.Transfer1BranchCode;
            changeLog.Transfer1AccountType = company.Transfer1AccountType;
            changeLog.Transfer1AccountNo = company.Transfer1AccountNo;
            changeLog.ConsignorCode = company.ConsignorCode;
            changeLog.Transfer2BankCode = company.Transfer2BankCode;
            changeLog.Transfer2BranchCode = company.Transfer2BranchCode;
            changeLog.Transfer2AccountType = company.Transfer2AccountType;
            changeLog.Transfer2AccountNo = company.Transfer2AccountNo;
            changeLog.PassbookNo = company.PassbookComment;
        }
        if (route == CompanyChangeRequestRoute.CompanyType16To19)
        {
            int typeNo = changeRequest.ConvertRequestTypeToTypeNo();
            CompanyType? companyType = company.Types.FirstOrDefault(t => t.TypeNo == typeNo);
            changeLog.CollectionCode = companyType?.TypeNo.ToString();
            changeLog.Cycle = companyType?.Cycle;
            if (companyType != null && !string.IsNullOrWhiteSpace(companyType.StartYearMonth) && companyType.StartYearMonth.Length >= 6)
            {
                changeLog.OperatingYear = companyType.StartYearMonth.Substring(0, 4);
                changeLog.OperatingMonth = companyType.StartYearMonth.Substring(4, 2);
            }
            changeLog.ItemName = companyType?.TypeName;
            changeLog.Amount = companyType?.Amount;
        }
        return changeLog;
    }

    private KanjiCompanyMasterChangeLog CreateKanjiCompanyMasterChangeLog_Common(JobContext context, CompanyChangeRequest changeRequest, CompanyChangeMessageType messageType)
    {
        return new KanjiCompanyMasterChangeLog
        {
            JobExecutionId = changeRequest.JobExecutionId,
            ProcessingDate = context.ProcessDate,
            CompanyCode = changeRequest.CompanyCode ?? string.Empty,
            SectionNo = changeRequest.Cban ?? string.Empty,
            MessageType = messageType.ToString("D"),
            ChangeAction = changeRequest.ChangeAction ?? string.Empty,
            SequenceNo = 1
        };
    }

    private KanjiCompanyMasterChangeLog CreateKanjiCompanyMasterChangeLog_After(JobContext context, CompanyChangeRequest changeRequest, Company? company, CompanyChangeMessageType messageType)
    {
        if (company == null)
        {
            throw new ArgumentNullException(nameof(company), "会社マスタが存在しないため、更新後のログを作成できません。");
        }

        KanjiCompanyMasterChangeLog changeLog = CreateKanjiCompanyMasterChangeLog_Common(context, changeRequest, messageType);

        CompanyChangeRequestRoute route = changeRequest.GetDenpyoKubun();
        if (route == CompanyChangeRequestRoute.CompanyKanjiName71)
        {
            changeLog.CompanyNameKanji = company.CompanyNameKanji;
        }
        else if (route == CompanyChangeRequestRoute.CompanyKanjiAddress71)
        {
            changeLog.PrefectureKanji = company.PrefectureKanji;
            changeLog.CityKanji = company.CityKanji;
            changeLog.TownKanji1 = company.TownKanji1;
            changeLog.TownKanji2 = company.TownKanji2;
        }
        else if (route == CompanyChangeRequestRoute.CompanyKanjiContact71)
        {
            changeLog.DepartmentKanji = company.DepartmentKanji;
            changeLog.PersonInChargeKanji = company.PersonInChargeKanji;
        }
        return changeLog;
    }

    private void SetIfNotEmpty(string? value, Action<string> setter)
    {
        if (!string.IsNullOrEmpty(value))
        {
            setter(value);
        }
    }

    private void SetIfHasValue(decimal? value, Action<decimal> setter)
    {
        if (value.HasValue)
        {
            setter(value.Value);
        }
    }

    private string JoinFlags(params string?[] flags)
    {
        return string.Concat(flags.Select(flag => flag ?? "0"));
    }
}
