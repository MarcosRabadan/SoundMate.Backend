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
            UserNotFoundException e =>
                (StatusCodes.Status404NotFound, "User not found", e.Message),

            AcademyNotFoundException e =>
                (StatusCodes.Status404NotFound, "Academy not found", e.Message),

            UserProfileNotFoundException e =>
                (StatusCodes.Status404NotFound, "User profile not found", e.Message),

            DisciplineNotFoundException e =>
                (StatusCodes.Status404NotFound, "Discipline not found", e.Message),

            // 404 and not the one above: "you do not study this" is a different answer from
            // "there is no such discipline", and a caller needs to know whether to offer an add
            // button or to fix its selector.
            StudiedDisciplineNotFoundException e =>
                (StatusCodes.Status404NotFound, "Studied discipline not found", e.Message),

            EmailAlreadyRegisteredException e =>
                (StatusCodes.Status409Conflict, "Email already registered", e.Message),

            SlugAlreadyTakenException e =>
                (StatusCodes.Status409Conflict, "Slug already taken", e.Message),

            UserStillHasMembershipsException e =>
                (StatusCodes.Status409Conflict, "User still has memberships", e.Message),

            AcademyStillHasMembersException e =>
                (StatusCodes.Status409Conflict, "Academy still has members", e.Message),

            DisciplineAlreadyAddedException e =>
                (StatusCodes.Status409Conflict, "Discipline already added", e.Message),

            // 409, not 404: the id is real and the caller is not confused about it — the
            // catalogue just stopped offering it. Whoever already studies it is unaffected.
            DisciplineNotAvailableException e =>
                (StatusCodes.Status409Conflict, "Discipline not available", e.Message),

            // 409, not 404: the academy is there and the id is valid, the operation just conflicts
            // with the state it is in — and the message says which operation was wanted instead.
            AcademyIsDeletedException e =>
                (StatusCodes.Status409Conflict, "Academy is deleted", e.Message),

            // 400, not 401: the caller is not failing to authenticate, they are supplying a wrong
            // value in a field of a request. SoundMate has no authentication to fail yet anyway.
            IncorrectPasswordException e =>
                (StatusCodes.Status400BadRequest, "Incorrect password", e.Message),

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
