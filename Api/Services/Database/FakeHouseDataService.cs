using RealEstateMap.Api.Models;
using RealEstateMap.Api.Services.Abstractions;

namespace RealEstateMap.Api.Services.Database;

public sealed class FakeHouseDataService : IHouseDataService
{
    private readonly FakeDataService _fakeDataService;

    public FakeHouseDataService(FakeDataService fakeDataService)
    {
        _fakeDataService = fakeDataService;
    }

    public Task<List<HouseLocation>> SearchAsync(MapSearchRequest request, CancellationToken cancellationToken) =>
        _fakeDataService.SearchAsync(request, cancellationToken);

    public Task<List<HouseLocation>> GetByBoundsAsync(double south, double west, double north, double east, CancellationToken cancellationToken) =>
        _fakeDataService.GetByBoundsAsync(south, west, north, east, cancellationToken);

    public Task<List<HouseLocation>> GetListAsync(MapSearchRequest request, CancellationToken cancellationToken) =>
        _fakeDataService.SearchAsync(request, cancellationToken);
}
