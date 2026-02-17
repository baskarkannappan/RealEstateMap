namespace RealEstateMap.Models;

public sealed class ApiOptions
{
    public string BaseUrl { get; init; } = "https://localhost:7261/";
    public string? PublicApiKey { get; init; }
}
