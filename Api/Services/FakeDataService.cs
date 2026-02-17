using RealEstateMap.Api.Models;

namespace RealEstateMap.Api.Services;

public sealed class FakeDataService
{
    private static readonly string[] Cities = ["Mumbai", "Delhi", "Bengaluru", "Hyderabad", "Chennai", "Kolkata", "Pune", "Ahmedabad", "Jaipur", "Lucknow"];

    private readonly List<HouseLocation> _houses;

    public FakeDataService()
    {
        var random = new Random(42);
        _houses = Enumerable.Range(1, 3500)
            .Select(index =>
            {
                var cityIndex = random.Next(Cities.Length);
                var city = Cities[cityIndex];
                var state = cityIndex switch
                {
                    0 => "Maharashtra",
                    1 => "Delhi",
                    2 => "Karnataka",
                    3 => "Telangana",
                    4 => "Tamil Nadu",
                    5 => "West Bengal",
                    6 => "Maharashtra",
                    7 => "Gujarat",
                    8 => "Rajasthan",
                    _ => "Uttar Pradesh"
                };

                // India geographic bounds approx: lat 8..37, lng 68..97
                var lat = 8 + random.NextDouble() * 29;
                var lng = 68 + random.NextDouble() * 29;

                return new HouseLocation
                {
                    Id = index.ToString(),
                    Lat = lat,
                    Lng = lng,
                    Address = $"{100 + index} Residency Road",
                    City = city,
                    State = state,
                    PostalCode = $"{100000 + random.Next(0, 899999):D6}"
                };
            })
            .ToList();
    }

    public Task<List<HouseLocation>> GetByBoundsAsync(double south, double west, double north, double east, CancellationToken cancellationToken)
    {
        var results = _houses
            .Where(h => h.Lat >= south && h.Lat <= north && h.Lng >= west && h.Lng <= east)
            .Take(1500)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<List<HouseLocation>> SearchAsync(MapSearchRequest request, CancellationToken cancellationToken)
    {
        var radiusKm = Math.Clamp(request.RadiusKm <= 0 ? 10 : request.RadiusKm, 1, 250);

        // If explicit center is provided from client geocoding, use it directly.
        if (request.CenterLat is double centerLat && request.CenterLng is double centerLng)
        {
            var centered = _houses
                .Where(h => DistanceKm(centerLat, centerLng, h.Lat, h.Lng) <= radiusKm)
                .Take(1500)
                .ToList();

            return Task.FromResult(centered);
        }

        // Backward compatible path: infer from text filters when center isn't provided.
        IEnumerable<HouseLocation> query = _houses;

        if (!string.IsNullOrWhiteSpace(request.PostalCode))
        {
            var pin = request.PostalCode.Trim();
            query = query.Where(h => h.PostalCode.Equals(pin, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            var city = request.City.Trim();
            query = query.Where(h => h.City.Contains(city, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(request.State))
        {
            var state = request.State.Trim();
            query = query.Where(h => h.State.Contains(state, StringComparison.OrdinalIgnoreCase));
        }

        var filtered = query.ToList();
        if (filtered.Count == 0)
        {
            return Task.FromResult(new List<HouseLocation>());
        }

        var center = filtered[0];
        var byRadius = _houses
            .Where(h => DistanceKm(center.Lat, center.Lng, h.Lat, h.Lng) <= radiusKm)
            .Take(1500)
            .ToList();

        return Task.FromResult(byRadius);
    }

    private static double DistanceKm(double lat1, double lng1, double lat2, double lng2)
    {
        const double radius = 6371;
        var dLat = ToRadians(lat2 - lat1);
        var dLng = ToRadians(lng2 - lng1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return radius * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
