using System.Net.Http.Headers;
using System.Text.Json;
using GardenAI.Application.Area.Contracts;
using GardenAI.Application.Common.Sync.Abstractions;
using GardenAI.Application.Common.Sync.Configuration;
using GardenAI.Application.Device.Contracts;
using GardenAI.Application.Entity.Contracts;
using GardenAI.Infrastructure.HomeAssistant.Common.Exceptions;
using Microsoft.Extensions.Logging;

namespace GardenAI.Infrastructure.HomeAssistant.Rest.Clients;

/// <summary>HTTP client for Home Assistant registry endpoints.</summary>
public sealed class HomeAssistantRestClient : IHomeAssistantRestClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly HomeAssistantOptions _options;
    private readonly ILogger<HomeAssistantRestClient> _logger;

    public HomeAssistantRestClient(
        HttpClient httpClient,
        HomeAssistantOptions options,
        ILogger<HomeAssistantRestClient> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public Task<IReadOnlyList<HaAreaDto>> GetAreasAsync(CancellationToken ct = default)
        => GetListAsync<HaAreaDto>("api/config/area_registry/list", ct);

    public Task<IReadOnlyList<HaDeviceDto>> GetDevicesAsync(CancellationToken ct = default)
        => GetListAsync<HaDeviceDto>("api/config/device_registry/list", ct);

    public Task<IReadOnlyList<HaEntityDto>> GetEntitiesAsync(CancellationToken ct = default)
        => GetListAsync<HaEntityDto>("api/config/entity_registry/list", ct);

    private async Task<IReadOnlyList<T>> GetListAsync<T>(string relativePath, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, relativePath);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        try
        {
            using var response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            var payload = await JsonSerializer.DeserializeAsync<List<T>>(stream, JsonOptions, ct).ConfigureAwait(false)
                ?? new List<T>();

            _logger.LogInformation(
                "Fetched {Count} items from Home Assistant endpoint {Path}",
                payload.Count,
                relativePath);

            return payload;
        }
        catch (HttpRequestException ex)
        {
            throw new HomeAssistantApiException($"Home Assistant request failed for '{relativePath}'.", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new HomeAssistantApiException($"Home Assistant request timed out for '{relativePath}'.", ex);
        }
    }
}

