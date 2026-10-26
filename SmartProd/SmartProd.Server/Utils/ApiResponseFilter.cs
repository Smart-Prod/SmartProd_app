using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

namespace SmartProd.API.Server.Utils;

public sealed class ApiResponseFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value is not null &&
            !IsAlreadyWrapped(objectResult.Value))
        {
            var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
            var succeeded = statusCode < StatusCodes.Status400BadRequest;
            objectResult.Value = new ApiResponse<object>
            {
                Success = succeeded,
                Message = succeeded ? ReadMessage(objectResult.Value, "Message") : ReadMessage(objectResult.Value, "Error", "Title", "Message"),
                Data = succeeded ? objectResult.Value : null
            };
        }

        await next();
    }

    private static bool IsAlreadyWrapped(object value) =>
        value.GetType().IsGenericType &&
        value.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>);

    private static string? ReadMessage(object value, params string[] names)
    {
        foreach (var name in names)
        {
            var property = value.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property?.GetValue(value) is string message && !string.IsNullOrWhiteSpace(message))
                return message;
        }

        return null;
    }
}
