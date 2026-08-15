using FluentValidation;
using UnifiedAccount.Web.Api;

namespace UnifiedAccount.Web.Validation;

/// <summary>
/// CreateCompanyRequestValidator を表すクラス。
/// </summary>
public sealed class CreateCompanyRequestValidator : AbstractValidator<CompanyEndpoints.CreateCompanyRequest>
{
    public CreateCompanyRequestValidator()
    {
        RuleFor(x => x.CompanyCode).RequiredCode(6, "会社コード");
        RuleFor(x => x.CompanyNameKana).NotEmpty().WithMessage("会社名カナは必須です。").MaximumLength(36).WithMessage("会社名カナは36文字以内である必要があります。");
        RuleFor(x => x.CompanyNameKanji).OptionalMaxLength(30, "会社名漢字").When(x => x.CompanyNameKanji is not null);
        RuleFor(x => x.ConsignorCode).RequiredCode(10, "委託者コード");
        RuleFor(x => x.PostalCode).OptionalMaxLength(7, "郵便番号").When(x => x.PostalCode is not null);
        RuleFor(x => x.Prefecture).OptionalMaxLength(9, "都道府県").When(x => x.Prefecture is not null);
        RuleFor(x => x.PhoneNumber).OptionalMaxLength(13, "電話番号").When(x => x.PhoneNumber is not null);
        RuleFor(x => x.BasicFee).InclusiveBetween(0m, 9999999999m).WithMessage("基本料金は0以上 9999999999以下で入力してください。");
        RuleFor(x => x.AdminFee).InclusiveBetween(0m, 9999999999m).WithMessage("管理料金は0以上 9999999999以下で入力してください。");
    }
}

/// <summary>
/// UpdateCompanyRequestValidator を表すクラス。
/// </summary>
public sealed class UpdateCompanyRequestValidator : AbstractValidator<CompanyEndpoints.UpdateCompanyRequest>
{
    public UpdateCompanyRequestValidator()
    {
        RuleFor(x => x.CompanyNameKana).NotEmpty().WithMessage("会社名カナを更新する場合は空文字にできません。").MaximumLength(36).WithMessage("会社名カナは36文字以内である必要があります。").When(x => x.CompanyNameKana is not null);
        RuleFor(x => x.CompanyNameKanji).OptionalMaxLength(30, "会社名漢字").When(x => x.CompanyNameKanji is not null);
        RuleFor(x => x.ConsignorCode).NotEmpty().WithMessage("委託者コードを更新する場合は必須です。").MaximumLength(10).WithMessage("委託者コードは10文字以内である必要があります。").When(x => x.ConsignorCode is not null);
        RuleFor(x => x.PostalCode).OptionalMaxLength(7, "郵便番号").When(x => x.PostalCode is not null);
        RuleFor(x => x.Prefecture).OptionalMaxLength(9, "都道府県").When(x => x.Prefecture is not null);
        RuleFor(x => x.PhoneNumber).OptionalMaxLength(13, "電話番号").When(x => x.PhoneNumber is not null);
        RuleFor(x => x.BasicFee!.Value).InclusiveBetween(0m, 9999999999m).WithMessage("基本料金は0以上 9999999999以下で入力してください。").When(x => x.BasicFee.HasValue);
        RuleFor(x => x.AdminFee!.Value).InclusiveBetween(0m, 9999999999m).WithMessage("管理料金は0以上 9999999999以下で入力してください。").When(x => x.AdminFee.HasValue);
    }
}

/// <summary>
/// CreateContractRequestValidator を表すクラス。
/// </summary>
public sealed class CreateContractRequestValidator : AbstractValidator<ContractEndpoints.CreateContractRequest>
{
    public CreateContractRequestValidator()
    {
        RuleFor(x => x.CompanyCode).RequiredCode(6, "会社コード");
        RuleFor(x => x.PersonalCode).RequiredCode(12, "個人コード");
        RuleFor(x => x.CheckDigit).NotEmpty().WithMessage("チェックデジットは必須です。").MaximumLength(1).WithMessage("チェックデジットは1文字である必要があります。");
        RuleFor(x => x.DepositorNameKana).NotEmpty().WithMessage("預金者名カナは必須です。").MaximumLength(32).WithMessage("預金者名カナは32文字以内である必要があります。");
        RuleFor(x => x.DepositorNameKanji).OptionalMaxLength(20, "預金者名漢字").When(x => x.DepositorNameKanji is not null);
        RuleFor(x => x.BankCode).RequiredDigits(4, "金融機関コード");
        RuleFor(x => x.BranchCode).RequiredDigits(3, "支店コード");
        RuleFor(x => x.AccountType).NotEmpty().WithMessage("預金種目は必須です。").MaximumLength(1).WithMessage("預金種目は1文字である必要があります。");
        RuleFor(x => x.AccountNo).NotEmpty().WithMessage("口座番号は必須です。").MaximumLength(10).WithMessage("口座番号は10文字以内である必要があります。").Matches("^[0-9]+$").WithMessage("口座番号は数字のみで入力してください。");
        RuleFor(x => x.WithdrawalDay).InclusiveBetween((short)1, (short)31).WithMessage("振替日は1から31の範囲で入力してください。");
        RuleFor(x => x.CurrentBillingAmount).InclusiveBetween(0m, 9999999999m).WithMessage("今回請求額は0以上 9999999999以下で入力してください。");
    }
}

/// <summary>
/// UpdateContractRequestValidator を表すクラス。
/// </summary>
public sealed class UpdateContractRequestValidator : AbstractValidator<ContractEndpoints.UpdateContractRequest>
{
    public UpdateContractRequestValidator()
    {
        RuleFor(x => x.DepositorNameKana).NotEmpty().WithMessage("預金者名カナを更新する場合は空文字にできません。").MaximumLength(32).WithMessage("預金者名カナは32文字以内である必要があります。").When(x => x.DepositorNameKana is not null);
        RuleFor(x => x.DepositorNameKanji).OptionalMaxLength(20, "預金者名漢字").When(x => x.DepositorNameKanji is not null);
        RuleFor(x => x.BankCode).NotEmpty().WithMessage("金融機関コードを更新する場合は必須です。").Length(4).WithMessage("金融機関コードは4桁である必要があります。").Matches("^[0-9]+$").WithMessage("金融機関コードは数字のみで入力してください。").When(x => x.BankCode is not null);
        RuleFor(x => x.BranchCode).NotEmpty().WithMessage("支店コードを更新する場合は必須です。").Length(3).WithMessage("支店コードは3桁である必要があります。").Matches("^[0-9]+$").WithMessage("支店コードは数字のみで入力してください。").When(x => x.BranchCode is not null);
        RuleFor(x => x.AccountType).NotEmpty().WithMessage("預金種目を更新する場合は必須です。").MaximumLength(1).WithMessage("預金種目は1文字である必要があります。").When(x => x.AccountType is not null);
        RuleFor(x => x.AccountNo).NotEmpty().WithMessage("口座番号を更新する場合は必須です。").MaximumLength(10).WithMessage("口座番号は10文字以内である必要があります。").Matches("^[0-9]+$").WithMessage("口座番号は数字のみで入力してください。").When(x => x.AccountNo is not null);
        RuleFor(x => x.WithdrawalDay!.Value).InclusiveBetween((short)1, (short)31).WithMessage("振替日は1から31の範囲で入力してください。").When(x => x.WithdrawalDay.HasValue);
        RuleFor(x => x.CurrentBillingAmount!.Value).InclusiveBetween(0m, 9999999999m).WithMessage("今回請求額は0以上 9999999999以下で入力してください。").When(x => x.CurrentBillingAmount.HasValue);
        RuleFor(x => x.SuspendFlag).Matches("^[01]$").WithMessage("停止フラグは 0 または 1 で入力してください。").When(x => x.SuspendFlag is not null);
    }
}

/// <summary>
/// CreateBankBranchRequestValidator を表すクラス。
/// </summary>
public sealed class CreateBankBranchRequestValidator : AbstractValidator<BankBranchEndpoints.CreateBankBranchRequest>
{
    public CreateBankBranchRequestValidator()
    {
        RuleFor(x => x.BankCode).RequiredDigits(4, "金融機関コード");
        RuleFor(x => x.BranchCode).RequiredDigits(3, "支店コード");
        RuleFor(x => x.BankNameKana).NotEmpty().WithMessage("銀行名カナは必須です。").MaximumLength(15).WithMessage("銀行名カナは15文字以内である必要があります。");
        RuleFor(x => x.BranchNameKana).NotEmpty().WithMessage("支店名カナは必須です。").MaximumLength(15).WithMessage("支店名カナは15文字以内である必要があります。");
        RuleFor(x => x.BankNameKanji).OptionalMaxLength(15, "銀行名漢字").When(x => x.BankNameKanji is not null);
        RuleFor(x => x.BranchNameKanji).OptionalMaxLength(15, "支店名漢字").When(x => x.BranchNameKanji is not null);
    }
}

/// <summary>
/// UpdateBankBranchRequestValidator を表すクラス。
/// </summary>
public sealed class UpdateBankBranchRequestValidator : AbstractValidator<BankBranchEndpoints.UpdateBankBranchRequest>
{
    public UpdateBankBranchRequestValidator()
    {
        RuleFor(x => x.BankNameKana)
            .NotEmpty().WithMessage("銀行名カナは必須です。")
            .MaximumLength(15).WithMessage("銀行名カナは15文字以内である必要があります。");
        RuleFor(x => x.BranchNameKana)
            .NotEmpty().WithMessage("支店名カナは必須です。")
            .MaximumLength(15).WithMessage("支店名カナは15文字以内である必要があります。");
        RuleFor(x => x.BankNameKanji)
            .OptionalMaxLength(15, "銀行名漢字");
        RuleFor(x => x.BranchNameKanji)
            .OptionalMaxLength(15, "支店名漢字");
        RuleFor(x => x.OriginalUpdatedAt)
            .NotEqual(default(DateTime)).WithMessage("更新日時は必須です。");
    }
}

/// <summary>
/// UpdateCalendarRequestValidator を表すクラス。
/// </summary>
public sealed class UpdateCalendarRequestValidator : AbstractValidator<CalendarEndpoints.UpdateCalendarRequest>
{
    public UpdateCalendarRequestValidator()
    {
        RuleFor(x => x.ProcessingType).NotEmpty().WithMessage("処理種別を更新する場合は空文字にできません。").MaximumLength(1).WithMessage("処理種別は1文字である必要があります。").When(x => x.ProcessingType is not null);
    }
}

/// <summary>
/// CreateCalendarRequestValidator を表すクラス。
/// </summary>
public sealed class CreateCalendarRequestValidator : AbstractValidator<CalendarEndpoints.CreateCalendarRequest>
{
    public CreateCalendarRequestValidator()
    {
        RuleFor(x => x.ProcessingDate).NotEqual(default(DateOnly)).WithMessage("処理日を指定してください。");
    }
}

/// <summary>
/// CopyCalendarRequestValidator を表すクラス。
/// </summary>
public sealed class CopyCalendarRequestValidator : AbstractValidator<CalendarEndpoints.CopyCalendarRequest>
{
    public CopyCalendarRequestValidator()
    {
        RuleFor(x => x.SourceYear).InclusiveBetween(1900, 2100).WithMessage("コピー元年は1900から2100の範囲で入力してください。");
        RuleFor(x => x.SourceMonth).InclusiveBetween(1, 12).WithMessage("コピー元月は1から12の範囲で入力してください。");
    }
}

/// <summary>
/// ApplyTemplateRequestValidator を表すクラス。
/// </summary>
public sealed class ApplyTemplateRequestValidator : AbstractValidator<CalendarEndpoints.ApplyTemplateRequest>
{
    public ApplyTemplateRequestValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(1900, 2100).WithMessage("対象年は1900から2100の範囲で入力してください。");
        RuleFor(x => x.Month).InclusiveBetween(1, 12).WithMessage("対象月は1から12の範囲で入力してください。");
        RuleFor(x => x.TemplateType).NotEmpty().WithMessage("テンプレート種別は必須です。").MaximumLength(20).WithMessage("テンプレート種別は20文字以内である必要があります。");
    }
}

/// <summary>
/// SendRequestValidator を表すクラス。
/// </summary>
public sealed class SendRequestValidator : AbstractValidator<ZenginEndpoints.SendRequest>
{
    public SendRequestValidator()
    {
        RuleFor(x => x.WithdrawalDate).NotEqual(default(DateOnly)).WithMessage("振替日を指定してください。");
        RuleFor(x => x.ConsignorCode).NotEmpty().WithMessage("委託者コードを指定する場合は空文字にできません。").MaximumLength(10).WithMessage("委託者コードは10文字以内である必要があります。").When(x => x.ConsignorCode is not null);
    }
}

/// <summary>
/// ReceiveRequestValidator を表すクラス。
/// </summary>
public sealed class ReceiveRequestValidator : AbstractValidator<ZenginEndpoints.ReceiveRequest>
{
    public ReceiveRequestValidator()
    {
        RuleFor(x => x.WithdrawalDate).NotEqual(default(DateOnly)).WithMessage("振替日を指定してください。");
    }
}
