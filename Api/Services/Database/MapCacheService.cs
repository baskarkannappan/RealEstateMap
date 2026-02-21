using Microsoft.Extensions.Options;
using RealEstateMap.Api.Models;
using RealEstateMap.Api.Options;
using RealEstateMap.Api.Services.Abstractions;
using RealEstateMap.Api.Services.Caching;

namespace RealEstateMap.Api.Services.Database;

public sealed class MapCacheService : IMapCacheService
{
    private const string MapRegion = "map-search";
    private readonly ICacheService _cacheService;
    private readonly CacheEntryPolicy _policy;

    public MapCacheService(ICacheService cacheService, IOptions<CacheOptions> options)
    {
        _cacheService = cacheService;
        var cacheOptions = options.Value;
        _policy = new CacheEntryPolicy(
            TimeSpan.FromMinutes(Math.Max(1, cacheOptions.AbsoluteExpirationMinutes)),
            TimeSpan.FromMinutes(Math.Max(1, cacheOptions.SlidingExpirationMinutes)));
    }

    public Task<List<HouseLocation>> GetOrAddAsync(
        double south,
        double west,
        double north,
        double east,
        Func<CancellationToken, Task<List<HouseLocation>>> factory,
        CancellationToken cancellationToken)
    {
        var key = BuildCacheKey(south, west, north, east);
        return _cacheService.GetOrCreateAsync(MapRegion, key, factory, _policy, cancellationToken);
    }

    public Task InvalidateAsync(CancellationToken cancellationToken) =>
        _cacheService.InvalidateRegionAsync(MapRegion, cancellationToken);

    private static string BuildCacheKey(double south, double west, double north, double east)
    {
        // Round to 3 decimal places to group nearby requests (approx 100m precision)
        return FormattableString.Invariant(
            $"v:s={Math.Round(south, 3)}:w={Math.Round(west, 3)}:n={Math.Round(north, 3)}:e={Math.Round(east, 3)}");
    }
}
