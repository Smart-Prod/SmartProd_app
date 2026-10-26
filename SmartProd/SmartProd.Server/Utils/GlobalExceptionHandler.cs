using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;

namespace SmartProd.API.Server.Utils;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            ValidationException => (StatusCodes.Status400BadRequest, exception.Message),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Acesso negado."),
            _ => (StatusCodes.Status500InternalServerError, "Ocorreu um erro interno.")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        else
            logger.LogInformation(exception, "Request rejected with status {StatusCode} for {Path}", statusCode, httpContext.Request.Path);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Data = null
        };

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}
