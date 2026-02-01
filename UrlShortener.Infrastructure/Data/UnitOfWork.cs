using UrlShortener.Core.Interfaces;
using UrlShortener.Infrastructure.Repositories;

namespace UrlShortener.Infrastructure.Data
{
    public class UnitOfWork(UrlDbContext context) : IUnitOfWork
    {
        private IUrlRepository _urls;

        // تنفيذ الـ Repository بنظام Lazy Loading (يُنشأ فقط عند الحاجة)
        public IUrlRepository Urls => _urls ??= new EfUrlRepository(context);

        public async Task<int> CompleteAsync()
        {
            return await context.SaveChangesAsync();
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
