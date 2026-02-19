namespace RealEstateMap.Api.Services.Caching;

public interface ICacheService
{
    Task<T> GetOrCreateAsync<T>(
        string region,
        string key,
        Func<CancellationToken, Task<T>> factory,
        CacheEntryPolicy policy,
        CancellationToken cancellationToken);

    Task InvalidateRegionAsync(string region, CancellationToken cancellationToken);
}
