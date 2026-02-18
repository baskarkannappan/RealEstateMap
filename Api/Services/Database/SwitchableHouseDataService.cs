using Microsoft.Extensions.Options;
using RealEstateMap.Api.Models;
using RealEstateMap.Api.Options;
using RealEstateMap.Api.Services.Abstractions;

namespace RealEstateMap.Api.Services.Database;

public sealed class SwitchableHouseDataService : IHouseDataService
{
    private readonly IHouseDbService _dbService;
    private readonly FakeHouseDataService _fakeService;
    private readonly DataSourceOptions _options;
    private readonly ILogger<SwitchableHouseDataService> _logger;

    public SwitchableHouseDataService(
        IHouseDbService dbService,
        FakeHouseDataService fakeService,
        IOptions<DataSourceOptions> options,
        ILogger<SwitchableHouseDataService> logger)
    {
        _dbService = dbService;
        _fakeService = fakeService;
        _options = options.Value;
        _logger = logger;
    }

    public Task<List<HouseLocation>> SearchAsync(MapSearchRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(s => s.SearchAsync(request, cancellationToken), request, cancellationToken);

    public Task<List<HouseLocation>> GetByBoundsAsync(double south, double west, double north, double east, CancellationToken cancellationToken) =>
        ExecuteAsync(s => s.GetByBoundsAsync(south, west, north, east, cancellationToken), null, cancellationToken);

    public Task<List<HouseLocation>> GetListAsync(MapSearchRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(s => s.GetListAsync(request, cancellationToken), request, cancellationToken);

    private async Task<List<HouseLocation>> ExecuteAsync(
        Func<IHouseDataService, Task<List<HouseLocation>>> operation,
        MapSearchRequest? request,
        CancellationToken cancellationToken)
    {
        if (!_options.UseDatabase)
        {
            return await operation(_fakeService);
        }

        try
        {
            return await operation(_dbService);
        }
        catch (Exception ex) when (_options.FallbackToFakeData)
        {
            _logger.LogWarning(ex, "Database service failed. Falling back to fake data source.");
            return await operation(_fakeService);
        }
    }
}
