using UrlShortener.Core.Entities;

namespace UrlShortener.Core.Interfaces
{
    public interface IUrlRepository
    {
        Task AddAsync(UrlMapping mapping); // تحويل لـ Async
        Task UpdateAsync(UrlMapping mapping);
        Task<UrlMapping?> GetByCodeAsync(string code);
        Task<List<UrlMapping>> GetAllByUserIdAsync(string userId);
    }
}
