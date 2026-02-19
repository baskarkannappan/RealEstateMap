namespace RealEstateMap.Api.Options;

public sealed class CacheOptions
{
    public const string SectionName = "Cache";

    public bool EnableDistributedCache { get; set; }
    public string? RedisConnectionString { get; set; }
    public int MemorySizeLimit { get; set; } = 1024;
    public int AbsoluteExpirationMinutes { get; set; } = 10;
    public int SlidingExpirationMinutes { get; set; } = 2;
    public int MaxPayloadBytes { get; set; } = 262_144;
    public bool WarmupOnStartup { get; set; } = true;
    public int WarmupPageSize { get; set; } = 200;
}
