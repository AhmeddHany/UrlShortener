using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    }
}
