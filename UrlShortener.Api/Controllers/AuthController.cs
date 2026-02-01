using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Core.DTO.Auth;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Interfaces;

namespace UrlShortener.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // لاحظ هنا اسم البراميتر: userManager
    public class AuthController(UserManager<User> userManager, IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var user = new User { UserName = model.Email, Email = model.Email };

            // التصحيح: شيلنا الـ (_) من اسم userManager
            var result = await userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "تم إنشاء الحساب بنجاح!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);

            if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = authService.GenerateJwtToken(user.Id, user.Email!);
                return Ok(new { Token = token });
            }

            return Unauthorized(new { message = "الإيميل أو الباسورد غلط" });
        }
    }
}