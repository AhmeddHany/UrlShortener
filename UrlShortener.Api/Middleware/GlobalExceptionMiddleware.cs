using FluentValidation; // سنحتاجها في خطوة التحقق القادمة
using System.Net;
using System.Text.Json;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Exceptions;

namespace UrlShortener.Api.Middleware
{
    /// <summary>
    /// Middleware مركزي لالتقاط ومعالجة كافة الاستثناءات في نظام اختصار الروابط.
    /// يضمن توحيد شكل الرد البرمجي (JSON) وحماية البيانات الحساسة في بيئة الإنتاج.
    /// </summary>
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
                // تمرير الطلب إلى المكون التالي في خط الأنابيب (Pipeline)
                await _next(context);
            }
            catch (Exception ex)
            {
                // في حال حدوث أي خطأ، يتم تحويل التحكم لميثود المعالجة
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // إعداد المتغيرات الافتراضية (خطأ سيرفر داخلي 500)
            var traceId = context.TraceIdentifier;
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred. Please try again later.";
            var errorCode = "INTERNAL_SERVER_ERROR";
            var validationErrors = new List<string>();

            // 1. تصنيف الاستثناء وتحديد الرد المناسب بناءً على النوع
            switch (exception)
            {
                // أخطاء منطق العمل المخصصة (400, 404, 401)
                case BaseException baseEx:
                    statusCode = baseEx.StatusCode;
                    message = baseEx.Message;
                    errorCode = baseEx.ErrorCode;
                    _logger.LogWarning("Business Exception: {ErrorCode} - {Message} [TraceId: {TraceId}]",
                        errorCode, message, traceId);
                    break;

                // أخطاء التحقق من البيانات (FluentValidation)
                case ValidationException valEx:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = "One or more validation errors occurred.";
                    errorCode = "VALIDATION_ERROR";
                    validationErrors = valEx.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.LogWarning("Validation failed: {Message} [TraceId: {TraceId}]", message, traceId);
                    break;

                // أي استثناء غير متوقع (Technical/Uncaught Exception)
                default:
                    _logger.LogError(exception, "Unhandled Exception: {Message} [TraceId: {TraceId}]",
                        exception.Message, traceId);
                    break;
            }

            // 2. إعداد رأس الاستجابة (HTTP Response Header)
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            // 3. بناء كائن الرد النهائي
            var response = new ApiErrorResponse(statusCode, message, errorCode, traceId);

            // إضافة أخطاء التحقق إن وجدت
            if (validationErrors.Any())
            {
                response.Errors.AddRange(validationErrors);
            }

            // إضافة تفاصيل برمجية دقيقة في بيئة التطوير فقط (Development)
            if (_env.IsDevelopment())
            {
                response.Errors.Add($"Exception Type: {exception.GetType().Name}");
                response.Errors.Add($"Source: {exception.Source}");
                response.Errors.Add($"Stack Trace: {exception.StackTrace}");
            }

            // 4. تحويل الكائن إلى JSON وإرساله
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true // لجعل الـ JSON مقروءاً بشكل أفضل في المتصفح
            };

            await context.Response.WriteAsJsonAsync(response, jsonOptions);
        }
    }
}