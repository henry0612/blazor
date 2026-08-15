using FluentValidation;
using UnifiedAccount.Application.DTOs;

namespace UnifiedAccount.Web.Validation;

/// <summary>
/// ログインリクエスト バリデーター
/// </summary>
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("ユーザーIDは必須です。")
            .MaximumLength(20).WithMessage("ユーザーIDは20文字以内で入力してください。");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("パスワードは必須です。");
    }
}

