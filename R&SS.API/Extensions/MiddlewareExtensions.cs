using R_SS.API.Middleware;

namespace R_SS.API.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalException(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}
