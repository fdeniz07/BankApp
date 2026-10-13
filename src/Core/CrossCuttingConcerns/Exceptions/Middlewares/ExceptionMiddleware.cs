using System.Net.Mime;
using Core.CrossCuttingConcerns.Exceptions.Handlers;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Microsoft.AspNetCore.Http;

namespace Core.CrossCuttingConcerns.Exceptions.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HttpExceptionHandler _httpExceptionHandler;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
        _httpExceptionHandler = new HttpExceptionHandler();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context.Response, exception);
        }
    }

    private Task HandleExceptionAsync(HttpResponse response, Exception exception)
    {
        response.ContentType = MediaTypeNames.Application.Json;
        _httpExceptionHandler.Response = response;

        return exception switch
        {
            BusinessException businessException => _httpExceptionHandler.HandleException(businessException),
            ValidationException validationException => _httpExceptionHandler.HandleException(validationException),
            AuthorizationException authorizationException => _httpExceptionHandler.HandleException(authorizationException),
            NotFoundException notFoundException => _httpExceptionHandler.HandleException(notFoundException),
            _ => _httpExceptionHandler.HandleException(exception)
        };
    }
}
