using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;
using UnifiedAccount.Domain.Enums;
using UnifiedAccount.Domain.Services;

namespace UnifiedAccount.Domain.Entities.Changes;

/// <summary>
/// 会社異動リクエスト (伝区11-19, 71-CBAN 1桁)
/// </summary>
[Table("TD_CompanyChangeRequests")]
[Index(nameof(RequestType))]
[Index(nameof(Cban))]
[Index(nameof(CompanyCode))]
[Index(nameof(JobExecutionId), nameof(BatchStatus))]
public class CompanyChangeRequest : BaseEntity
{
    [Required, MaxLength(2)]
    public string RequestType { get; set; } = string.Empty;

    [MaxLength(1)]
    /// <summary>
    /// 照合番号を取得または設定する。
    /// </summary>
    public string? Cban { get; set; }

    [Required, MaxLength(1)]
    public string ChangeAction { get; set; } = string.Empty;

    [MaxLength(6)] public string? CompanyCode { get; set; }
    [MaxLength(40)] public string? CompanyName { get; set; }
    [MaxLength(13)] public string? PhoneNumber { get; set; }
    [MaxLength(7)] public string? PostalCode { get; set; }
    [MaxLength(9)] public string? Prefecture { get; set; }
    [MaxLength(20)] public string? City { get; set; }
    [MaxLength(24)] public string? Town1 { get; set; }
    [MaxLength(24)] public string? Town2 { get; set; }
    [MaxLength(2)] public string? WithdrawalDay1 { get; set; }
    [MaxLength(2)] public string? WithdrawalDay2 { get; set; }
    [MaxLength(2)] public string? WithdrawalDay3 { get; set; }
    [MaxLength(2)] public string? WithdrawalDay4 { get; set; }
    [MaxLength(1)] public string? ProcessingFlag1 { get; set; }
    [MaxLength(1)] public string? ProcessingFlag2 { get; set; }
    [MaxLength(1)] public string? ProcessingFlag3 { get; set; }
    [MaxLength(1)] public string? ProcessingFlag4 { get; set; }
    [MaxLength(1)] public string? ProcessingFlag5 { get; set; }
    [MaxLength(1)] public string? ProcessingFlag6 { get; set; }
    [MaxLength(1)] public string? ProcessingFlag7 { get; set; }
    [MaxLength(1)] public string? ProcessingFlag8 { get; set; }
    [MaxLength(1)] public string? ProcessingFlag9 { get; set; }
    [MaxLength(1)] public string? ProcessingFlag10 { get; set; }
    [MaxLength(1)] public string? ProcessingFlag11 { get; set; }
    [MaxLength(1)] public string? ProcessingFlag12 { get; set; }
    [MaxLength(1)] public string? SheetFlag1 { get; set; }
    [MaxLength(1)] public string? SheetFlag2 { get; set; }
    [MaxLength(1)] public string? SheetFlag3 { get; set; }
    [MaxLength(1)] public string? SheetFlag4 { get; set; }
    [MaxLength(1)] public string? SheetFlag5 { get; set; }
    [MaxLength(1)] public string? SheetFlag6 { get; set; }
    [MaxLength(1)] public string? SheetFlag7 { get; set; }
    [MaxLength(1)] public string? SheetFlag8 { get; set; }
    [MaxLength(1)] public string? SheetFlag9 { get; set; }
    [MaxLength(1)] public string? SheetFlag10 { get; set; }
    [MaxLength(1)] public string? SheetFlag11 { get; set; }
    [MaxLength(1)] public string? SheetFlag12 { get; set; }
    [MaxLength(100)] public string? ProcessingFlagsJson { get; set; }
    [MaxLength(100)] public string? ReportFlagsJson { get; set; }
    [Precision(6, 0)] public decimal? BasicFee { get; set; }
    [Precision(6, 0)] public decimal? AdminFee { get; set; }
    [Precision(2, 0)] public decimal PageChangeKey { get; set; }
    [Precision(4, 0)] public decimal? NewUnitPrice { get; set; }
    [Precision(4, 0)] public decimal? ModifyUnitPrice { get; set; }
    [Precision(4, 0)] public decimal? Transfer1UnitPrice { get; set; }
    [Precision(4, 0)] public decimal? Transfer2UnitPrice { get; set; }
    [Precision(4, 0)] public decimal? ReceiptUnitPrice { get; set; }
    [MaxLength(4)] public string? Transfer1BankCode { get; set; }
    [MaxLength(3)] public string? Transfer1BranchCode { get; set; }
    [MaxLength(1)] public string? Transfer1AccountType { get; set; }
    [MaxLength(10)] public string? Transfer1AccountNo { get; set; }
    [MaxLength(4)] public string? Transfer2BankCode { get; set; }
    [MaxLength(3)] public string? Transfer2BranchCode { get; set; }
    [MaxLength(1)] public string? Transfer2AccountType { get; set; }
    [MaxLength(10)] public string? Transfer2AccountNo { get; set; }
    [MaxLength(8)] public string? PassbookComment { get; set; }
    [MaxLength(10)] public string? ConsignorCode { get; set; }
    [MaxLength(1)] public string? TypeCategory { get; set; }
    [MaxLength(2)] public string? Cycle { get; set; }
    [MaxLength(4)] public string? OperatingYear { get; set; }
    [MaxLength(2)] public string? OperatingMonth { get; set; }
    [MaxLength(12)] public string? TypeName { get; set; }
    [Precision(8, 0)] public decimal? Amount { get; set; }
    [MaxLength(30)] public string? CompanyNameKanji { get; set; }
    [MaxLength(4)] public string? PrefectureKanji { get; set; }
    [MaxLength(10)] public string? CityKanji { get; set; }
    [MaxLength(20)] public string? TownKanji1 { get; set; }
    [MaxLength(20)] public string? TownKanji2 { get; set; }
    [MaxLength(20)] public string? DepartmentKanji { get; set; }
    [MaxLength(20)] public string? PersonInChargeKanji { get; set; }

    public bool IsRequiredOrFormatError { get; set; }
    public bool IsApplied { get; set; }
    public bool IsUnitPriceOrOperatingYearMonthError { get; set; }
    public bool IsModifyUnitPriceOrAmountError { get; set; }
    public bool IsTransfer1UnitPriceError { get; set; }
    public bool IsTransfer2UnitPriceError { get; set; }
    public bool IsReceiptUnitPriceError { get; set; }
    public bool IsChangeActionError { get; set; }
    public bool IsCombinationOrNotApplicableError { get; set; }
    public bool IsCountExceededError { get; set; }

    [Required, MaxLength(50)]
    public string JobExecutionId { get; set; } = string.Empty;

    [Required, MaxLength(16)]
    public string BatchStatus { get; set; } = "SUCCESS";

    [MaxLength(20)]
    /// <summary>
    /// 失敗実行者プログラムを取得または設定する。
    /// </summary>
    public string? FailedByProgram { get; set; }

    [MaxLength(4)]
    /// <summary>
    /// 失敗理由コードを取得または設定する。
    /// </summary>
    public string? FailedReasonCode { get; set; }

    /// <summary>
    /// 失敗日時を取得または設定する。
    /// </summary>
    public DateTime? FailedAt { get; set; }

    public int Version { get; set; } = 1;

    /// <summary>
    /// RequestType, Cban から伝票区分を取得する。
    /// </summary>
    public CompanyChangeRequestRoute GetDenpyoKubun()
    {
        return CompanyChangeRequestRouter.Resolve(RequestType, Cban);
    }

    /// <summary>
    /// ChangeActionの列挙型
    /// </summary>
    public enum ChangeActionType
    {
        Delete = 1,
        Add = 2,
        Update = 3
    }

    /// <summary>
    /// ChangeActionを列挙型に変換して取得する。変換できない場合はnullを返す。
    /// </summary>
    public ChangeActionType? GetChangeActionType()
    {
        return Enum.TryParse<ChangeActionType>(ChangeAction, out var changeActionType)
            && Enum.IsDefined(changeActionType)
                ? changeActionType
                : null;
    }

    /// <summary>
    /// Cbanの列挙型
    /// </summary>
    public enum CbanType
    {

        /// <summary>会社名（漢字）</summary>
        CompanyNameKanji = 1,

        /// <summary>住所（漢字）</summary>
        AddressKanji = 2,

        /// <summary>部署名・担当者名</summary>
        Department_PersonKanji = 3
    }

    /// <summary>
    /// TypeCategoryの列挙型
    /// </summary>
    public enum TypeCategoryType
    {
        /// <summary>なし</summary>
        None = 0,
        /// <summary>変動金額</summary>
        VariableAmount = 1,
        /// <summary>固定金額（契約者単位）</summary>
        FixedAmountPerContractor = 2,
        /// <summary>固定金額（委託会社単位）</summary>
        FixedAmountPerConsignor = 3
    }

    /// <summary>
    /// TypeCategoryを列挙型に変換して取得する。変換できない場合はnullを返す。
    /// </summary>
    public TypeCategoryType? GetTypeCategoryType()
    {
        return Enum.TryParse<TypeCategoryType>(TypeCategory, out var typeCategoryType)
            && Enum.IsDefined(typeCategoryType)
                ? typeCategoryType
                : null;
    }

    /// <summary>
    /// 必須・書式エラーをチェックする。
    /// </summary>
    public Boolean CheckIsRequiredOrFormatError()
    {
        CompanyChangeRequestRoute denpyoKubun = GetDenpyoKubun();
        if (denpyoKubun == CompanyChangeRequestRoute.Unsupported)
        {
            return true;
        }

        ChangeActionType? changeActionType = GetChangeActionType();
        if (changeActionType == ChangeActionType.Delete)
        {
            return false; // 削除の場合はチェックをスキップ
        }

        // 必須チェック
        switch (denpyoKubun)
        {
            case CompanyChangeRequestRoute.CompanyBasic11:
                if (changeActionType == ChangeActionType.Add && string.IsNullOrEmpty(CompanyName))
                {
                    return true;
                }
                break;
            case CompanyChangeRequestRoute.CompanyProcessing13:
                if (changeActionType == ChangeActionType.Add && (
                    string.IsNullOrEmpty(WithdrawalDay1)
                    && string.IsNullOrEmpty(WithdrawalDay2)
                    && string.IsNullOrEmpty(WithdrawalDay3)
                    && string.IsNullOrEmpty(WithdrawalDay4)))
                {
                    return true;
                }
                break;
            case CompanyChangeRequestRoute.CompanyType16To19:
                if ((!string.IsNullOrEmpty(OperatingYear) && string.IsNullOrEmpty(OperatingMonth))
                || (string.IsNullOrEmpty(OperatingYear) && !string.IsNullOrEmpty(OperatingMonth)))
                {
                    return true;
                }
                break;
            case CompanyChangeRequestRoute.CompanyKanjiName71:
                if (string.IsNullOrEmpty(CompanyNameKanji))
                {
                    return true;
                }
                break;
            case CompanyChangeRequestRoute.CompanyKanjiAddress71:
                if (string.IsNullOrEmpty(PrefectureKanji)
                || string.IsNullOrEmpty(CityKanji))
                {
                    return true;
                }
                break;
            default:
                break;
        }

        // 数値チェック
        switch (denpyoKubun)
        {
            case CompanyChangeRequestRoute.CompanyProcessing13:
                if ((!string.IsNullOrEmpty(WithdrawalDay1) && !ulong.TryParse(WithdrawalDay1, out _))
                || (!string.IsNullOrEmpty(WithdrawalDay2) && !ulong.TryParse(WithdrawalDay2, out _))
                || (!string.IsNullOrEmpty(WithdrawalDay3) && !ulong.TryParse(WithdrawalDay3, out _))
                || (!string.IsNullOrEmpty(WithdrawalDay4) && !ulong.TryParse(WithdrawalDay4, out _)))
                {
                    return true;
                }
                break;

            /* Decimal型のためエラーとならない。チェックはコメントアウト
            case CompanyChangeRequestRoute.CompanyFee14:

                if ((!MoneyAmount.TryCreate(BasicFee, out _))
                || (!MoneyAmount.TryCreate(AdminFee, out _))
                || (!MoneyAmount.TryCreate(NewUnitPrice, out _))
                || (!MoneyAmount.TryCreate(ModifyUnitPrice, out _))
                || (!MoneyAmount.TryCreate(Transfer1UnitPrice, out _))
                || (!MoneyAmount.TryCreate(Transfer2UnitPrice, out _))
                || (!MoneyAmount.TryCreate(ReceiptUnitPrice, out _)))
                {
                    return true;
                }
                break;
            */
            case CompanyChangeRequestRoute.CompanyType16To19:
                if ((!string.IsNullOrEmpty(Cycle) && !ulong.TryParse(Cycle, out _))
                || (!string.IsNullOrEmpty(OperatingYear) && !ulong.TryParse(OperatingYear, out _))
                || (!string.IsNullOrEmpty(OperatingMonth) && !ulong.TryParse(OperatingMonth, out _)))
                {
                    return true;
                }
                /* Decimal型のためエラーとならない。チェックはコメントアウト
                if (!MoneyAmount.TryCreate(Amount, out _))
                {
                    return true;
                }
                */
                break;
            default:
                break;
        }

        // 値域チェック
        switch (denpyoKubun)
        {
            case CompanyChangeRequestRoute.CompanyFee14:
                if (!short.TryParse(PageChangeKey.ToString(), out var pageChangeKey) || pageChangeKey >= 19)
                {
                    return true;
                }
                break;
            case CompanyChangeRequestRoute.CompanyType16To19:
                if (GetTypeCategoryType() is null)
                {
                    return true;
                }
                break;
            default:
                break;
        }
        return false;
    }

    /// <summary>
    /// NewUnitPrice数値/年月エラーをチェックする。
    /// </summary>
    public Boolean CheckIsUnitPriceOrOperatingYearMonthError()
    {

        ChangeActionType? changeActionType = GetChangeActionType();
        if (changeActionType == ChangeActionType.Delete)
        {
            return false; // 削除の場合はチェックをスキップ
        }

        CompanyChangeRequestRoute denpyoKubun = GetDenpyoKubun();

        switch (denpyoKubun)
        {
            /* Decimal型のためエラーとならない。チェックはコメントアウト
            case CompanyChangeRequestRoute.CompanyFee14:
                if (!MoneyAmount.TryCreate(NewUnitPrice, out _))
                {
                    return true;
                }
                break;
            */
            case CompanyChangeRequestRoute.CompanyType16To19:
                if ((!string.IsNullOrEmpty(Cycle) && !ValueObjects.Cycle.TryCreate(Cycle, out _))
                 || (!string.IsNullOrEmpty(OperatingMonth) && !ValueObjects.OperatingMonth.TryCreate(OperatingMonth, out _)))
                {
                    return true;
                }
                break;
            default:
                break;
        }
        return false;
    }

    /// <summary>
    /// ModifyUnitPrice、Amount数値エラーをチェックする。
    /// </summary>
    public Boolean CheckIsModifyUnitPriceOrAmountError()
    {

        /* Decimal型のためエラーとならない。チェックはコメントアウト
        ChangeActionType? changeActionType = GetChangeActionType();
        if (changeActionType == ChangeActionType.Delete)
        {
            return false; // 削除の場合はチェックをスキップ
        }

        CompanyChangeRequestRoute denpyoKubun = GetDenpyoKubun();
        
        switch (denpyoKubun)
        {
            case CompanyChangeRequestRoute.CompanyFee14:
                if (!MoneyAmount.TryCreate(ModifyUnitPrice, out _))
                {
                    return true;
                }
                break;
            case CompanyChangeRequestRoute.CompanyType16To19:
                if (!MoneyAmount.TryCreate(Amount, out _))
                {
                    return true;
                }
                break;
            default:
                break;
        }
        */
        return false;
    }

    /// <summary>
    /// Transfer1UnitPrice数値エラーをチェックする。
    /// </summary>
    public Boolean CheckIsTransfer1UnitPriceError()
    {

        /* Decimal型のためエラーとならない。チェックはコメントアウト
        ChangeActionType? changeActionType = GetChangeActionType();
        if (changeActionType == ChangeActionType.Delete)
        {
            return false; // 削除の場合はチェックをスキップ
        }

        CompanyChangeRequestRoute denpyoKubun = GetDenpyoKubun();
        
        switch (denpyoKubun)
        {
            case CompanyChangeRequestRoute.CompanyFee14:
                if (!MoneyAmount.TryCreate(Transfer1UnitPrice, out _))
                {
                    return true;
                }
                break;
            default:
                break;
        }
        */
        return false;
    }

    /// <summary>
    /// Transfer2UnitPrice数値エラーをチェックする。
    /// </summary>
    public Boolean CheckIsTransfer2UnitPriceError()
    {

        /* Decimal型のためエラーとならない。チェックはコメントアウト
        ChangeActionType? changeActionType = GetChangeActionType();
        if (changeActionType == ChangeActionType.Delete)
        {
            return false; // 削除の場合はチェックをスキップ
        }

        CompanyChangeRequestRoute denpyoKubun = GetDenpyoKubun();
        
        switch (denpyoKubun)
        {
            case CompanyChangeRequestRoute.CompanyFee14:
                if (!MoneyAmount.TryCreate(Transfer2UnitPrice, out _))
                {
                    return true;
                }
                break;
            default:
                break;
        }
        */
        return false;
    }

    /// <summary>
    /// ReceiptUnitPrice数値エラーをチェックする。
    /// </summary>
    public Boolean CheckIsReceiptUnitPriceError()
    {

        /* Decimal型のためエラーとならない。チェックはコメントアウト
        ChangeActionType? changeActionType = GetChangeActionType();
        if (changeActionType == ChangeActionType.Delete)
        {
            return false; // 削除の場合はチェックをスキップ
        }
        
        CompanyChangeRequestRoute denpyoKubun = GetDenpyoKubun();
        
        switch (denpyoKubun)
        {
            case CompanyChangeRequestRoute.CompanyFee14:
                if (!MoneyAmount.TryCreate(ReceiptUnitPrice, out _))
                {
                    return true;
                }
                break;
            default:
                break;
        }
        */
        return false;
    }

    /// <summary>
    /// 異動区分エラーをチェックする。
    /// </summary>
    public Boolean CheckIsChangeActionError()
    {
        return GetChangeActionType() is null;
    }

    /// <summary>
    /// エラーのいずれかに該当したらTrueを返す。
    /// </summary>
    public Boolean IsError()
    {
        return IsRequiredOrFormatError
        || IsUnitPriceOrOperatingYearMonthError
        || IsModifyUnitPriceOrAmountError
        || IsTransfer1UnitPriceError
        || IsTransfer2UnitPriceError
        || IsReceiptUnitPriceError
        || IsChangeActionError
        || IsCombinationOrNotApplicableError
        || IsCountExceededError;
    }

    public int ConvertRequestTypeToTypeNo()
    {
        CompanyChangeRequestRoute denpyoKubun = GetDenpyoKubun();
        if (denpyoKubun == CompanyChangeRequestRoute.CompanyType16To19)
        {
            return int.Parse(RequestType) - 15; // RequestTypeが16～19の場合、typeNoは1～4
        }
        return 0;
    }
}
