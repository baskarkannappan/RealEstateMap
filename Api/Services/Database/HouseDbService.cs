using Microsoft.Extensions.Caching.Memory;
using RealEstateMap.Api.Services.Abstractions;
using RealEstateMap.DAL.Repositories;
using ApiModels = RealEstateMap.Api.Models;
using SharedModels = RealEstateMap.Shared.Models;

namespace RealEstateMap.Api.Services.Database;

public interface IHouseDbService : IHouseDataService;

public sealed class HouseDbService : IHouseDbService
{
    private readonly IHouseRepository _houseRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<HouseDbService> _logger;

    public HouseDbService(IHouseRepository houseRepository, IMemoryCache cache, ILogger<HouseDbService> logger)
    {
        _houseRepository = houseRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<ApiModels.HouseLocation>> SearchAsync(ApiModels.MapSearchRequest request, CancellationToken cancellationToken)
    {
        var pagination = BuildPagination(request);
        if (request.CenterLat is not double centerLat || request.CenterLng is not double centerLng)
        {
            return (await _houseRepository.SearchByFiltersAsync(ToSharedRequest(request, pagination), cancellationToken))
                .Select(ToApiModel)
                .ToList();
        }

        var radius = Math.Clamp(request.RadiusKm <= 0 ? 10 : request.RadiusKm, 1, 250);
        var cacheKey = $"radius:{Math.Round(centerLat, 4)}:{Math.Round(centerLng, 4)}:{Math.Round(radius, 2)}:{pagination.PageNumber}:{pagination.PageSize}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20);
            var rows = await _houseRepository.SearchByRadiusAsync(centerLat, centerLng, radius, pagination, cancellationToken);
            return rows.Select(ToApiModel).ToList();
        }) ?? new List<ApiModels.HouseLocation>();
    }

    public async Task<List<ApiModels.HouseLocation>> GetByBoundsAsync(double south, double west, double north, double east, CancellationToken cancellationToken)
    {
        var pagination = new SharedModels.PaginationRequest { PageNumber = 1, PageSize = 500 };
        var cacheKey = $"bounds:{Math.Round(south, 3)}:{Math.Round(west, 3)}:{Math.Round(north, 3)}:{Math.Round(east, 3)}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
            var rows = await _houseRepository.SearchByBoundsAsync(south, west, north, east, pagination, cancellationToken);
            return rows.Select(ToApiModel).ToList();
        }) ?? new List<ApiModels.HouseLocation>();
    }

    public async Task<List<ApiModels.HouseLocation>> GetListAsync(ApiModels.MapSearchRequest request, CancellationToken cancellationToken)
    {
        var pagination = BuildPagination(request);
        var rows = await _houseRepository.SearchByFiltersAsync(ToSharedRequest(request, pagination), cancellationToken);
        return rows.Select(ToApiModel).ToList();
    }

    private static SharedModels.PaginationRequest BuildPagination(ApiModels.MapSearchRequest request)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 250 : Math.Clamp(request.PageSize, 1, 1000);
        return new SharedModels.PaginationRequest { PageNumber = pageNumber, PageSize = pageSize };
    }

    private static SharedModels.MapSearchRequest ToSharedRequest(ApiModels.MapSearchRequest request, SharedModels.PaginationRequest pagination) => new()
    {
        PostalCode = request.PostalCode,
        City = request.City,
        State = request.State,
        CenterLat = request.CenterLat,
        CenterLng = request.CenterLng,
        RadiusKm = request.RadiusKm,
        Pagination = pagination,
        South = request.South,
        West = request.West,
        North = request.North,
        East = request.East
    };

    private static ApiModels.HouseLocation ToApiModel(SharedModels.HouseLocation model) => new()
    {
        Id = model.HouseId == Guid.Empty ? Guid.NewGuid().ToString("N") : model.HouseId.ToString("N"),
        Address = model.Address,
        City = model.City,
        State = model.State,
        PostalCode = model.PostalCode,
        Lat = model.Lat,
        Lng = model.Lng
    };
}
