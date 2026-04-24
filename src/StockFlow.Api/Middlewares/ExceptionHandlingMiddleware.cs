using System.Net;
using System.Text.Json;
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
            _logger.LogError(exception, "Unhandled exception while processing request.");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            ValidationDomainException => HttpStatusCode.BadRequest,
            EntityNotFoundException => HttpStatusCode.NotFound,
            DuplicateInternalCodeException => HttpStatusCode.Conflict,
            BusinessAccessDeniedException => HttpStatusCode.Forbidden,
            ProductExpiredException => HttpStatusCode.Conflict,
            ProductInactiveException => HttpStatusCode.Conflict,
            InsufficientStockException => HttpStatusCode.Conflict,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = JsonSerializer.Serialize(new
        {
            message = statusCode == HttpStatusCode.InternalServerError ? "An unexpected error occurred." : exception.Message
        });

        return context.Response.WriteAsync(payload);
    }
}
