namespace RealEstateMap.Models;

public sealed class MapSearchRequest
{
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }

    // Optional explicit center point for radius search.
    public double? CenterLat { get; set; }
    public double? CenterLng { get; set; }

    public double RadiusKm { get; set; } = 10;
}
