using RealEstateMap.Models;

namespace RealEstateMap.Services;

public interface IHouseService
{
    Task<List<HouseLocation>> SearchAsync(MapSearchRequest request);
    Task<List<HouseLocation>> GetBoundsAsync(double south, double west, double north, double east);
}
