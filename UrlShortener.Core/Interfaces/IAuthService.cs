namespace UrlShortener.Core.Interfaces
{
    public interface IAuthService
    {
        string GenerateJwtToken(string userId, string email);
    }
}
