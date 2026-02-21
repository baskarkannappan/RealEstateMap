namespace RealEstateMap.Api.Models;

public sealed class HouseLocation
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public double Lat { get; init; }
    public double Lng { get; init; }
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
}
