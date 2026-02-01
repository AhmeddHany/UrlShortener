using System.Net;
using System.Text.Json;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Exceptions;

namespace UrlShortener.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
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
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var traceId = context.TraceIdentifier;
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred. Please try again later.";
            var errorCode = "INTERNAL_SERVER_ERROR";

            // 1. التحقق إذا كان الخطأ مخصصاً (Business Exception)
            if (exception is BaseException baseEx)
            {
                statusCode = baseEx.StatusCode;
                message = baseEx.Message;
                errorCode = baseEx.ErrorCode;

                _logger.LogWarning("Business Exception: {ErrorCode} - {Message} [TraceId: {TraceId}]",
                    errorCode, message, traceId);
            }
            else
            {
                // 2. خطأ غير متوقع (Technical Exception)
                _logger.LogError(exception, "Unhandled Exception: {Message} [TraceId: {TraceId}]",
                    exception.Message, traceId);
            }

            // 3. بناء الاستجابة
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new ApiErrorResponse(statusCode, message, errorCode, traceId);

            // إضافة تفاصيل الخطأ في بيئة التطوير فقط
            if (_env.IsDevelopment())
            {
                response.Errors.Add($"Exception: {exception.GetType().Name}");
                response.Errors.Add($"Detail: {exception.Message}");
                response.Errors.Add($"StackTrace: {exception.StackTrace}");
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            await context.Response.WriteAsJsonAsync(response, jsonOptions);
        }
    }
}