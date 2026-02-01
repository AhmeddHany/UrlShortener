namespace UrlShortener.Core.Exceptions
{
    public abstract class BaseException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }

        protected BaseException(string message, string errorCode, int statusCode) : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }

    // استثناء عند عدم وجود مورد (404)
    public class NotFoundException : BaseException
    {
        public NotFoundException(string message, string errorCode = "RESOURCE_NOT_FOUND")
            : base(message, errorCode, 404) { }
    }

    // استثناء عند وجود بيانات خاطئة أو غير صالحة (400)
    public class BadRequestException : BaseException
    {
        public BadRequestException(string message, string errorCode = "BAD_REQUEST")
            : base(message, errorCode, 400) { }
    }

    // استثناء خاص بالصلاحيات (401)
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message, string errorCode = "UNAUTHORIZED_ACCESS")
            : base(message, errorCode, 401) { }
    }
}