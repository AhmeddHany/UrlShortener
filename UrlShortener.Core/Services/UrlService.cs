using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using UrlShortener.Core.DTO;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Interfaces;

namespace UrlShortener.Core.Services
{
    public class UrlService(IUnitOfWork unitOfWork, IDistributedCache cache, IAnalyticsChannel analyticsChannel) : IUrlService
    {
        public async Task<UrlMapping> CreateShortUrl(string longUrl, string userId)
        {
            long uniqueId = DateTime.UtcNow.Ticks;
            string shortCode = Base62Encode(uniqueId);

            var mapping = new UrlMapping
            {
                LongUrl = longUrl,
                ShortCode = shortCode,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await unitOfWork.Urls.AddAsync(mapping);
            await unitOfWork.CompleteAsync();

            await cache.SetStringAsync(shortCode, longUrl, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            });

            return mapping;
        }

        public async Task<UrlMapping?> GetOriginalUrl(string code)
        {
            // 1. محاولة الجلب من الكاش
            var cachedData = await cache.GetStringAsync(code);

            if (!string.IsNullOrEmpty(cachedData))
            {
                try
                {
                    return JsonSerializer.Deserialize<UrlMapping>(cachedData);
                }
                catch (JsonException)
                {
                    await cache.RemoveAsync(code);
                }
            }

            var mapping = await unitOfWork.Urls.GetByCodeAsync(code);

            if (mapping != null)
            {
                // 3. تخزين الكائن كاملاً كـ JSON
                var jsonMapping = JsonSerializer.Serialize(mapping);
                await cache.SetStringAsync(code, jsonMapping, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                });
                return mapping;
            }

            return null;
        }
        // تنفيذ الدالة التي كانت ناقصة لجلب روابط المستخدم
        public async Task<List<UrlMapping>> GetUserUrlsAsync(string userId)
        {
            return await unitOfWork.Urls.GetAllByUserIdAsync(userId);
        }

        private string Base62Encode(long value)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            string result = "";
            do
            {
                result = chars[(int)(value % 62)] + result;
                value /= 62;
            } while (value > 0);
            return result;
        }
        public async Task TrackClickAsync(long urlId, string? ip, string? ua, string? referrer)
        {
            var clickData = new ClickData(urlId, ip, ua, referrer);
            await analyticsChannel.WriteClickAsync(clickData);
        }
    }
}