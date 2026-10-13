using Core.CrossCuttingConcerns.Exceptions.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Core.CrossCuttingConcerns.Exceptions.Extensions;

public static class ApplicationBuilderExceptionMiddlewareExtensions
{
    public static void ConfigureCustomExceptionMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
    }
}
