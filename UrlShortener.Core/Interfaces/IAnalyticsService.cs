using UrlShortener.Core.DTO;

namespace UrlShortener.Core.Interfaces
{
    public interface IAnalyticsService
    {
        // الحصول على إحصائيات عامة للينك معين
        Task<LinkStatsDto> GetLinkStatsAsync(int urlId);

        // الحصول على أكثر البلاد زيارة
        Task<List<CountryStatDto>> GetTopCountriesAsync(int urlId);
    }
}
