using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Core.DTO.Auth
{
    public class RegisterDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "الباسورد لازم يكون 6 أرقام أو حروف على الأقل")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "الباسورد غير متطابق")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
