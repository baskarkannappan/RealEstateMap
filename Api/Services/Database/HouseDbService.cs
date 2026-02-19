using Microsoft.Extensions.Options;
using RealEstateMap.Api.Options;
using RealEstateMap.Api.Services.Abstractions;
using RealEstateMap.Api.Services.Caching;
using ApiModels = RealEstateMap.Api.Models;

namespace RealEstateMap.Api.Services.Database;

public interface IHouseCacheInvalidationService
{
    Task InvalidateListingsAsync(CancellationToken cancellationToken);
}

public interface IHouseDbService : IHouseDataService, IHouseCacheInvalidationService;

public sealed class HouseDbService : IHouseDbService
{
    private const string ListingsRegion = "house-listings";

    private readonly IHouseQueryService _queryService;
    private readonly ICacheService _cacheService;
    private readonly CacheEntryPolicy _policy;

    public HouseDbService(
        IHouseQueryService queryService,
        ICacheService cacheService,
        IOptions<CacheOptions> options)
    {
        _queryService = queryService;
        _cacheService = cacheService;

        var cacheOptions = options.Value;
        _policy = new CacheEntryPolicy(
            TimeSpan.FromMinutes(Math.Max(1, cacheOptions.AbsoluteExpirationMinutes)),
            TimeSpan.FromMinutes(Math.Max(1, cacheOptions.SlidingExpirationMinutes)));
    }

    public Task<List<ApiModels.HouseLocation>> SearchAsync(ApiModels.MapSearchRequest request, CancellationToken cancellationToken)
    {
        var key = BuildSearchKey(request);
        return _cacheService.GetOrCreateAsync(
            ListingsRegion,
            key,
            ct => _queryService.SearchAsync(request, ct),
            _policy,
            cancellationToken);
    }

    public Task<List<ApiModels.HouseLocation>> GetByBoundsAsync(
        double south,
        double west,
        double north,
        double east,
        CancellationToken cancellationToken)
    {
        var key = BuildBoundsKey(south, west, north, east);
        return _cacheService.GetOrCreateAsync(
            ListingsRegion,
            key,
            ct => _queryService.GetByBoundsAsync(south, west, north, east, ct),
            _policy,
            cancellationToken);
    }

    public Task<List<ApiModels.HouseLocation>> GetListAsync(ApiModels.MapSearchRequest request, CancellationToken cancellationToken)
    {
        var key = BuildListKey(request);
        return _cacheService.GetOrCreateAsync(
            ListingsRegion,
            key,
            ct => _queryService.GetListAsync(request, ct),
            _policy,
            cancellationToken);
    }

    public Task InvalidateListingsAsync(CancellationToken cancellationToken) =>
        _cacheService.InvalidateRegionAsync(ListingsRegion, cancellationToken);

    private static string BuildSearchKey(ApiModels.MapSearchRequest request)
    {
        var centerLat = request.CenterLat?.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) ?? "null";
        var centerLng = request.CenterLng?.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) ?? "null";
        var radius = Math.Round(request.RadiusKm <= 0 ? 10 : request.RadiusKm, 2);

        return FormattableString.Invariant(
            $"search:postal={Normalize(request.PostalCode)}:city={Normalize(request.City)}:state={Normalize(request.State)}:lat={centerLat}:lng={centerLng}:radius={radius}:page={NormalizePage(request.PageNumber)}:size={NormalizeSize(request.PageSize)}");
    }

    private static string BuildListKey(ApiModels.MapSearchRequest request) =>
        FormattableString.Invariant(
            $"list:postal={Normalize(request.PostalCode)}:city={Normalize(request.City)}:state={Normalize(request.State)}:page={NormalizePage(request.PageNumber)}:size={NormalizeSize(request.PageSize)}");

    private static string BuildBoundsKey(double south, double west, double north, double east) =>
        FormattableString.Invariant(
            $"bounds:s={Math.Round(south, 3)}:w={Math.Round(west, 3)}:n={Math.Round(north, 3)}:e={Math.Round(east, 3)}");

    private static int NormalizePage(int pageNumber) => pageNumber <= 0 ? 1 : pageNumber;

    private static int NormalizeSize(int pageSize) => pageSize <= 0 ? 250 : Math.Clamp(pageSize, 1, 1000);

    private static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "null" : value.Trim().ToLowerInvariant();
}
