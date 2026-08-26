using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SoundMate.API.Filters;

/// <summary>
/// Runs the FluentValidation validator for every action argument that has one, and short-circuits
/// with a 400 listing every bad field at once.
/// <para>
/// This is the job Agendia gives to a MediatR <c>ValidationBehavior</c>. SoundMate has no MediatR,
/// and <c>FluentValidation.AspNetCore</c> — which used to do this automatically — stopped at 11.3.1
/// and never followed FluentValidation 12, so it is abandoned. Hence a filter: registered once,
/// globally, so no endpoint has to remember to validate.
/// </para>
/// </summary>
public sealed class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Resolved per request, not injected: validators are registered scoped, and a filter
        // instance outlives the scope it runs in.
        var services = context.HttpContext.RequestServices;

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (services.GetService(validatorType) is not IValidator validator)
                continue;

            var result = await validator.ValidateAsync(
                new ValidationContext<object>(argument),
                context.HttpContext.RequestAborted);

            if (result.IsValid)
                continue;

            foreach (var failure in result.Errors)
                context.ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);

            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState));
            return;
        }

        await next();
    }
}
