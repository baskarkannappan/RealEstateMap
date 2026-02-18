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

    // Optional bounds payload support.
    public double? South { get; init; }
    public double? West { get; init; }
    public double? North { get; init; }
    public double? East { get; init; }

    // Pagination controls.
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 250;
}
