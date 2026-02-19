namespace RealEstateMap.Api.Services.Caching;

public sealed record CacheEntryPolicy(
    TimeSpan AbsoluteExpiration,
    TimeSpan SlidingExpiration,
    long Size = 1);
