using System.Runtime.InteropServices;
using System.Text.Json;
using HomeAssistant.Presentation.GardenAdvisor.Contracts;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Presentation.GardenAdvisor.Services;

/// <summary>Service to read available rooms from Home Assistant's area registry configuration file.</summary>
public interface IHomeAssistantAreaProvider
{
    /// <summary>Reads and parses Home Assistant area registry to return available rooms.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Collection of available rooms (areas) from HA configuration.</returns>
    Task<IReadOnlyList<RoomResponse>> GetAvailableRoomsAsync(CancellationToken ct = default);
}

/// <summary>Default implementation that reads from Home Assistant configuration directory.</summary>
public sealed class HomeAssistantAreaProvider : IHomeAssistantAreaProvider
{
    private const string AreaRegistryFileName = "area_registry.json";
    private readonly ILogger<HomeAssistantAreaProvider> _logger;
    private readonly string? _haConfigPath;

    /// <summary>Initialises the provider with Home Assistant config path from environment.</summary>
    public HomeAssistantAreaProvider(ILogger<HomeAssistantAreaProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Check for Home Assistant config path in environment
        // Common locations: /home/homeassistant/.homeassistant or C:\Users\<user>\AppData\Roaming\homeassistant
        _haConfigPath = Environment.GetEnvironmentVariable("HOMEASSISTANT_CONFIG_PATH")
            ?? (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? "/home/homeassistant/.homeassistant"
                : null);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RoomResponse>> GetAvailableRoomsAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_haConfigPath))
        {
            _logger.LogWarning("HOMEASSISTANT_CONFIG_PATH not set; returning empty room list.");
            return [];
        }

        try
        {
            var areaRegistryPath = Path.Combine(_haConfigPath, AreaRegistryFileName);
            
            if (!File.Exists(areaRegistryPath))
            {
                _logger.LogWarning("Area registry file not found at {Path}; returning empty room list.", areaRegistryPath);
                return [];
            }

            var jsonContent = await File.ReadAllTextAsync(areaRegistryPath, ct);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            if (!root.TryGetProperty("areas", out var areasElement))
            {
                _logger.LogWarning("'areas' property not found in area registry.");
                return [];
            }

            var rooms = new List<RoomResponse>();
            foreach (var area in areasElement.EnumerateArray())
            {
                if (area.TryGetProperty("id", out var idElem) && 
                    area.TryGetProperty("name", out var nameElem) &&
                    idElem.ValueKind == JsonValueKind.String &&
                    nameElem.ValueKind == JsonValueKind.String)
                {
                    var areaId = idElem.GetString();
                    var areaName = nameElem.GetString();

                    if (!string.IsNullOrWhiteSpace(areaId) && !string.IsNullOrWhiteSpace(areaName))
                    {
                        rooms.Add(new RoomResponse(areaId, areaName));
                    }
                }
            }

            _logger.LogInformation("Loaded {RoomCount} rooms from Home Assistant area registry.", rooms.Count);
            return rooms.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read Home Assistant area registry; returning empty room list.");
            return [];
        }
    }
}


