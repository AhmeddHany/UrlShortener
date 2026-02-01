using Microsoft.EntityFrameworkCore;
using UrlShortener.Core.DTO; // تأكد إن الفولدر اسمه DTO مش DTOs
using UrlShortener.Core.Interfaces;
using UrlShortener.Infrastructure.Data;

namespace UrlShortener.Infrastructure.Services
{
    // الـ Primary Constructor بيتكتب مباشرة بعد اسم الكلاس كده
    public class AnalyticsService(UrlDbContext context) : IAnalyticsService
    {
        public async Task<LinkStatsDto> GetLinkStatsAsync(int urlId)
        {
            var analytics = context.ClickAnalytics.Where(x => x.UrlMappingId == urlId);

            return new LinkStatsDto
            {
                TotalClicks = await analytics.CountAsync(),

                LastClickedAt = await analytics
                    .OrderByDescending(x => x.ClickedAt)
                    .Select(x => x.ClickedAt)
                    .FirstOrDefaultAsync(),

                TopCountries = await GetTopCountriesAsync(urlId),

                DeviceBreakdown = await analytics
                    .GroupBy(x => x.DeviceType)
                    .Select(g => new DeviceStatDto
                    {
                        DeviceType = g.Key ?? "Unknown",
                        Count = g.Count()
                    }).ToListAsync(),

                ClicksLast7Days = await analytics
                    .Where(x => x.ClickedAt >= DateTime.UtcNow.AddDays(-7))
                    .GroupBy(x => x.ClickedAt.Date)
                    .Select(g => new DailyClickDto
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync()
            };
        }

        public async Task<List<CountryStatDto>> GetTopCountriesAsync(int urlId)
        {
            return await context.ClickAnalytics
                .Where(x => x.UrlMappingId == urlId)
                .GroupBy(x => x.Country)
                .Select(g => new CountryStatDto
                {
                    CountryName = g.Key ?? "Unknown",
                    ClickCount = g.Count()
                })
                .OrderByDescending(x => x.ClickCount)
                .Take(5)
                .ToListAsync();
        }
    }
}