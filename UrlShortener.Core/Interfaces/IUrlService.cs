using UrlShortener.Core.Entities;

namespace UrlShortener.Core.Interfaces
{
    public interface IUrlService
    {
        // يتطلب معرف المستخدم لربط الرابط بصاحبه
        Task<UrlMapping> CreateShortUrl(string longUrl, string userId);

        // لجلب الرابط الأصلي عند التحويل
        Task<UrlMapping?> GetOriginalUrl(string code);

        // لجلب كافة روابط مستخدم معين
        Task<List<UrlMapping>> GetUserUrlsAsync(string userId);
        Task TrackClickAsync(long urlMappingId, string? ipAddress, string? userAgent, string? referrer);
    }
}
