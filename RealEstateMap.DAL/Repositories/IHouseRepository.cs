using RealEstateMap.Shared.Models;

namespace RealEstateMap.DAL.Repositories;

public interface IHouseRepository
{
    Task<IReadOnlyList<HouseLocation>> SearchByBoundsAsync(
        double south,
        double west,
        double north,
        double east,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HouseLocation>> SearchByRadiusAsync(
        double centerLat,
        double centerLng,
        double radiusKm,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HouseLocation>> SearchByFiltersAsync(
        MapSearchRequest request,
        CancellationToken cancellationToken = default);
}
