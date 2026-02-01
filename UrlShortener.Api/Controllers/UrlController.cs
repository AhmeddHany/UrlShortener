using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using UrlShortener.Core.DTO;
using UrlShortener.Core.Interfaces;

namespace UrlShortener.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // استخدام Primary Constructor لحقن الخدمة
    public class UrlController(IUrlService urlService) : ControllerBase
    {
        [EnableRateLimiting("fixed")]
        [Authorize] // تأمين إنشاء الروابط ليكون للمستخدمين المسجلين فقط
        [HttpPost("shorten")]
        public async Task<IActionResult> Shorten([FromBody] UrlRequest request)
        {
            if (string.IsNullOrEmpty(request.LongUrl))
                return BadRequest(new { message = "The Long URL field is required." });

            // استخراج معرف المستخدم من التوكن لربط الرابط بصاحبه
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var mapping = await urlService.CreateShortUrl(request.LongUrl, userId);

            return Ok(new
            {
                ShortCode = mapping.ShortCode,
                OriginalUrl = mapping.LongUrl,
                ShortUrl = $"{Request.Scheme}://{Request.Host}/{mapping.ShortCode}",
                CreatedAt = mapping.CreatedAt
            });
        }
        [AllowAnonymous]
        [HttpGet("/{code}")]
        public async Task<IActionResult> RedirectToOriginal(string code)
        {
            // 1. استدعاء الخدمة (التي تعيد الآن كائن UrlMapping)
            var mapping = await urlService.GetOriginalUrl(code);

            if (mapping == null)
            {
                return NotFound(new { message = "The requested short code was not found." });
            }

            // 2. استخراج بيانات الزائر
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();
            var referrer = Request.Headers["Referer"].ToString();

            // 3. التتبع (استخدام mapping.Id الذي أصبح متاحاً الآن)
            _ = urlService.TrackClickAsync(mapping.Id, ip, userAgent, referrer);

            // 4. التأكد من البروتوكول والتحويل باستخدام mapping.LongUrl
            var destinationUrl = mapping.LongUrl;
            if (!destinationUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                destinationUrl = "https://" + destinationUrl;
            }

            return Redirect(destinationUrl);
        }

        [Authorize]
        [HttpGet("my-links")]
        public async Task<IActionResult> GetMyLinks()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var links = await urlService.GetUserUrlsAsync(userId!);
            return Ok(links);
        }
    }
}