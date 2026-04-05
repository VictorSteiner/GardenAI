using HomeAssistant.Application.PotConfigurations.DTOs;
using HomeAssistant.Domain.Common.Handlers;
using HomeAssistant.Domain.PotConfigurations.Abstractions;
using HomeAssistant.Domain.SensorReadings.Abstractions;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Application.PotConfigurations.Queries;

/// <summary>Handles GetDashboardAggregationQuery by aggregating all rooms, pots, and seeds with health statuses.</summary>
public sealed class GetDashboardAggregationQueryHandler : IQueryHandler<GetDashboardAggregationQuery, DashboardAggregationDto>
{
    private readonly IPotConfigurationRepository _potRepository;
    private readonly ISensorReadingRepository _sensorRepository;
    private readonly IHarvestReadinessCalculator _calculator;
    private readonly ILogger<GetDashboardAggregationQueryHandler> _logger;

    // Default ideal ranges (used when profile is not available)
    private const double DefaultIdealMoistureMin = 40;
    private const double DefaultIdealMoistureMax = 80;
    private const double DefaultIdealTempMinC = 15;
    private const double DefaultIdealTempMaxC = 28;

    /// <summary>Initialises the handler.</summary>
    public GetDashboardAggregationQueryHandler(
        IPotConfigurationRepository potRepository,
        ISensorReadingRepository sensorRepository,
        IHarvestReadinessCalculator calculator,
        ILogger<GetDashboardAggregationQueryHandler> logger)
    {
        _potRepository = potRepository ?? throw new ArgumentNullException(nameof(potRepository));
        _sensorRepository = sensorRepository ?? throw new ArgumentNullException(nameof(sensorRepository));
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<DashboardAggregationDto> HandleAsync(GetDashboardAggregationQuery query, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var allConfigs = await _potRepository.GetAllAsync(ct);

        var roomsByAreaId = allConfigs.GroupBy(c => c.RoomAreaId).ToDictionary(g => g.Key, g => g.ToList());
        var rooms = new List<RoomDashboardDto>();

        var allReadinessScores = new List<int>();
        var readyToHarvestCount = 0;
        var ripeningCount = 0;
        var growingCount = 0;
        var criticalPotsCount = 0;

        foreach (var (roomAreaId, configs) in roomsByAreaId)
        {
            var pots = new List<PotDashboardDto>();
            var firstConfig = configs.First();

            foreach (var config in configs)
            {
                var recentReadings = await _sensorRepository.GetLatestReadingsByPotIdAsync(config.PotId, limit: 10, ct);
                var latestReading = recentReadings.FirstOrDefault();

                var seedDtos = new List<SeedDashboardDto>();
                var seedReadinessScores = new List<int>();

                foreach (var seed in config.CurrentSeeds)
                {
                    var readinessScore = _calculator.CalculateReadinessScore(seed, recentReadings);
                    var readinessCategory = _calculator.MapScoreToCategory(readinessScore);

                    seedDtos.Add(new SeedDashboardDto(
                        seed.Id,
                        seed.PlantName,
                        seed.SeedName,
                        seed.Status,
                        readinessScore,
                        readinessCategory));

                    seedReadinessScores.Add(readinessScore);

                    // Aggregate counts
                    if (readinessCategory == "ready-now") readyToHarvestCount++;
                    if (readinessCategory == "ripening") ripeningCount++;
                    if (seed.Status == "growing") growingCount++;
                }

                // Calculate average readiness for pot
                var potAverageReadiness = seedReadinessScores.Count > 0
                    ? (int)seedReadinessScores.Average()
                    : 0;

                allReadinessScores.AddRange(seedReadinessScores);

                // Determine health status using default ideal ranges
                var healthStatus = _calculator.DetermineHealthStatus(
                    potAverageReadiness,
                    latestReading,
                    DefaultIdealMoistureMin,
                    DefaultIdealMoistureMax,
                    DefaultIdealTempMinC,
                    DefaultIdealTempMaxC);

                if (healthStatus == "critical")
                    criticalPotsCount++;

                pots.Add(new PotDashboardDto(
                    config.PotId,
                    $"Pot {config.PotId:N}".Substring(0, 20), // Fallback label
                    seedDtos.AsReadOnly(),
                    potAverageReadiness,
                    healthStatus));
            }

            rooms.Add(new RoomDashboardDto(
                roomAreaId,
                firstConfig.RoomName,
                pots.AsReadOnly()));
        }

        // Determine overall system health
        var overallHealthStatus = criticalPotsCount > 0
            ? "critical"
            : (allReadinessScores.Count > 0 && allReadinessScores.Average() < 30)
                ? "warning"
                : "healthy";

        var result = new DashboardAggregationDto(
            DateTimeOffset.UtcNow,
            rooms.AsReadOnly(),
            overallHealthStatus,
            readyToHarvestCount,
            ripeningCount,
            growingCount,
            criticalPotsCount);

        _logger.LogDebug(
            "Generated dashboard aggregation: {RoomCount} rooms, {PotCount} pots, overall status={OverallStatus}",
            rooms.Count,
            rooms.SelectMany(r => r.Pots).Count(),
            overallHealthStatus);

        return result;
    }
}

