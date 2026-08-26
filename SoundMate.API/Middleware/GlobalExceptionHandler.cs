using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SoundMate.Application.Common.Exceptions;
using SoundMate.Domain.Common;

namespace SoundMate.API.Middleware;

/// <summary>
/// Turns the exceptions the lower layers throw into HTTP answers.
/// <para>
/// Without this a <c>DomainException</c> — "that email is not a valid format" — reaches the caller
/// as a 500, which says the server is broken when the request was. The catch-all case is the
/// opposite concern: an unexpected exception must NOT put its message on the wire, because that is
/// where connection strings and stack traces leak from.
/// </para>
/// </summary>
internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService,
                                  ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
                                                Exception exception,
                                                CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            EmailAlreadyRegisteredException e =>
                (StatusCodes.Status409Conflict, "Email already registered", e.Message),

            ValidationException e =>
                (StatusCodes.Status400BadRequest, "Validation failed", e.Message),

            DomainException e =>
                (StatusCodes.Status400BadRequest, "Invalid request", e.Message),

            // Deliberately generic. The real message goes to the log, not to the caller.
            _ => (StatusCodes.Status500InternalServerError,
                  "An unexpected error occurred",
                  "The request could not be completed.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception on {Method} {Path}",
                             httpContext.Request.Method, httpContext.Request.Path);
        else
            _logger.LogInformation("{ExceptionType} answered as {Status}: {Message}",
                                   exception.GetType().Name, status, exception.Message);

        httpContext.Response.StatusCode = status;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
            }
        });
    }
}
