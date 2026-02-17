using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using RealEstateMap.Models;

namespace RealEstateMap.Services;

public sealed class HouseService : IHouseService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private readonly ILogger<HouseService> _logger;

    public HouseService(IOptions<ApiOptions> apiOptions, IAuthService authService, ILogger<HouseService> logger)
    {
        _authService = authService;
        _logger = logger;
        _httpClient = new HttpClient { BaseAddress = new Uri(apiOptions.Value.BaseUrl) };
    }

    public async Task<List<HouseLocation>> SearchAsync(MapSearchRequest request)
    {
        try
        {
            return await SendAuthorizedAsync(async ct =>
            {
                var response = await _httpClient.PostAsJsonAsync("api/house/search", request, ct);
                return await ReadResultAsync(response, ct);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search request failed.");
            return [];
        }
    }

    public async Task<List<HouseLocation>> GetBoundsAsync(double south, double west, double north, double east)
    {
        try
        {
            return await SendAuthorizedAsync(async ct =>
            {
                var url = $"api/house/bounds?south={south}&west={west}&north={north}&east={east}";
                var response = await _httpClient.GetAsync(url, ct);
                return await ReadResultAsync(response, ct);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bounds request failed.");
            return [];
        }
    }

    private async Task<List<HouseLocation>> SendAuthorizedAsync(Func<CancellationToken, Task<List<HouseLocation>>> action)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        var token = await _authService.GetTokenAsync(cts.Token);
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("No token available for API call.");
            return [];
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var result = await action(cts.Token);

        if (result.Count > 0)
        {
            return result;
        }

        return result;
    }

    private async Task<List<HouseLocation>> ReadResultAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _authService.InvalidateToken();
            var retryToken = await _authService.GetTokenAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(retryToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", retryToken);
            }

            _logger.LogWarning("Received 401 from API.");
            return [];
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("API call failed with status code {StatusCode}.", (int)response.StatusCode);
            return [];
        }

        var payload = await response.Content.ReadFromJsonAsync<List<HouseLocation>>(cancellationToken: cancellationToken);
        return payload ?? [];
    }
}
