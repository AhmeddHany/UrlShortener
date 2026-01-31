using Microsoft.Extensions.Caching.Distributed;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Interfaces;

namespace UrlShortener.Core.Services
{
    public class UrlService
    {
        private readonly IUrlRepository _repository;
        private readonly IDistributedCache _cache; // الإضافة هنا
        public UrlService(IUrlRepository repository, IDistributedCache cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<UrlMapping> CreateShortUrl(string longUrl)
        {
            long uniqueId = DateTime.UtcNow.Ticks;
            string shortCode = Base62Encode(uniqueId);
            var mapping = new UrlMapping
            {
                Id = uniqueId,
                LongUrl = longUrl,
                ShortCode = shortCode
            };
            await _repository.AddAsync(mapping);
            await _cache.SetStringAsync(shortCode, longUrl, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            });
            return mapping;
        }

        public async Task<string?> GetOriginalUrl(string code)
        {
            var cachedUrl = await _cache.GetStringAsync(code);
            if (!string.IsNullOrEmpty(cachedUrl))
            {
                return cachedUrl;
            }
            var mapping = await _repository.GetByCodeAsync(code);
            if (mapping != null)
            {
                await _cache.SetStringAsync(code, mapping.LongUrl, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                });

                return mapping.LongUrl;
            }
            return null;
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
    }
}
