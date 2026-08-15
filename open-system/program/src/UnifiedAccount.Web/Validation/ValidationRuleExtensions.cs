using FluentValidation;

namespace UnifiedAccount.Web.Validation;

internal static class ValidationRuleExtensions
{
    public static IRuleBuilderOptions<T, string> RequiredCode<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        int maxLength,
        string fieldName)
    {
        return ruleBuilder
            .NotEmpty().WithMessage($"{fieldName}は必須です。")
            .MaximumLength(maxLength).WithMessage($"{fieldName}は{maxLength}文字以内である必要があります。");
    }

    public static IRuleBuilderOptions<T, string> RequiredDigits<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        int length,
        string fieldName)
    {
        return ruleBuilder
            .NotEmpty().WithMessage($"{fieldName}は必須です。")
            .Length(length).WithMessage($"{fieldName}は{length}桁である必要があります。")
            .Matches("^[0-9]+$").WithMessage($"{fieldName}は数字のみで入力してください。");
    }

    public static IRuleBuilderOptions<T, string?> OptionalMaxLength<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        int maxLength,
        string fieldName)
    {
        return ruleBuilder
            .MaximumLength(maxLength)
            .WithMessage($"{fieldName}は{maxLength}文字以内である必要があります。");
    }

    public static IRuleBuilderOptions<T, string?> OptionalDigits<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        int length,
        string fieldName)
    {
        return ruleBuilder
            .Length(length).WithMessage($"{fieldName}は{length}桁である必要があります。")
            .Matches("^[0-9]+$").WithMessage($"{fieldName}は数字のみで入力してください。");
    }
}
