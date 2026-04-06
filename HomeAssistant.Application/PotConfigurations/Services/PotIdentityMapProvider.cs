using HomeAssistant.Application.PotConfigurations.Abstractions;
using HomeAssistant.Application.PotConfigurations.Configuration;
using HomeAssistant.Domain.PotConfigurations.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HomeAssistant.Application.PotConfigurations.Services;

/// <summary>Resolves configured pot-number mappings and validates map quality at runtime.</summary>
public sealed class PotIdentityMapProvider : IPotIdentityMapProvider
{
    private readonly PotIdentityMapOptions _options;
    private readonly IPotConfigurationRepository _potConfigurationRepository;
    private readonly ILogger<PotIdentityMapProvider> _logger;

    /// <summary>Creates a new <see cref="PotIdentityMapProvider"/>.</summary>
    public PotIdentityMapProvider(
        IOptions<PotIdentityMapOptions> options,
        IPotConfigurationRepository potConfigurationRepository,
        ILogger<PotIdentityMapProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
        _potConfigurationRepository = potConfigurationRepository ?? throw new ArgumentNullException(nameof(potConfigurationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Guid?> ResolvePotIdAsync(int potNumber, CancellationToken ct = default)
    {
        if (potNumber <= 0)
            return null;

        var map = await GetMapAsync(ct);
        return map.TryGetValue(potNumber, out var potId) ? potId : null;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<int, Guid>> GetMapAsync(CancellationToken ct = default)
    {
        ValidateConfiguredMap();
        await ValidateAgainstPersistedConfigurationsAsync(ct);
        return _options.PotNumberToId;
    }

    private void ValidateConfiguredMap()
    {
        if (_options.PotNumberToId.Count == 0)
        {
            _logger.LogWarning("Pot identity mapping is empty. Configure PotIdentityMap:PotNumberToId.");
            return;
        }

        foreach (var key in _options.PotNumberToId.Keys)
        {
            if (key <= 0)
                _logger.LogWarning("Pot identity mapping contains invalid pot number {PotNumber}.", key);
        }

        var duplicateIds = _options.PotNumberToId
            .GroupBy(kv => kv.Value)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        foreach (var duplicateId in duplicateIds)
            _logger.LogWarning("Pot identity mapping reuses pot id {PotId} across multiple pot numbers.", duplicateId);

        if (_options.ExpectedPotNumbers.Count == 0)
            return;

        foreach (var expectedPotNumber in _options.ExpectedPotNumbers)
        {
            if (!_options.PotNumberToId.ContainsKey(expectedPotNumber))
                _logger.LogWarning("Pot identity mapping is missing expected pot number {PotNumber}.", expectedPotNumber);
        }
    }

    private async Task ValidateAgainstPersistedConfigurationsAsync(CancellationToken ct)
    {
        var configurations = await _potConfigurationRepository.GetAllAsync(ct);
        if (configurations.Count == 0)
            return;

        var mappedPotIds = _options.PotNumberToId.Values.ToHashSet();
        foreach (var configuredPot in configurations)
        {
            if (!mappedPotIds.Contains(configuredPot.PotId))
            {
                _logger.LogWarning(
                    "Persisted pot configuration {PotId} is not present in PotIdentityMap:PotNumberToId.",
                    configuredPot.PotId);
            }
        }
    }
}

