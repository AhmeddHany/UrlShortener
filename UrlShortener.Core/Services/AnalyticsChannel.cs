using System.Threading.Channels;
using UrlShortener.Core.DTO;
using UrlShortener.Core.Interfaces;
namespace UrlShortener.Core.Services
{
    public class AnalyticsChannel : IAnalyticsChannel
    {
        private readonly Channel<ClickData> _channel;

        public AnalyticsChannel()
        {
            _channel = Channel.CreateUnbounded<ClickData>(new UnboundedChannelOptions
            {
                SingleReader = true, // عامل واحد بيسحب
                SingleWriter = false // كذا Controller بيرموا
            });
        }

        public async ValueTask WriteClickAsync(ClickData data) => await _channel.Writer.WriteAsync(data);

        public IAsyncEnumerable<ClickData> ReadAllAsync(CancellationToken ct) => _channel.Reader.ReadAllAsync(ct);
    }
}
