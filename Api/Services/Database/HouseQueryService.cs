using RealEstateMap.DAL.Repositories;
using ApiModels = RealEstateMap.Api.Models;
using SharedModels = RealEstateMap.Shared.Models;

namespace RealEstateMap.Api.Services.Database;

public interface IHouseQueryService
{
    Task<List<ApiModels.HouseLocation>> SearchAsync(ApiModels.MapSearchRequest request, CancellationToken cancellationToken);
    Task<List<ApiModels.HouseLocation>> GetByBoundsAsync(double south, double west, double north, double east, CancellationToken cancellationToken);
    Task<List<ApiModels.HouseLocation>> GetListAsync(ApiModels.MapSearchRequest request, CancellationToken cancellationToken);
}

public sealed class HouseQueryService : IHouseQueryService
{
    private readonly IHouseRepository _houseRepository;

    public HouseQueryService(IHouseRepository houseRepository)
    {
        _houseRepository = houseRepository;
    }

    public async Task<List<ApiModels.HouseLocation>> SearchAsync(ApiModels.MapSearchRequest request, CancellationToken cancellationToken)
    {
        var pagination = BuildPagination(request);
        if (request.CenterLat is not double centerLat || request.CenterLng is not double centerLng)
        {
            var fallbackRows = await _houseRepository.SearchByFiltersAsync(ToSharedRequest(request, pagination), cancellationToken);
            return fallbackRows.Select(ToApiModel).ToList();
        }

        var radius = Math.Clamp(request.RadiusKm <= 0 ? 10 : request.RadiusKm, 1, 250);
        var rows = await _houseRepository.SearchByRadiusAsync(centerLat, centerLng, radius, pagination, cancellationToken);
        return rows.Select(ToApiModel).ToList();
    }

    public async Task<List<ApiModels.HouseLocation>> GetByBoundsAsync(
        double south,
        double west,
        double north,
        double east,
        CancellationToken cancellationToken)
    {
        var pagination = new SharedModels.PaginationRequest { PageNumber = 1, PageSize = 200 }; // Optimized limit
        var rows = await _houseRepository.SearchByBoundsAsync(south, west, north, east, pagination, cancellationToken);
        return rows.Select(ToApiModel).ToList();
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

    private static SharedModels.MapSearchRequest ToSharedRequest(
        ApiModels.MapSearchRequest request,
        SharedModels.PaginationRequest pagination) => new()
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
        Lat = model.Lat,
        Lng = model.Lng
    };
}
