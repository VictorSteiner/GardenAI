using HomeAssistant.Application.PotConfigurations.DTOs;
using HomeAssistant.Domain.Common.Handlers;
using HomeAssistant.Domain.PotConfigurations.Abstractions;
using HomeAssistant.Domain.SensorReadings.Abstractions;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Application.PotConfigurations.Queries;

/// <summary>Handles GetHarvestReadinessQuery by computing readiness for all seeds and filtering by optional status.</summary>
public sealed class GetHarvestReadinessQueryHandler : IQueryHandler<GetHarvestReadinessQuery, IReadOnlyList<HarvestReadinessDto>>
{
    private readonly IPotConfigurationRepository _potRepository;
    private readonly ISensorReadingRepository _sensorRepository;
    private readonly IHarvestReadinessCalculator _calculator;
    private readonly ILogger<GetHarvestReadinessQueryHandler> _logger;

    /// <summary>Initialises the handler.</summary>
    public GetHarvestReadinessQueryHandler(
        IPotConfigurationRepository potRepository,
        ISensorReadingRepository sensorRepository,
        IHarvestReadinessCalculator calculator,
        ILogger<GetHarvestReadinessQueryHandler> logger)
    {
        _potRepository = potRepository ?? throw new ArgumentNullException(nameof(potRepository));
        _sensorRepository = sensorRepository ?? throw new ArgumentNullException(nameof(sensorRepository));
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<HarvestReadinessDto>> HandleAsync(GetHarvestReadinessQuery query, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var allConfigs = await _potRepository.GetAllAsync(ct);
        var result = new List<HarvestReadinessDto>();

        foreach (var config in allConfigs)
        {
            foreach (var seed in config.CurrentSeeds)
            {
                // Filter by status if provided
                if (!string.IsNullOrEmpty(query.FilterByStatus) && seed.Status != query.FilterByStatus)
                    continue;

                // Fetch recent readings for this pot (last 10)
                var recentReadings = await _sensorRepository.GetLatestReadingsByPotIdAsync(config.PotId, limit: 10, ct);

                // Calculate readiness score
                var readinessScore = _calculator.CalculateReadinessScore(seed, recentReadings);
                var readinessCategory = _calculator.MapScoreToCategory(readinessScore);

                // Compute time metrics
                var daysSincePlanting = (int)(DateTimeOffset.UtcNow - seed.PlantedDate).TotalDays;
                var daysUntilHarvest = seed.ExpectedHarvestDate.HasValue
                    ? (int)(seed.ExpectedHarvestDate.Value - DateTimeOffset.UtcNow).TotalDays
                    : (int?)null;

                var dto = new HarvestReadinessDto(
                    seed.Id,
                    config.PotId,
                    seed.PlantName,
                    seed.SeedName,
                    seed.Status,
                    readinessScore,
                    readinessCategory,
                    seed.PlantedDate,
                    seed.ExpectedHarvestDate,
                    daysSincePlanting,
                    daysUntilHarvest);

                result.Add(dto);
            }
        }

        _logger.LogDebug("Retrieved harvest readiness for {SeedCount} seeds (filtered by status: {FilterStatus}).",
            result.Count, query.FilterByStatus ?? "none");

        return result.AsReadOnly();
    }
}

