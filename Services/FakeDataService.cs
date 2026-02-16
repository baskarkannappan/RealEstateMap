using RealEstateMap.Models;

namespace RealEstateMap.Services;

public class FakeDataService
{
    private List<HouseLocation> Houses = new();

    // --- Grid-based cache for map zoom grouping ---
    private Dictionary<string, List<HouseLocation>> GridCache = new();

    public FakeDataService()
    {
        var rnd = new Random();

        for (int i = 0; i < 1000; i++)
        {
            var house = new HouseLocation
            {
                Lat = 20.5937 + rnd.NextDouble() * 5,
                Lng = 78.9629 + rnd.NextDouble() * 5,
                Address = $"House {i}",
                Pincode = $"{110000 + rnd.Next(0, 89999)}",
                City = $"City {rnd.Next(1, 50)}",
                State = $"State {rnd.Next(1, 10)}"
            };
            Houses.Add(house);

            // Precompute grid key for clustering (0.1 degree grid)
            var key = GetGridKey(house.Lat, house.Lng, 1); // 1 decimal for coarse clustering
            if (!GridCache.ContainsKey(key))
                GridCache[key] = new List<HouseLocation>();
            GridCache[key].Add(house);
        }

        // Add test house
        var testHouse = new HouseLocation
        {
            Lat = 20.6,
            Lng = 78.96,
            Address = "Test House",
            Pincode = "497442",
            City = "Test City",
            State = "Test State"
        };
        Houses.Add(testHouse);
        var testKey = GetGridKey(testHouse.Lat, testHouse.Lng, 1);
        if (!GridCache.ContainsKey(testKey))
            GridCache[testKey] = new List<HouseLocation>();
        GridCache[testKey].Add(testHouse);
    }

    // -------------------------------------------------
    // Map Bounds Filter
    // -------------------------------------------------
    public List<HouseLocation> GetByBounds(double south, double west, double north, double east)
    {
        // Optionally, can use GridCache to filter faster for large datasets
        return Houses
            .Where(h => h.Lat >= south && h.Lat <= north && h.Lng >= west && h.Lng <= east)
            .ToList();
    }

    // -------------------------------------------------
    // Main Search Method (Exact postal code, radius)
    // -------------------------------------------------
    public List<HouseLocation> Search(string? postalCode, string? city, string? state, double radiusKm = 5)
    {
        IEnumerable<HouseLocation> results = Houses;

        // Postal Filter (Exact)
        if (!string.IsNullOrWhiteSpace(postalCode))
        {
            postalCode = postalCode.Trim();
            results = results.Where(h =>
                h.Pincode != null &&
                h.Pincode.Trim().Equals(postalCode, StringComparison.OrdinalIgnoreCase));
        }

        // City Filter
        if (!string.IsNullOrWhiteSpace(city))
        {
            city = city.Trim();
            results = results.Where(h =>
                h.City != null &&
                h.City.Contains(city, StringComparison.OrdinalIgnoreCase));
        }

        // State Filter
        if (!string.IsNullOrWhiteSpace(state))
        {
            state = state.Trim();
            results = results.Where(h =>
                h.State != null &&
                h.State.Contains(state, StringComparison.OrdinalIgnoreCase));
        }

        var filtered = results.ToList();

        // Apply radius filter around first match
        if (!filtered.Any())
            return filtered;

        var center = filtered.First();

        return filtered
            .Where(h => CalculateDistanceKm(center.Lat, center.Lng, h.Lat, h.Lng) <= radiusKm)
            .ToList();
    }

    // -------------------------------------------------
    // Cluster / grid-based access for zoom-level grouping
    // -------------------------------------------------
    public Dictionary<string, List<HouseLocation>> GetGridCache() => GridCache;

    // Helper: compute grid key
    private string GetGridKey(double lat, double lng, int precision = 1)
    {
        var latKey = Math.Round(lat, precision);
        var lngKey = Math.Round(lng, precision);
        return $"{latKey}_{lngKey}";
    }

    // -------------------------------------------------
    // Haversine Distance
    // -------------------------------------------------
    private double CalculateDistanceKm(double lat1, double lng1, double lat2, double lng2)
    {
        const double R = 6371;
        double dLat = ToRadians(lat2 - lat1);
        double dLng = ToRadians(lng2 - lng1);
        double a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
            Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double deg) => deg * Math.PI / 180;
}
