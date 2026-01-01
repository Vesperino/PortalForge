using PortalForge.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace PortalForge.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        catch (CustomException ex)
        {
            _logger.LogError(ex, "Custom Exception: {Message}", ex.Message);
            await HandleExceptionSafeAsync(context, ex);
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogError(ex, "FluentValidation Exception: {Message}", ex.Message);

            var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();
            var exception = new CustomException(
                "Validation failed",
                errors,
                HttpStatusCode.BadRequest);

            await HandleExceptionSafeAsync(context, exception);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt: {Message}", ex.Message);

            var exception = new CustomException(
                "Access denied. You do not have permission to perform this action.",
                statusCode: HttpStatusCode.Forbidden);
            await HandleExceptionSafeAsync(context, exception);
        }
        catch (Exception ex)
        {
            // Log full exception details for debugging, but never expose to client
            var correlationId = context.TraceIdentifier;

            if (ex.InnerException != null)
            {
                _logger.LogError(ex.InnerException,
                    "Unhandled Exception (Inner) [CorrelationId: {CorrelationId}]: {Message}",
                    correlationId, ex.InnerException.Message);
            }
            else
            {
                _logger.LogError(ex,
                    "Unhandled Exception [CorrelationId: {CorrelationId}]: {Message}",
                    correlationId, ex.Message);
            }

            // Return generic error message to client - never expose internal exception details
            var exception = new CustomException(
                "An unexpected error occurred. Please try again later.",
                statusCode: HttpStatusCode.InternalServerError);
            await HandleExceptionSafeAsync(context, exception);
        }
    }

    private async Task HandleExceptionSafeAsync(HttpContext context, CustomException ex)
    {
        try
        {
            await HandleExceptionAsync(context, ex);
        }
        catch (Exception handlerEx)
        {
            _logger.LogCritical(handlerEx, "FATAL: Error handler failed for exception: {OriginalMessage}", ex.Message);
            await WriteFallbackErrorAsync(context);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, CustomException ex)
    {
        var code = ex.StatusCode;

        // Get correlation ID from context
        var correlationId = context.TraceIdentifier;

        var errorResponse = new
        {
            error = ex.Message,
            statusCode = (int)code,
            correlationId = correlationId,
            timestamp = DateTime.UtcNow,
            path = context.Request.Path.Value,
            errors = ex.ErrorMessages ?? new List<string>()
        };

        var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }

    private static async Task WriteFallbackErrorAsync(HttpContext context)
    {
        try
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\":\"Internal server error\",\"statusCode\":500}");
            }
        }
        catch
        {
            // Last resort - do nothing if even fallback fails
        }
    }
}
