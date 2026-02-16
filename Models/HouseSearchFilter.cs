namespace RealEstateMap.Models;

public class HouseSearchFilter
{
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }

    public double? CenterLat { get; set; }
    public double? CenterLng { get; set; }

    public double RadiusKm { get; set; } = 5;
}
