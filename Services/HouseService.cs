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

    public Task<List<HouseLocation>> SearchAsync(MapSearchRequest request, CancellationToken cancellationToken = default) =>
        SendAsync(
            _ => new HttpRequestMessage(HttpMethod.Post, "api/house/search")
            {
                Content = JsonContent.Create(request)
            },
            cancellationToken);


    public Task<List<HouseLocation>> SearchByCenterAsync(
        double centerLat,
        double centerLng,
        double radiusKm,
        string? postalCode = null,
        string? city = null,
        string? state = null,
        CancellationToken cancellationToken = default)
    {
        var request = new MapSearchRequest
        {
            PostalCode = postalCode,
            City = city,
            State = state,
            CenterLat = centerLat,
            CenterLng = centerLng,
            RadiusKm = Math.Clamp(radiusKm, 1, 250)
        };

        return SearchAsync(request, cancellationToken);
    }

    public Task<List<HouseLocation>> GetBoundsAsync(double south, double west, double north, double east, CancellationToken cancellationToken = default) =>
        SendAsync(
            _ => new HttpRequestMessage(
                HttpMethod.Get,
                $"api/house/bounds?south={south:F6}&west={west:F6}&north={north:F6}&east={east:F6}"),
            cancellationToken);

    public Task<List<HouseLocation>> GetListAsync(MapSearchRequest request, CancellationToken cancellationToken = default) =>
        SendAsync(
            _ => new HttpRequestMessage(HttpMethod.Post, "api/house/list")
            {
                Content = JsonContent.Create(request)
            },
            cancellationToken);

    private async Task<List<HouseLocation>> SendAsync(
        Func<HttpClient, HttpRequestMessage> requestFactory,
        CancellationToken externalCancellationToken)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, externalCancellationToken);
        var ct = linkedCts.Token;

        try
        {
            for (var attempt = 1; attempt <= 2; attempt++)
            {
                var token = await _authService.GetTokenAsync(ct);
                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogWarning("Skipping API call because token acquisition failed.");
                    return [];
                }

                var client = _httpClientFactory.CreateClient("ApiClient");
                using var request = requestFactory(client);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                _logger.LogDebug("Calling API endpoint {Method} {Path}.", request.Method, request.RequestUri);
                using var response = await client.SendAsync(request, ct);

                if (response.StatusCode == HttpStatusCode.Unauthorized && attempt == 1)
                {
                    _logger.LogWarning("API returned 401, refreshing token and retrying once.");
                    _authService.InvalidateToken();
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API request failed with status code {StatusCode}.", (int)response.StatusCode);
                    return [];
                }

                var payload = await response.Content.ReadFromJsonAsync<List<HouseLocation>>(cancellationToken: ct);
                return payload ?? [];
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "API request canceled or timed out.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling API. Verify API URL, HTTPS cert trust and CORS policy.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected HouseService error.");
        }

        return [];
    }
}
