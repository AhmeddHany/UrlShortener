using UrlShortener.Core.DTO;

namespace UrlShortener.Core.Interfaces
{
    public interface IAnalyticsChannel
    {
        ValueTask WriteClickAsync(ClickData data);
        IAsyncEnumerable<ClickData> ReadAllAsync(CancellationToken ct);
    }
}
