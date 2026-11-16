using BookingPlatformInsiders.Application.Common.Exceptions;
using BookingPlatformInsiders.Domain.Bookings;
using BookingPlatformInsiders.Domain.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BookingPlatformInsiders.ErrorHandling;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            ValidationException validationException => CreateValidationProblem(validationException),
            _ => CreateProblem(exception)
        };

        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}.",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateValidationProblem(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).ToArray());

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred."
        };

        problemDetails.Extensions["code"] = "validation.failed";
        problemDetails.Extensions["errors"] = errors;

        return problemDetails;
    }

    private static ProblemDetails CreateProblem(Exception exception)
    {
        var (status, code, title) = Map(exception);

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title
        };

        problemDetails.Extensions["code"] = code;

        return problemDetails;
    }

    private static (int Status, string Code, string Title) Map(Exception exception) =>
        exception switch
        {
            RoomNotFoundException useCaseException =>
                (StatusCodes.Status404NotFound, useCaseException.Code, useCaseException.Message),
            BookingNotFoundException useCaseException =>
                (StatusCodes.Status404NotFound, useCaseException.Code, useCaseException.Message),
            ConcurrencyConflictException useCaseException =>
                (StatusCodes.Status409Conflict, useCaseException.Code, useCaseException.Message),
            BookingOverlapException domainException =>
                (StatusCodes.Status409Conflict, domainException.Code, domainException.Message),
            BookingCannotCancelException domainException =>
                (StatusCodes.Status409Conflict, domainException.Code, domainException.Message),
            BookingCannotCompleteException domainException =>
                (StatusCodes.Status409Conflict, domainException.Code, domainException.Message),
            BookingStartInPastException domainException =>
                (StatusCodes.Status400BadRequest, domainException.Code, domainException.Message),
            DomainException domainException =>
                (StatusCodes.Status400BadRequest, domainException.Code, domainException.Message),
            _ => (StatusCodes.Status500InternalServerError, "server.error", "An unexpected error occurred.")
        };
}
