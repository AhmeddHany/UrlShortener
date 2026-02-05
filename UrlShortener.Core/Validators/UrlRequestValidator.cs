using FluentValidation;
using UrlShortener.Core.DTO;

namespace UrlShortener.Core.Validators
{
    public class UrlRequestValidator : AbstractValidator<UrlRequest>
    {
        public UrlRequestValidator()
        {
            RuleFor(x => x.LongUrl)
                .NotEmpty().WithMessage("الرابط الطويل مطلوب ولا يمكن تركه فارغاً.")
                .MaximumLength(2048).WithMessage("الرابط طويل جداً، الحد الأقصى هو 2048 حرفاً.")
                .Must(BeAValidUrl).WithMessage("صيغة الرابط غير صحيحة. يجب أن يبدأ بـ http أو https.")
                .MustAsync(async (url, cancellation) => await CheckDns(url))
                .WithMessage("هذا الموقع غير موجود أو تعذر الوصول إليه.");
        }

        private bool BeAValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
        private async Task<bool> CheckDns(string url)
        {
            try
            {
                var host = new Uri(url).Host;
                await System.Net.Dns.GetHostEntryAsync(host);
                return true;
            }
            catch { return false; }
        }
    }
}
