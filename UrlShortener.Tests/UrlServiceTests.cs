using Microsoft.Extensions.Caching.Distributed;
using Moq;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Interfaces;
using UrlShortener.Core.Services;
using Xunit;

namespace UrlShortener.Tests
{
    public class UrlServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUrlRepository> _urlRepoMock;
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<IAnalyticsChannel> _analyticsChannelMock; // أضف هذا الـ Mock
        private readonly UrlService _urlService;
        public UrlServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _urlRepoMock = new Mock<IUrlRepository>();
            _cacheMock = new Mock<IDistributedCache>();
            _analyticsChannelMock = new Mock<IAnalyticsChannel>(); // تهيئة الـ Mock

            _unitOfWorkMock.Setup(u => u.Urls).Returns(_urlRepoMock.Object);

            // حقن الباراميتر الثالث الجديد هنا
            _urlService = new UrlService(_unitOfWorkMock.Object, _cacheMock.Object, _analyticsChannelMock.Object);
        }

        [Fact]
        public async Task GetOriginalUrl_ShouldReturnMappingFromDb_WhenCacheMisses()
        {
            // Arrange
            string code = "testCode";
            var mapping = new UrlMapping { Id = 1, ShortCode = code, LongUrl = "https://google.com" };

            // محاكاة عدم وجود بيانات في الكاش
            _cacheMock.Setup(x => x.GetAsync(code, default)).ReturnsAsync((byte[]?)null);

            // محاكاة وجود البيانات في قاعدة البيانات
            _urlRepoMock.Setup(x => x.GetByCodeAsync(code)).ReturnsAsync(mapping);

            // Act
            var result = await _urlService.GetOriginalUrl(code);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mapping.LongUrl, result.LongUrl);
            Assert.Equal(mapping.Id, result.Id);
        }

        [Fact]
        public async Task CreateShortUrl_ShouldSaveToDbAndCompleteTransaction()
        {
            // Arrange
            var longUrl = "https://www.google.com";
            var userId = "user-123";

            // Act
            var result = await _urlService.CreateShortUrl(longUrl, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(longUrl, result.LongUrl);
            Assert.Equal(userId, result.UserId);

            // التأكد من إضافة الرابط واستدعاء CompleteAsync لحفظ التغييرات
            _urlRepoMock.Verify(r => r.AddAsync(It.IsAny<UrlMapping>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);

            // التأكد من تحديث الكاش
            _cacheMock.Verify(c => c.SetAsync(
                result.ShortCode,
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                default), Times.Once);
        }
    }
}