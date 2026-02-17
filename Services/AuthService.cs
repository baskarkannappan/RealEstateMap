using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using RealEstateMap.Models;

namespace RealEstateMap.Services;

public sealed class AuthService : IAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApiOptions _apiOptions;
    private readonly ILogger<AuthService> _logger;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    private string? _cachedToken;
    private DateTime _tokenExpiryUtc;

    public AuthService(
        IHttpClientFactory httpClientFactory,
        IOptions<ApiOptions> apiOptions,
        ILogger<AuthService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _apiOptions = apiOptions.Value;
        _logger = logger;
    }

    public async Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (HasValidToken())
        {
            return _cachedToken;
        }

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (HasValidToken())
            {
                return _cachedToken;
            }

            return await RequestTokenWithRetryAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Token retrieval canceled.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected token retrieval error.");
            return null;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    public void InvalidateToken()
    {
        _cachedToken = null;
        _tokenExpiryUtc = DateTime.MinValue;
    }

    private async Task<string?> RequestTokenWithRetryAsync(CancellationToken cancellationToken)
    {
        // Retry once to handle transient failures.
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            var token = await RequestTokenAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(token))
            {
                return token;
            }

            if (attempt == 1)
            {
                await Task.Delay(150, cancellationToken);
            }
        }

        return null;
    }

    private async Task<string?> RequestTokenAsync(CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("ApiClient");
        var request = new LoginRequest { PublicApiKey = _apiOptions.PublicApiKey };

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync("api/auth/login", request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Auth endpoint call failed.");
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Auth endpoint returned status code {StatusCode}.", (int)response.StatusCode);
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
        if (payload is null || string.IsNullOrWhiteSpace(payload.Token))
        {
            _logger.LogWarning("Auth endpoint returned an empty token payload.");
            return null;
        }

        _cachedToken = payload.Token;
        _tokenExpiryUtc = payload.ExpiresUtc;
        return _cachedToken;
    }

    private bool HasValidToken() =>
        !string.IsNullOrWhiteSpace(_cachedToken) &&
        _tokenExpiryUtc > DateTime.UtcNow.AddMinutes(1);
}
