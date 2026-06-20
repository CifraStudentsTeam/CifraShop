using System.Net;
using System.Text.Json;

namespace CifraShop.API.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Необработанное исключение: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex, _env);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception, IHostEnvironment env)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                ArgumentException argEx => (HttpStatusCode.BadRequest, argEx.Message),
                KeyNotFoundException keyEx => (HttpStatusCode.NotFound, keyEx.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Доступ запрещён"),
                InvalidOperationException opEx => (HttpStatusCode.Conflict, opEx.Message),
                OverflowException ovfEx => (HttpStatusCode.BadRequest, ovfEx.Message),
                _ => (HttpStatusCode.InternalServerError, "Внутренняя ошибка сервера")
            };

            context.Response.StatusCode = (int)statusCode;

            object response = env.IsDevelopment()
                ? new { error = message, stackTrace = exception.StackTrace }
                : new { error = message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
