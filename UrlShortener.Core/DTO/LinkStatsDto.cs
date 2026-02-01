namespace UrlShortener.Core.DTO
{
    public class LinkStatsDto
    {
        public int TotalClicks { get; set; }
        public DateTime? LastClickedAt { get; set; }
        public List<CountryStatDto> TopCountries { get; set; } = new();
        public List<DeviceStatDto> DeviceBreakdown { get; set; } = new();
        public List<DailyClickDto> ClicksLast7Days { get; set; } = new();
    }
    public class CountryStatDto
    {
        public string CountryName { get; set; } = "Unknown";
        public int ClickCount { get; set; }
    }

    // 3. إحصائيات الأجهزة (Mobile vs Desktop)
    public class DeviceStatDto
    {
        public string DeviceType { get; set; } = "Unknown";
        public int Count { get; set; }
    }

    // 4. للرسم البياني (Clicks per Day)
    public class DailyClickDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }
}
