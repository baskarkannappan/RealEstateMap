using RealEstateMap.Api.Models;

namespace RealEstateMap.Api.Services.Abstractions;

public interface IHouseDataService
{
    Task<List<HouseLocation>> SearchAsync(MapSearchRequest request, CancellationToken cancellationToken);
    Task<List<HouseLocation>> GetByBoundsAsync(double south, double west, double north, double east, CancellationToken cancellationToken);
    Task<List<HouseLocation>> GetListAsync(MapSearchRequest request, CancellationToken cancellationToken);
}
