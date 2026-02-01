using Microsoft.EntityFrameworkCore;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Interfaces;
using UrlShortener.Infrastructure.Data;

namespace UrlShortener.Infrastructure.Repositories
{
    public class EfUrlRepository : IUrlRepository
    {
        private readonly UrlDbContext _dbContext;
        public EfUrlRepository(UrlDbContext dbContext) => _dbContext = dbContext;

        public async Task AddAsync(UrlMapping mapping)
        {
            _dbContext.UrlMappings.Add(mapping);
            await _dbContext.SaveChangesAsync(); // الحفظ الأول لجلب الـ ID
        }

        public async Task UpdateAsync(UrlMapping mapping)
        {
            _dbContext.UrlMappings.Update(mapping);
            await _dbContext.SaveChangesAsync(); // الحفظ الثاني لتحديث الـ Code
        }

        public async Task<UrlMapping?> GetByCodeAsync(string code)
        {
            return await _dbContext.UrlMappings.FirstOrDefaultAsync(x => x.ShortCode == code);
        }
        public async Task<List<UrlMapping>> GetAllByUserIdAsync(string userId)
        {
            return await _dbContext.UrlMappings
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }
    }
}
