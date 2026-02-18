namespace RealEstateMap.Shared.Models;

public sealed class House
{
    public Guid HouseId { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime? CreatedUtc { get; set; }
}
