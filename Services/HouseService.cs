using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RealEstateMap.Models;

namespace RealEstateMap.Services;

public sealed class HouseService : IHouseService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthService _authService;
    private readonly ILogger<HouseService> _logger;

    public HouseService(IHttpClientFactory httpClientFactory, IAuthService authService, ILogger<HouseService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _authService = authService;
        _logger = logger;
    }

    public Task<List<HouseLocation>> SearchAsync(MapSearchRequest request) =>
        SendAsync(client => new HttpRequestMessage(HttpMethod.Post, "api/house/search")
        {
            Content = JsonContent.Create(request)
        });

    public Task<List<HouseLocation>> GetBoundsAsync(double south, double west, double north, double east) =>
        SendAsync(client => new HttpRequestMessage(
            HttpMethod.Get,
            $"api/house/bounds?south={south:F6}&west={west:F6}&north={north:F6}&east={east:F6}"));

    private async Task<List<HouseLocation>> SendAsync(Func<HttpClient, HttpRequestMessage> requestFactory)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        try
        {
            // One normal try + one forced refresh try.
            for (var attempt = 1; attempt <= 2; attempt++)
            {
                var token = await _authService.GetTokenAsync(timeoutCts.Token);
                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogWarning("Skipping API call because no token is available.");
                    return [];
                }

                var client = _httpClientFactory.CreateClient("ApiClient");
                using var request = requestFactory(client);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var response = await client.SendAsync(request, timeoutCts.Token);

                if (response.StatusCode == HttpStatusCode.Unauthorized && attempt == 1)
                {
                    _logger.LogWarning("API returned 401. Invalidating token and retrying once.");
                    _authService.InvalidateToken();
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API request failed with status code {StatusCode}.", (int)response.StatusCode);
                    return [];
                }

                var payload = await response.Content.ReadFromJsonAsync<List<HouseLocation>>(cancellationToken: timeoutCts.Token);
                return payload ?? [];
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "API request canceled or timed out.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP failure while calling API.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected API service error.");
        }

        return [];
    }
}
