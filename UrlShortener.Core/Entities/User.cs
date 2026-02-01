using Microsoft.AspNetCore.Identity;

namespace UrlShortener.Core.Entities
{
    public class User : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<UrlMapping> UrlMappings { get; set; } = new List<UrlMapping>();
    }
}
