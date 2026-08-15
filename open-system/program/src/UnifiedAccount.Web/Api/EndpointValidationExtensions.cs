using FluentValidation;

namespace UnifiedAccount.Web.Api;

/// <summary>
/// EndpointValidationExtensions を表すクラス。
/// </summary>
public static class EndpointValidationExtensions
{
    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder)
        where TRequest : class
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
            if (request is null)
                return await next(context);

            var validator = context.HttpContext.RequestServices.GetRequiredService<IValidator<TRequest>>();
            var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);

            if (result.IsValid)
                return await next(context);

            return Results.ValidationProblem(result.ToDictionary());
        });
    }

    private static IDictionary<string, string[]> ToDictionary(this FluentValidation.Results.ValidationResult result)
    {
        return result.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).Distinct().ToArray());
    }
}
