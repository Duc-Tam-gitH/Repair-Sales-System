using System.Text.Json;
using FluentValidation;
using R_SS.API.Responses;
using R_SS.BLL.Exceptions;

namespace R_SS.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";

            context.Response.StatusCode = ex switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                ValidationException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message,
                Data = null,
                Errors = ex is ValidationException validationException
                    ? validationException.Errors.Select(error => error.ErrorMessage).ToList()
                    : null
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}
