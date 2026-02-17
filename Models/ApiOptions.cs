namespace RealEstateMap.Models;

public sealed class ApiOptions
{
    public string BaseUrl { get; init; } = string.Empty;
    public string? PublicApiKey { get; init; }
}
