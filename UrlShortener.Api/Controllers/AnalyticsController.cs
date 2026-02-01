using Microsoft.AspNetCore.Mvc;
using UrlShortener.Core.Interfaces;

namespace UrlShortener.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController(IAnalyticsService analyticsService) : ControllerBase
    {
        // GET: api/analytics/{urlId}
        [HttpGet("{urlId}")]
        public async Task<IActionResult> GetUrlStats(int urlId)
        {
            // بنادي الخدمة اللي عملناها في الانفرا
            var stats = await analyticsService.GetLinkStatsAsync(urlId);

            if (stats == null)
            {
                return NotFound(new { message = "Stats not found for this URL." });
            }

            return Ok(stats);
        }

        [HttpGet("top-countries/{urlId}")]
        public async Task<IActionResult> GetTopCountries(int urlId)
        {
            var countries = await analyticsService.GetTopCountriesAsync(urlId);
            return Ok(countries);
        }
    }
}
