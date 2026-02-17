namespace RealEstateMap.Api.Models;

public sealed class MapSearchRequest
{
    public string? PostalCode { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }

    // Optional explicit center point for radius search.
    public double? CenterLat { get; init; }
    public double? CenterLng { get; init; }

    public double RadiusKm { get; init; } = 10;
}
