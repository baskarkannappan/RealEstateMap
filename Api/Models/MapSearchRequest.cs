namespace RealEstateMap.Api.Models;

public sealed class MapSearchRequest
{
    public string? PostalCode { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public double RadiusKm { get; init; } = 10;
}
