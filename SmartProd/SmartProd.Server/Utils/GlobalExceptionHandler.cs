using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
            ConflictException => (StatusCodes.Status409Conflict, exception.Message),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "O registro foi alterado por outra operação. Recarregue os dados e tente novamente."),
            DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } } =>
                (StatusCodes.Status409Conflict, "Um registro com os mesmos valores únicos já existe."),
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
