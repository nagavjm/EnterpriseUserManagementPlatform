using System.Net;
using System.Text.Json;
using EnterpriseUserManagement.Application.Common;
using EnterpriseUserManagement.Domain.Exceptions;

namespace EnterpriseUserManagement.Api.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate _next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        this._next = _next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            DomainValidationException validationEx => (
                (int)HttpStatusCode.BadRequest,
                validationEx.Message,
                new List<string> { validationEx.Message }
            ),
            UserNotFoundException notFoundEx => (
                (int)HttpStatusCode.NotFound,
                notFoundEx.Message,
                new List<string> { notFoundEx.Message }
            ),
            DuplicateEmailException conflictEx => (
                (int)HttpStatusCode.Conflict,
                conflictEx.Message,
                new List<string> { conflictEx.Message }
            ),
            StorageCorruptedException storageEx => (
                (int)HttpStatusCode.InternalServerError,
                "Storage failure: the data store could not be read or is corrupted.",
                new List<string> { storageEx.Message }
            ),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                "An unexpected server error occurred. Please try again later.",
                new List<string> { "An internal server error occurred." }
            )
        };

        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled server error processing HTTP {Method} {Path}: {Message}", 
                context.Request.Method, context.Request.Path, exception.Message);
        }
        else
        {
            _logger.LogWarning("Handled application exception for HTTP {Method} {Path} [Status {StatusCode}]: {Message}", 
                context.Request.Method, context.Request.Path, statusCode, exception.Message);
        }

        context.Response.StatusCode = statusCode;

        var response = ApiResponse<object>.Fail(message, errors);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

public static class GlobalExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}
