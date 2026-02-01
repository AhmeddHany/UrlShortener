using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using UAParser;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Interfaces;
using UrlShortener.Infrastructure.Data;
namespace UrlShortener.Infrastructure.BackgroundServices
{
    public class AnalyticsWorker : BackgroundService
    {
        private readonly IAnalyticsChannel _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _httpClientFactory;

        public AnalyticsWorker(IAnalyticsChannel channel, IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory)
        {
            _channel = channel;
            _serviceProvider = serviceProvider;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var uaParser = Parser.GetDefault();
            var client = _httpClientFactory.CreateClient();

            await foreach (var data in _channel.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<UrlDbContext>();

                    // 1. تحليل المتصفح والجهاز (من الـ UserAgent)
                    var clientInfo = uaParser.Parse(data.UserAgent);

                    // 2. تحليل البلد والمدينة (من الـ IP)
                    var (country, city) = await GetLocationAsync(client, data.IpAddress);

                    // 3. بناء الـ Entity بالكامل
                    var analyticEntry = new ClickAnalytic
                    {
                        UrlMappingId = data.UrlId,
                        ClickedAt = DateTime.UtcNow,
                        IpAddress = data.IpAddress,
                        Referrer = data.Referrer,
                        UserAgent = data.UserAgent,

                        // بيانات من الـ UA Parser
                        Browser = clientInfo.UA.Family,
                        DeviceType = clientInfo.Device.Family == "Other" ? "Desktop" : clientInfo.Device.Family,

                        // بيانات من الـ IP API
                        Country = country,
                        City = city
                    };

                    // 4. تحديث الداتابيز
                    var urlMapping = await db.UrlMappings.FindAsync(data.UrlId);
                    if (urlMapping != null)
                    {
                        urlMapping.ClickCount++;
                        db.ClickAnalytics.Add(analyticEntry);
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Worker Error]: {ex.Message}");
                }
            }
        }

        // ميثود مساعدة لجلب الموقع الجغرافي
        private async Task<(string Country, string City)> GetLocationAsync(HttpClient client, string? ip)
        {
            try
            {
                if (string.IsNullOrEmpty(ip) || ip == "::1" || ip == "127.0.0.1")
                    return ("Local", "DevMachine");

                // بنكلم API مجاني (ip-api.com)
                var response = await client.GetFromJsonAsync<IpApiResponse>($"http://ip-api.com/json/{ip}");
                return (response?.Country ?? "Unknown", response?.City ?? "Unknown");
            }
            catch { return ("Unknown", "Unknown"); }
        }
        public record IpApiResponse(
            [property: JsonPropertyName("country")] string Country,
            [property: JsonPropertyName("city")] string City
        );
    }
}
