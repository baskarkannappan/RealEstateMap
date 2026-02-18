using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateMap.Api.Models;
using RealEstateMap.Api.Services.Abstractions;

namespace RealEstateMap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class HouseController : ControllerBase
{
    private readonly IHouseDataService _houseDataService;

    public HouseController(IHouseDataService houseDataService)
    {
        _houseDataService = houseDataService;
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
        var results = await _houseDataService.GetByBoundsAsync(south, west, north, east, cancellationToken);
        return Ok(results);
    }
}
