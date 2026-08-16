using System.Net;
using System.Text.Json;

namespace WatchListScreening.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
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
            _logger.LogError(ex, "Bir hata oluştu: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var (statusCode, message) = exception switch
        {
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, "İstenen kaynak bulunamadı."),
            ArgumentException => ((int)HttpStatusCode.BadRequest, exception.Message),
            _ => ((int)HttpStatusCode.InternalServerError, "Sunucu tarafında beklenmeyen bir hata oluştu.")
        };

        context.Response.StatusCode = statusCode;

        var response = new { Success = false, Message = message };
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await context.Response.WriteAsync(jsonResponse);
    }
}
