namespace UrlShortener.Core.Entities
{
    public class ApiErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public string TraceId { get; set; }
        public List<string> Errors { get; set; } = new();

        public ApiErrorResponse(int statusCode, string message, string errorCode, string traceId)
        {
            StatusCode = statusCode;
            Message = message;
            ErrorCode = errorCode;
            TraceId = traceId;
        }
    }
}
