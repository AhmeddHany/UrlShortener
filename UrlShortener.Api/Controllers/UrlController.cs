using Microsoft.AspNetCore.Mvc;
using UrlShortener.Core.DTO;
using UrlShortener.Core.Services;

namespace UrlShortener.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly UrlService _service;

        public UrlController(UrlService service)
        {
            _service = service;
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> Shorten([FromBody] UrlRequest request)
        {
            if (string.IsNullOrEmpty(request.LongUrl))
                return BadRequest("URL cannot be empty");
            var mapping = await _service.CreateShortUrl(request.LongUrl);
            return Ok(new
            {
                ShortCode = mapping.ShortCode,
                OriginalUrl = mapping.LongUrl,
                ShortUrl = $"{Request.Scheme}://{Request.Host}/{mapping.ShortCode}"
            });
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> RedirectToOriginal(string code)
        {
            var originalUrl = await _service.GetOriginalUrl(code);
            if (string.IsNullOrEmpty(originalUrl))
                return NotFound(new { message = "Short code not found or expired" });

            return Redirect(originalUrl);
        }
    }
}
