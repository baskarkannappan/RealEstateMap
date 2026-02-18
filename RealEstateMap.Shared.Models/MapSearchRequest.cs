namespace RealEstateMap.Shared.Models;

public sealed class MapSearchRequest
{
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public double? CenterLat { get; set; }
    public double? CenterLng { get; set; }
    public double RadiusKm { get; set; } = 10;
    public double? South { get; set; }
    public double? West { get; set; }
    public double? North { get; set; }
    public double? East { get; set; }
    public PaginationRequest Pagination { get; set; } = new();
}
