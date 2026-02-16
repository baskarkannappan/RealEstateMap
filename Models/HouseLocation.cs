namespace RealEstateMap.Models;

public class HouseLocation
{
    // Unique Identifier
    public string Id { get; set; } = Guid.NewGuid().ToString();

    // Location
    public double Lat { get; set; }
    public double Lng { get; set; }

    // Address Details
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string Pincode { get; set; } = "";

    // -------------------------------------------------
    // Marker Type for map coloring / legend
    // -------------------------------------------------
    // Possible values:
    // "normal" - default blue
    // "search" - search result (orange)
    // "premium" - premium listing (green)
    // "sold" - sold property (grey)
    public string Type { get; set; } = "normal";
}
