namespace UrlShortener.Core.Entities
{
    public class ClickAnalytic
    {
        public int Id { get; set; }
        public long UrlMappingId { get; set; }
        public DateTime ClickedAt { get; set; } = DateTime.UtcNow;

        // بيانات العميل
        public string? IpAddress { get; set; }
        public string? Country { get; set; }   // هنجيبها من الـ IP
        public string? City { get; set; }      // هنجيبها من الـ IP

        public string? Referrer { get; set; }  // الموقع اللي جابه لعندنا
        public string? UserAgent { get; set; } // بيانات المتصفح والجهاز
        public string? Browser { get; set; }   // Chrome, Safari...
        public string? DeviceType { get; set; } // Mobile, Desktop...

        public UrlMapping UrlMapping { get; set; } = null!;
    }
}
