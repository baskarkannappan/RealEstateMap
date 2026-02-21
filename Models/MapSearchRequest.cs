namespace RealEstateMap.Models;

public sealed class MapSearchRequest
{
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PropertyIntent { get; set; } = "Buy";
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinBeds { get; set; }
    public int? MaxBeds { get; set; }
    public int? MinBaths { get; set; }
    public int? MaxBaths { get; set; }
    public string? HomeType { get; set; }
    public int? MinSqFt { get; set; }
    public int? MaxSqFt { get; set; }

    // Optional explicit center point for radius search.
    public double? CenterLat { get; set; }
    public double? CenterLng { get; set; }

    public double RadiusKm { get; set; } = 10;
    public PaginationRequest Pagination { get; set; } = new();
}
