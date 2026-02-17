using RealEstateMap.Api.Models;

namespace RealEstateMap.Api.Services;

public sealed class FakeDataService
{
    private static readonly string[] Cities = ["Mumbai", "Delhi", "Bengaluru", "Hyderabad", "Chennai", "Kolkata", "Pune", "Ahmedabad", "Jaipur", "Lucknow"];

    private static readonly Dictionary<string, (double Lat, double Lng)> PinCenters = new(StringComparer.OrdinalIgnoreCase)
    {
        ["110001"] = (28.6328, 77.2197),
        ["400001"] = (18.9388, 72.8354),
        ["560001"] = (12.9763, 77.6033),
        ["500001"] = (17.3850, 78.4867),
        ["600001"] = (13.0827, 80.2707),
        ["700001"] = (22.5726, 88.3639),
        ["411001"] = (18.5204, 73.8567),
        ["380001"] = (23.0225, 72.5714),
        ["302001"] = (26.9124, 75.7873),
        ["226001"] = (26.8467, 80.9462)
    };

    private static readonly Dictionary<string, (double Lat, double Lng)> PinPrefixCenters = new(StringComparer.OrdinalIgnoreCase)
    {
        ["110"] = (28.6139, 77.2090),
        ["400"] = (19.0760, 72.8777),
        ["560"] = (12.9716, 77.5946),
        ["500"] = (17.3850, 78.4867),
        ["600"] = (13.0827, 80.2707),
        ["700"] = (22.5726, 88.3639),
        ["411"] = (18.5204, 73.8567),
        ["380"] = (23.0225, 72.5714),
        ["302"] = (26.9124, 75.7873),
        ["226"] = (26.8467, 80.9462)
    };

    private static readonly Dictionary<string, (double Lat, double Lng)> CityCenters = new(StringComparer.OrdinalIgnoreCase)
    {
        ["mumbai|maharashtra"] = (19.0760, 72.8777),
        ["delhi|delhi"] = (28.6139, 77.2090),
        ["bengaluru|karnataka"] = (12.9716, 77.5946),
        ["hyderabad|telangana"] = (17.3850, 78.4867),
        ["chennai|tamil nadu"] = (13.0827, 80.2707),
        ["kolkata|west bengal"] = (22.5726, 88.3639),
        ["pune|maharashtra"] = (18.5204, 73.8567),
        ["ahmedabad|gujarat"] = (23.0225, 72.5714),
        ["jaipur|rajasthan"] = (26.9124, 75.7873),
        ["lucknow|uttar pradesh"] = (26.8467, 80.9462),
        ["mumbai"] = (19.0760, 72.8777),
        ["delhi"] = (28.6139, 77.2090),
        ["bengaluru"] = (12.9716, 77.5946),
        ["hyderabad"] = (17.3850, 78.4867),
        ["chennai"] = (13.0827, 80.2707),
        ["kolkata"] = (22.5726, 88.3639),
        ["pune"] = (18.5204, 73.8567),
        ["ahmedabad"] = (23.0225, 72.5714),
        ["jaipur"] = (26.9124, 75.7873),
        ["lucknow"] = (26.8467, 80.9462)
    };

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

        if (!TryResolveCenter(request, out var centerLat, out var centerLng))
        {
            return Task.FromResult(new List<HouseLocation>());
        }

        var centered = _houses
            .Where(h => DistanceKm(centerLat, centerLng, h.Lat, h.Lng) <= radiusKm)
            .Take(1500)
            .ToList();

        return Task.FromResult(centered);
    }

    private static bool TryResolveCenter(MapSearchRequest request, out double lat, out double lng)
    {
        if (request.CenterLat is double centerLat && request.CenterLng is double centerLng)
        {
            lat = centerLat;
            lng = centerLng;
            return true;
        }

        var postalCode = request.PostalCode?.Trim();
        if (!string.IsNullOrWhiteSpace(postalCode))
        {
            if (PinCenters.TryGetValue(postalCode, out var exactPinCenter))
            {
                lat = exactPinCenter.Lat;
                lng = exactPinCenter.Lng;
                return true;
            }

            if (postalCode.Length >= 3)
            {
                var prefix = postalCode[..3];
                if (PinPrefixCenters.TryGetValue(prefix, out var prefixCenter))
                {
                    lat = prefixCenter.Lat;
                    lng = prefixCenter.Lng;
                    return true;
                }
            }
        }

        var city = request.City?.Trim();
        var state = request.State?.Trim();
        if (!string.IsNullOrWhiteSpace(city) && !string.IsNullOrWhiteSpace(state))
        {
            var cityStateKey = $"{city.ToLowerInvariant()}|{state.ToLowerInvariant()}";
            if (CityCenters.TryGetValue(cityStateKey, out var cityStateCenter))
            {
                lat = cityStateCenter.Lat;
                lng = cityStateCenter.Lng;
                return true;
            }
        }

        if (!string.IsNullOrWhiteSpace(city) && CityCenters.TryGetValue(city.ToLowerInvariant(), out var cityCenter))
        {
            lat = cityCenter.Lat;
            lng = cityCenter.Lng;
            return true;
        }

        lat = 0;
        lng = 0;
        return false;
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
