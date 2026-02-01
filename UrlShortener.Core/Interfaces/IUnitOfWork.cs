namespace UrlShortener.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // إضافة الـ Repositories كخصائص (Properties)
        IUrlRepository Urls { get; }
        // إذا كان لديك ريبوزيتوري آخر للإحصائيات مثلاً
        // IAnalyticsRepository Analytics { get; }

        // الدالة المركزية لحفظ جميع التغييرات في كل الـ Repositories
        Task<int> CompleteAsync();
    }
}
