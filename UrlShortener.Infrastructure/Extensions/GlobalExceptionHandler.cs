using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
namespace UrlShortener.Infrastructure.Extensions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // 1. استخراج بيانات تتبع الخطأ
            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
            var requestPath = httpContext.Request.Path;
            var method = httpContext.Request.Method;

            // 2. تسجيل الخطأ في الـ Logger المحلي (ببيانات منظمة Structured Logging)
            _logger.LogError(exception,
                "[{TraceId}] Error occurred in {Method} {Path}: {Message}",
                traceId, method, requestPath, exception.Message);

            // 3. إرسال بيانات إضافية لـ Sentry لسهولة البحث
            SentrySdk.ConfigureScope(scope =>
            {
                scope.SetTag("traceId", traceId);
                scope.SetTag("requestPath", requestPath);
                scope.SetExtra("QueryString", httpContext.Request.QueryString.Value);
            });

            SentrySdk.CaptureException(exception);

            // 4. تحديد نوع الخطأ والـ Status Code بناءً على نوع الـ Exception
            var (statusCode, title) = exception switch
            {
                ArgumentException or InvalidOperationException => (StatusCodes.Status400BadRequest, "طلب غير صالح"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "غير مصرح بالدخول"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "المورد غير موجود"),
                _ => (StatusCodes.Status500InternalServerError, "خطأ داخلي في السيرفر")
            };

            // 5. تجهيز الرد للمستخدم (ProblemDetails)
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = statusCode == 500
                    ? $"يرجى التواصل مع الدعم الفني وتزويدهم برقم التتبع: {traceId}"
                    : exception.Message,
                Instance = requestPath
            };

            // إضافة رقم التتبع للـ Response عشان يظهر في الـ JSON
            problemDetails.Extensions.Add("traceId", traceId);

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response
                .WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}