using System.Text.Json;
using HomeAssistant.Infrastructure.HomeAssistant.Protocol.Abstractions;
using HomeAssistant.Infrastructure.HomeAssistant.Protocol.Contracts;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Infrastructure.HomeAssistant.Protocol.Services;

/// <summary>Reads Home Assistant area registry data from the configured Home Assistant config path.</summary>
public sealed class HomeAssistantAreaProvider : IHomeAssistantAreaProvider
{
    private const string AreaRegistryRelativePath = ".storage/area_registry";
    private readonly ILogger<HomeAssistantAreaProvider> _logger;
    private readonly string? _configPath;

    /// <summary>Creates a new <see cref="HomeAssistantAreaProvider"/>.</summary>
    public HomeAssistantAreaProvider(ILogger<HomeAssistantAreaProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configPath = Environment.GetEnvironmentVariable("HOMEASSISTANT_CONFIG_PATH");
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<HomeAssistantArea>> GetAvailableAreasAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_configPath))
        {
            _logger.LogWarning("HOMEASSISTANT_CONFIG_PATH not set; returning an empty Home Assistant area list.");
            return [];
        }

        var registryPath = Path.Combine(_configPath, AreaRegistryRelativePath);
        if (!File.Exists(registryPath))
        {
            _logger.LogWarning("Home Assistant area registry was not found at {Path}.", registryPath);
            return [];
        }

        try
        {
            var content = await File.ReadAllTextAsync(registryPath, ct).ConfigureAwait(false);
            using var document = JsonDocument.Parse(content);
            if (!document.RootElement.TryGetProperty("data", out var dataElement) || dataElement.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("Home Assistant area registry at {Path} did not contain a top-level data array.", registryPath);
                return [];
            }

            var areas = new List<HomeAssistantArea>();
            foreach (var areaElement in dataElement.EnumerateArray())
            {
                if (!areaElement.TryGetProperty("id", out var idElement) || idElement.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                if (!areaElement.TryGetProperty("name", out var nameElement) || nameElement.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var id = idElement.GetString();
                var name = nameElement.GetString();
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                areas.Add(new HomeAssistantArea(id, name));
            }

            _logger.LogInformation("Loaded {AreaCount} Home Assistant areas.", areas.Count);
            return areas.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read Home Assistant area registry from {Path}.", registryPath);
            return [];
        }
    }
}

