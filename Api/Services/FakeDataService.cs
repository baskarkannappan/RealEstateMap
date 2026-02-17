using RealEstateMap.Api.Models;

namespace RealEstateMap.Api.Services;

public sealed class FakeDataService
{
    private readonly List<HouseLocation> _houses;

    public FakeDataService()
    {
        var random = new Random(42);
        _houses = Enumerable.Range(1, 2500)
            .Select(index => new HouseLocation
            {
                Id = index.ToString(),
                Lat = 39.5 + (random.NextDouble() - 0.5) * 8,
                Lng = -98.35 + (random.NextDouble() - 0.5) * 12,
                Address = $"{100 + index} Main St",
                City = $"City {random.Next(1, 30)}",
                State = $"State {random.Next(1, 8)}",
                PostalCode = $"{70000 + random.Next(0, 9999)}"
            })
            .ToList();
    }

    public Task<List<HouseLocation>> GetByBoundsAsync(double south, double west, double north, double east, CancellationToken cancellationToken)
    {
        var results = _houses
            .Where(h => h.Lat >= south && h.Lat <= north && h.Lng >= west && h.Lng <= east)
            .Take(1000)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<List<HouseLocation>> SearchAsync(MapSearchRequest request, CancellationToken cancellationToken)
    {
        IEnumerable<HouseLocation> query = _houses;

        if (!string.IsNullOrWhiteSpace(request.PostalCode))
        {
            var postalCode = request.PostalCode.Trim();
            query = query.Where(h => h.PostalCode.Equals(postalCode, StringComparison.OrdinalIgnoreCase));
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
        var radiusKm = Math.Clamp(request.RadiusKm <= 0 ? 10 : request.RadiusKm, 1, 250);
        var byRadius = filtered
            .Where(h => DistanceKm(center.Lat, center.Lng, h.Lat, h.Lng) <= radiusKm)
            .Take(1000)
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
