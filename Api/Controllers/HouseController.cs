using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateMap.Api.Models;
using RealEstateMap.Api.Services.Abstractions;
using RealEstateMap.Api.Services.Database;

namespace RealEstateMap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class HouseController : ControllerBase
{
    private readonly IHouseDataService _houseDataService;
    private readonly IHouseDbService _houseDbService;
    private readonly IMapCacheService _mapCacheService;

    public HouseController(IHouseDataService houseDataService, IHouseDbService houseDbService, IMapCacheService mapCacheService)
    {
        _houseDataService = houseDataService;
        _houseDbService = houseDbService;
        _mapCacheService = mapCacheService;
    }

    [HttpPost("list")]
    public async Task<ActionResult<List<HouseLocation>>> List([FromBody] MapSearchRequest request, CancellationToken cancellationToken)
    {
        var results = await _houseDataService.GetListAsync(request, cancellationToken);
        return Ok(results);
    }

    [HttpPost("search")]
    public async Task<ActionResult<List<HouseLocation>>> Search([FromBody] MapSearchRequest request, CancellationToken cancellationToken)
    {
        var results = await _houseDataService.SearchAsync(request, cancellationToken);
        return Ok(results);
    }

    [HttpGet("bounds")]
    public async Task<ActionResult<List<HouseLocation>>> Bounds(
        [FromQuery] double south,
        [FromQuery] double west,
        [FromQuery] double north,
        [FromQuery] double east,
        CancellationToken cancellationToken)
    {
        var results = await _mapCacheService.GetOrAddAsync(
            south, west, north, east,
            ct => _houseDataService.GetByBoundsAsync(south, west, north, east, ct),
            cancellationToken);

        return Ok(results);
    }

    [HttpPost("cache/invalidate")]
    public async Task<IActionResult> InvalidateCache(CancellationToken cancellationToken)
    {
        await _houseDbService.InvalidateListingsAsync(cancellationToken);
        return NoContent();
    }
}
