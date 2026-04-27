using Microsoft.AspNetCore.Mvc;
using StockFlow.Api.Extensions;
using StockFlow.Domain.Exceptions;

namespace StockFlow.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            LogException(exception);
            await HandleExceptionAsync(context, exception);
        }
    }

    private void LogException(Exception exception)
    {
        if (exception is DomainException)
        {
            _logger.LogInformation(exception, "Handled request failure: {ExceptionType}", exception.GetType().Name);
            return;
        }

        _logger.LogError(exception, "Unhandled exception while processing request.");
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        ProblemDetails problemDetails = exception switch
        {
            ValidationDomainException validationException => context.CreateValidationProblemDetails(validationException.Message, validationException.Message),
            InvalidUserContextException invalidUserContextException => context.CreateProblemDetails(StatusCodes.Status401Unauthorized, "Unauthorized", invalidUserContextException.Message, "unauthorized"),
            EntityNotFoundException entityNotFoundException => context.CreateProblemDetails(StatusCodes.Status404NotFound, "Resource not found", entityNotFoundException.Message, "not-found"),
            DuplicateInternalCodeException duplicateInternalCodeException => context.CreateProblemDetails(StatusCodes.Status409Conflict, "Conflict", duplicateInternalCodeException.Message, "conflict"),
            BusinessAccessDeniedException businessAccessDeniedException => context.CreateProblemDetails(StatusCodes.Status403Forbidden, "Forbidden", businessAccessDeniedException.Message, "forbidden"),
            ProductExpiredException productExpiredException => context.CreateProblemDetails(StatusCodes.Status409Conflict, "Conflict", productExpiredException.Message, "conflict"),
            ProductInactiveException productInactiveException => context.CreateProblemDetails(StatusCodes.Status409Conflict, "Conflict", productInactiveException.Message, "conflict"),
            InsufficientStockException insufficientStockException => context.CreateProblemDetails(StatusCodes.Status409Conflict, "Conflict", insufficientStockException.Message, "conflict"),
            _ => context.CreateProblemDetails(StatusCodes.Status500InternalServerError, "Internal server error", "An unexpected error occurred.", "internal-server-error")
        };

        return context.WriteProblemDetailsAsync(problemDetails);
    }
}
