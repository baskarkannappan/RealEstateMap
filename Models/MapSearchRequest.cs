namespace RealEstateMap.Models;

public sealed class MapSearchRequest
{
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public double RadiusKm { get; set; } = 10;
}
