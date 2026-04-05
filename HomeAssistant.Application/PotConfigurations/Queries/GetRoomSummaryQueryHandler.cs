using HomeAssistant.Application.PotConfigurations.DTOs;
using HomeAssistant.Domain.Common.Handlers;
using HomeAssistant.Domain.PotConfigurations.Abstractions;
using HomeAssistant.Domain.SensorReadings.Abstractions;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Application.PotConfigurations.Queries;

/// <summary>Handles GetRoomSummaryQuery by aggregating metrics for a specific room.</summary>
public sealed class GetRoomSummaryQueryHandler : IQueryHandler<GetRoomSummaryQuery, RoomSummaryDto>
{
    private readonly IPotConfigurationRepository _potRepository;
    private readonly ISensorReadingRepository _sensorRepository;
    private readonly IHarvestReadinessCalculator _calculator;
    private readonly ILogger<GetRoomSummaryQueryHandler> _logger;

    /// <summary>Initialises the handler.</summary>
    public GetRoomSummaryQueryHandler(
        IPotConfigurationRepository potRepository,
        ISensorReadingRepository sensorRepository,
        IHarvestReadinessCalculator calculator,
        ILogger<GetRoomSummaryQueryHandler> logger)
    {
        _potRepository = potRepository ?? throw new ArgumentNullException(nameof(potRepository));
        _sensorRepository = sensorRepository ?? throw new ArgumentNullException(nameof(sensorRepository));
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<RoomSummaryDto> HandleAsync(GetRoomSummaryQuery query, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNullOrEmpty(query.RoomAreaId);

        // Fetch all configurations for the room
        var configs = await _potRepository.GetByRoomAreaIdAsync(query.RoomAreaId, ct);

        if (configs.Count == 0)
        {
            _logger.LogWarning("Room {RoomAreaId} has no pot configurations.", query.RoomAreaId);
            return new RoomSummaryDto(
                query.RoomAreaId,
                string.Empty,
                0,
                0,
                0,
                "healthy",
                0,
                0);
        }

        var roomName = configs.First().RoomName;
        var readinessScores = new List<int>();
        var activeSeedCount = 0;
        var readyToHarvestCount = 0;
        var ripeningCount = 0;

        foreach (var config in configs)
        {
            foreach (var seed in config.CurrentSeeds)
            {
                activeSeedCount++;

                // Fetch recent readings for readiness calculation
                var recentReadings = await _sensorRepository.GetLatestReadingsByPotIdAsync(config.PotId, limit: 10, ct);
                var readinessScore = _calculator.CalculateReadinessScore(seed, recentReadings);
                var readinessCategory = _calculator.MapScoreToCategory(readinessScore);

                readinessScores.Add(readinessScore);

                if (readinessCategory == "ready-now") readyToHarvestCount++;
                if (readinessCategory == "ripening") ripeningCount++;
            }
        }

        // Calculate average readiness
        var averageReadiness = readinessScores.Count > 0
            ? (int)readinessScores.Average()
            : 0;

        // Determine health status
        var healthStatus = averageReadiness >= 70
            ? "healthy"
            : averageReadiness < 30
                ? "critical"
                : "warning";

        var result = new RoomSummaryDto(
            query.RoomAreaId,
            roomName,
            configs.Count,
            activeSeedCount,
            averageReadiness,
            healthStatus,
            readyToHarvestCount,
            ripeningCount);

        _logger.LogDebug(
            "Generated room summary: room={RoomAreaId}, pots={PotCount}, seeds={SeedCount}, avgReadiness={AvgReadiness}",
            query.RoomAreaId,
            configs.Count,
            activeSeedCount,
            averageReadiness);

        return result;
    }
}

