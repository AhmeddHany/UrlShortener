using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UrlShortener.Core.Helper;

namespace UrlShortener.Core.Entities
{
    public class UrlMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // أنا المتحكم في الـ ID
        public long Id { get; set; }
        [Required]
        [MaxLength(20)]
        public string ShortCode { get; set; } = string.Empty;

        [Required]
        public string LongUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? UserId { get; set; }
        public User? User { get; set; }
        public UserPlan Plan { get; set; } = UserPlan.Free; // افتراضياً مجاني
        public string? CustomerId { get; set; } // رقم العميل في Stripe مثلاً مستقبلاً
        public int ClickCount { get; set; } = 0; // عداد سريع
        public bool IsActive { get; set; } = true;
        public DateTime? ExpiresAt { get; set; }
    }
}
