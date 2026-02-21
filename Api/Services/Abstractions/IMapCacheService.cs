using RealEstateMap.Api.Models;

namespace RealEstateMap.Api.Services.Abstractions;

public interface IMapCacheService
{
    Task<List<HouseLocation>> GetOrAddAsync(
        double south,
        double west,
        double north,
        double east,
        Func<CancellationToken, Task<List<HouseLocation>>> factory,
        CancellationToken cancellationToken);

    Task InvalidateAsync(CancellationToken cancellationToken);
}
