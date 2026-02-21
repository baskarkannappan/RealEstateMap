using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RealEstateMap.Api.Options;

namespace RealEstateMap.Api.Services.Caching;

public sealed class HybridCacheService : ICacheService
{
    private const int LockStripeCount = 64;
    private const string RegionPrefix = "cache:region:";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache? _distributedCache;
    private readonly CacheOptions _options;
    private readonly ILogger<HybridCacheService> _logger;
    private readonly SemaphoreSlim[] _keyLocks;
    private readonly ConcurrentDictionary<string, long> _regionVersions = new(StringComparer.Ordinal);

    public HybridCacheService(
        IMemoryCache memoryCache,
        IServiceProvider serviceProvider,
        IOptions<CacheOptions> options,
        ILogger<HybridCacheService> logger)
    {
        _memoryCache = memoryCache;
        _options = options.Value;
        _logger = logger;
        _distributedCache = _options.EnableDistributedCache ? serviceProvider.GetService<IDistributedCache>() : null;
        _keyLocks = Enumerable.Range(0, LockStripeCount).Select(_ => new SemaphoreSlim(1, 1)).ToArray();
    }

    public async Task<T> GetOrCreateAsync<T>(
        string region,
        string key,
        Func<CancellationToken, Task<T>> factory,
        CacheEntryPolicy policy,
        CancellationToken cancellationToken)
    {
        var regionVersion = await GetRegionVersionAsync(region, cancellationToken);
        var fullKey = BuildEntryKey(region, regionVersion, key);
        if (_memoryCache.TryGetValue<T>(fullKey, out var cached) && cached is not null)
        {
            return cached;
        }

        if (_distributedCache is not null)
        {
            var distributedValue = await TryGetFromDistributedAsync<T>(fullKey, cancellationToken);
            if (distributedValue is not null)
            {
                SetMemory(fullKey, distributedValue, policy);
                return distributedValue;
            }
        }

        var keyLock = GetLock(fullKey);
        await keyLock.WaitAsync(cancellationToken);
        try
        {
            if (_memoryCache.TryGetValue<T>(fullKey, out cached) && cached is not null)
            {
                return cached;
            }

            if (_distributedCache is not null)
            {
                var distributedValue = await TryGetFromDistributedAsync<T>(fullKey, cancellationToken);
                if (distributedValue is not null)
                {
                    SetMemory(fullKey, distributedValue, policy);
                    return distributedValue;
                }
            }

            var created = await factory(cancellationToken);
            SetMemory(fullKey, created, policy);

            if (_distributedCache is not null)
            {
                await TrySetDistributedAsync(fullKey, created, policy, cancellationToken);
            }

            return created;
        }
        finally
        {
            keyLock.Release();
        }
    }

    public async Task InvalidateRegionAsync(string region, CancellationToken cancellationToken)
    {
        var version = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _regionVersions[region] = version;

        if (_distributedCache is null)
        {
            return;
        }

        try
        {
            var versionKey = BuildRegionVersionKey(region);
            var bytes = Encoding.UTF8.GetBytes(version.ToString());
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            };

            await _distributedCache.SetAsync(versionKey, bytes, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate region {Region} in distributed cache", region);
        }
    }

    private async Task<long> GetRegionVersionAsync(string region, CancellationToken cancellationToken)
    {
        if (_regionVersions.TryGetValue(region, out var version))
        {
            return version;
        }

        if (_distributedCache is not null)
        {
            try
            {
                var payload = await _distributedCache.GetAsync(BuildRegionVersionKey(region), cancellationToken);
                if (payload is { Length: > 0 } &&
                    long.TryParse(Encoding.UTF8.GetString(payload), out var distributedVersion))
                {
                    _regionVersions[region] = distributedVersion;
                    return distributedVersion;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read region version for {Region} from distributed cache", region);
            }
        }

        var initial = 1L;
        _regionVersions[region] = initial;

        if (_distributedCache is not null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
                };
                await _distributedCache.SetAsync(
                    BuildRegionVersionKey(region),
                    Encoding.UTF8.GetBytes(initial.ToString()),
                    options,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set initial region version for {Region} in distributed cache", region);
            }
        }

        return initial;
    }

    private async Task<T?> TryGetFromDistributedAsync<T>(string key, CancellationToken cancellationToken)
    {
        try
        {
            var payload = await _distributedCache!.GetAsync(key, cancellationToken);
            if (payload is null || payload.Length == 0)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(payload, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Distributed cache read failed for key {CacheKey}", key);
            return default;
        }
    }

    private async Task TrySetDistributedAsync<T>(
        string key,
        T value,
        CacheEntryPolicy policy,
        CancellationToken cancellationToken)
    {
        try
        {
            var payload = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
            if (payload.Length > _options.MaxPayloadBytes)
            {
                return;
            }

            var distributedOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = policy.AbsoluteExpiration,
                SlidingExpiration = policy.SlidingExpiration
            };
            await _distributedCache!.SetAsync(key, payload, distributedOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Distributed cache write failed for key {CacheKey}", key);
        }
    }

    private void SetMemory<T>(string key, T value, CacheEntryPolicy policy)
    {
        var memoryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = policy.AbsoluteExpiration,
            SlidingExpiration = policy.SlidingExpiration,
            Size = policy.Size
        };

        _memoryCache.Set(key, value, memoryOptions);
    }

    private static string BuildRegionVersionKey(string region) => $"{RegionPrefix}{region}:version";

    private static string BuildEntryKey(string region, long version, string key) => $"{RegionPrefix}{region}:v{version}:{key}";

    private SemaphoreSlim GetLock(string key)
    {
        var hash = unchecked((uint)key.GetHashCode());
        return _keyLocks[(int)(hash % LockStripeCount)];
    }
}
