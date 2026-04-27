using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StockFlow.Api.Extensions;

public static class ProblemDetailsExtensions
{
    private const string ProblemBaseType = "https://stockflow-api/problems/";

    public static ProblemDetails CreateProblemDetails(this HttpContext context, int status, string title, string detail, string type)
    {
        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = ProblemBaseType + type
        };

        problemDetails.Extensions["traceId"] = GetTraceId(context);
        return problemDetails;
    }

    public static ValidationProblemDetails CreateValidationProblemDetails(this HttpContext context, ModelStateDictionary modelState, string detail)
    {
        var problemDetails = new ValidationProblemDetails(modelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Detail = detail,
            Type = ProblemBaseType + "validation"
        };

        problemDetails.Extensions["traceId"] = GetTraceId(context);
        return problemDetails;
    }

    public static ValidationProblemDetails CreateValidationProblemDetails(this HttpContext context, string detail, params string[] errors)
    {
        var modelState = new ModelStateDictionary();

        foreach (var error in errors.Where(error => !string.IsNullOrWhiteSpace(error)))
        {
            modelState.AddModelError(string.Empty, error);
        }

        return context.CreateValidationProblemDetails(modelState, detail);
    }

    public static Task WriteProblemDetailsAsync(this HttpContext context, ProblemDetails problemDetails)
    {
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static string GetTraceId(HttpContext context)
        => Activity.Current?.Id ?? context.TraceIdentifier;
}
