namespace RealEstateMap.Shared.Models;

public sealed class HouseLocation
{
    public Guid HouseId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
}
