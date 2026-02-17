namespace RealEstateMap.Models;

public sealed class LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresUtc { get; init; }
}
