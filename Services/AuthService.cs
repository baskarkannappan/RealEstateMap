using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using RealEstateMap.Models;

namespace RealEstateMap.Services;

public sealed class AuthService : IAuthService
{
    private readonly ApiOptions _apiOptions;
    private readonly ILogger<AuthService> _logger;
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    private string? _cachedToken;
    private DateTime _tokenExpiryUtc;

    public AuthService(IOptions<ApiOptions> apiOptions, ILogger<AuthService> logger)
    {
        _apiOptions = apiOptions.Value;
        _logger = logger;
        _httpClient = new HttpClient { BaseAddress = new Uri(_apiOptions.BaseUrl) };
    }

    public async Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (TokenIsValid())
        {
            return _cachedToken;
        }

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (TokenIsValid())
            {
                return _cachedToken;
            }

            var request = new LoginRequest
            {
                Username = _apiOptions.Username,
                Password = _apiOptions.Password
            };

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsJsonAsync("api/auth/login", request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to call auth endpoint.");
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Authentication failed with status code {StatusCode}.", (int)response.StatusCode);
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
            if (payload is null || string.IsNullOrWhiteSpace(payload.Token))
            {
                _logger.LogWarning("Authentication response was empty.");
                return null;
            }

            _cachedToken = payload.Token;
            _tokenExpiryUtc = payload.ExpiresUtc;
            return _cachedToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving token.");
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

    private bool TokenIsValid() =>
        !string.IsNullOrWhiteSpace(_cachedToken) &&
        _tokenExpiryUtc > DateTime.UtcNow.AddMinutes(1);
}
