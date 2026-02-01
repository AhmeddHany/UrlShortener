using Microsoft.AspNetCore.Mvc;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Interfaces;
using UrlShortener.Core.Services;
using UrlShortener.Infrastructure.Data;

namespace UrlShortener.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : Controller
    {
        private readonly UrlService _service;

        public TestController(UrlService service)
        {
            _service = service;
        }
        [HttpPost("stress")]
        public async Task<IActionResult> Stress([FromServices] IServiceScopeFactory scopeFactory)
        {
            // سنستخدم معرف مستخدم افتراضي لعملية اختبار الضغط
            string dummyUserId = "system-stress-test";

            var tasks = Enumerable.Range(0, 10000).Select(async i =>
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    // ملاحظة: يفضل دائماً استخدام الواجهة IUrlService بدلاً من الكلاس المباشر
                    var service = scope.ServiceProvider.GetRequiredService<IUrlService>();

                    // تمرير الرابط ومعرف المستخدم معاً
                    await service.CreateShortUrl("https://example.com/" + i, dummyUserId);
                }
            });

            await Task.WhenAll(tasks);
            return Ok("Completed 10,000 requests successfully.");
        }
        [HttpPost("stress-bulk")]
        public async Task<IActionResult> StressBulk([FromServices] IServiceScopeFactory scopeFactory)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<UrlDbContext>();

                for (int i = 0; i < 10000; i++)
                {
                    long uniqueId = DateTime.UtcNow.Ticks + i; // إضافة i لمنع التكرار
                    db.UrlMappings.Add(new UrlMapping
                    {
                        Id = uniqueId,
                        LongUrl = "https://example.com/" + i,
                        ShortCode = Base62Encode(uniqueId)
                    });

                    // كل 1000 سجل، ابعتهم "خبطة واحدة" للداتابيز
                    if (i % 1000 == 0)
                    {
                        await db.SaveChangesAsync();
                    }
                }
                await db.SaveChangesAsync(); // الباقي
            }
            return Ok("Done in Bulk!");
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
