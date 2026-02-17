namespace RealEstateMap.Services;

public interface IAuthService
{
    Task<string?> GetTokenAsync(CancellationToken cancellationToken = default);
    void InvalidateToken();
}
