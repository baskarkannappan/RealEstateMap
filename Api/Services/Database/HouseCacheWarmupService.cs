using Microsoft.Extensions.Options;
using RealEstateMap.Api.Models;
using RealEstateMap.Api.Options;

namespace RealEstateMap.Api.Services.Database;

public sealed class HouseCacheWarmupService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CacheOptions _cacheOptions;
    private readonly DataSourceOptions _dataSourceOptions;
    private readonly ILogger<HouseCacheWarmupService> _logger;

    public HouseCacheWarmupService(
        IServiceScopeFactory scopeFactory,
        IOptions<CacheOptions> cacheOptions,
        IOptions<DataSourceOptions> dataSourceOptions,
        ILogger<HouseCacheWarmupService> logger)
    {
        _scopeFactory = scopeFactory;
        _cacheOptions = cacheOptions.Value;
        _dataSourceOptions = dataSourceOptions.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_cacheOptions.WarmupOnStartup || !_dataSourceOptions.UseDatabase)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var houseDataService = scope.ServiceProvider.GetRequiredService<IHouseDbService>();

        var warmupRequest = new MapSearchRequest
        {
            PageNumber = 1,
            PageSize = Math.Clamp(_cacheOptions.WarmupPageSize, 1, 1000)
        };

        try
        {
            _ = await houseDataService.GetListAsync(warmupRequest, cancellationToken);
            _logger.LogInformation("House cache warmup completed.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "House cache warmup failed.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
