using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace OnlineBookAdventures.Api.Middleware;

/// <summary>
/// Catches unhandled exceptions and returns structured RFC 7807 ProblemDetails responses.
/// </summary>
public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning("Validation failed: {Errors}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, "Validation Failed",
                ex.Errors.Select(e => e.ErrorMessage).ToArray());
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Resource not found: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.NotFound, "Not Found", [ex.Message]);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Unauthorized: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.Forbidden, "Forbidden", [ex.Message]);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Invalid operation: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.Conflict, "Conflict", [ex.Message]);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "Internal Server Error",
                ["An unexpected error occurred."]);
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string title,
        string[] errors)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Type = $"https://httpstatuses.com/{(int)statusCode}"
        };

        if (errors.Length > 0)
            problem.Extensions["errors"] = errors;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
            .ConfigureAwait(false);
    }
}
