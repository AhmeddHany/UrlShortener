using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Text;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Interfaces;
using UrlShortener.Core.Services;
using Xunit;

namespace UrlShortener.Tests
{
    public class UrlServiceTests
    {
        private readonly Mock<IUrlRepository> _repositoryMock;
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly UrlService _urlService;

        public UrlServiceTests()
        {
            _repositoryMock = new Mock<IUrlRepository>();
            _cacheMock = new Mock<IDistributedCache>();
            _urlService = new UrlService(_repositoryMock.Object, _cacheMock.Object);
        }

        [Fact]
        public async Task GetOriginalUrl_ShouldReturnFromCache_WhenKeyExists()
        {
            // Arrange
            string code = "testCode";
            string expectedUrl = "https://google.com";
            var encodedUrl = Encoding.UTF8.GetBytes(expectedUrl);

            // بنخلي الـ Mock يرجع الـ bytes كأنها جاية من Redis
            _cacheMock.Setup(x => x.GetAsync(code, default))
                      .ReturnsAsync(encodedUrl);

            // Act
            var result = await _urlService.GetOriginalUrl(code);

            // Assert
            Assert.Equal(expectedUrl, result);
            // نتأكد إننا مروحناش للداتابيز لأننا لقيناها في الكاش
            _repositoryMock.Verify(x => x.GetByCodeAsync(It.IsAny<string>()), Times.Never);
        }
        [Fact]
        public async Task CreateShortUrl_ShouldSaveToDbAndCache_WhenUrlIsValid()
        {
            // 1. Arrange (التجهيز)
            var longUrl = "https://www.google.com";

            // 2. Act (التنفيذ)
            var result = await _urlService.CreateShortUrl(longUrl);
            Assert.NotNull(result);
            Assert.Equal(longUrl, result.LongUrl);

            // ب- نتأكد إن الـ ShortCode اتولد فعلاً (مش null ولا فاضي)
            Assert.False(string.IsNullOrWhiteSpace(result.ShortCode));

            // ج- نتأكد إن الخدمة نادت الداتابيز عشان تحفظ (Verify)
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<UrlMapping>()), Times.Once);

            // د- نتأكد إن الخدمة حطت الرابط في الكاش فوراً (Write-through Cache)
            _cacheMock.Verify(c => c.SetAsync(
                result.ShortCode,
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                default),
            Times.Once);
        }
    }
}
