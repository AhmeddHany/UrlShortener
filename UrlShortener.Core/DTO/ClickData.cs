namespace UrlShortener.Core.DTO
{
    public record ClickData(long UrlId, string? IpAddress, string? UserAgent, string? Referrer);
}
