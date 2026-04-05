using System.Diagnostics.Metrics;
using HomeAssistant.Domain.PotConfigurations.Abstractions;
using HomeAssistant.Domain.PotConfigurations.Entities;
using HomeAssistant.Domain.SensorReadings.Entities;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Application.PotConfigurations.Services;

/// <summary>Calculates harvest readiness scores using formula-based logic without persisting scores.</summary>
public sealed class HarvestReadinessCalculator : IHarvestReadinessCalculator
{
    private readonly ILogger<HarvestReadinessCalculator> _logger;
    private readonly Histogram<int> _readinessScoreHistogram;

    /// <summary>Initialises the calculator with logging and metrics.</summary>
    public HarvestReadinessCalculator(ILogger<HarvestReadinessCalculator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var meter = new Meter("HomeAssistant.Application.PotConfigurations");
        _readinessScoreHistogram = meter.CreateHistogram<int>("harvest_readiness_score", description: "Histogram of harvest readiness scores");
    }

    /// <inheritdoc/>
    public int CalculateReadinessScore(SeedAssignment seed, IReadOnlyList<SensorReading> recentReadings)
    {
        ArgumentNullException.ThrowIfNull(seed);
        ArgumentNullException.ThrowIfNull(recentReadings);

        // Base score: (days_since_planting / days_to_harvest) * 100, clamped 0–100
        var daysSincePlanting = (int)(DateTimeOffset.UtcNow - seed.PlantedDate).TotalDays;
        var daysToHarvest = seed.ExpectedHarvestDate.HasValue
            ? (int)(seed.ExpectedHarvestDate.Value - seed.PlantedDate).TotalDays
            : 90; // Default to 90 days if not specified

        var baseScore = daysToHarvest > 0
            ? Math.Min(100, (int)((daysSincePlanting / (double)daysToHarvest) * 100))
            : 100;

        var score = baseScore;

        // Apply bonuses and penalties based on recent readings
        if (recentReadings.Count >= 3)
        {
            var lastThreeReadings = recentReadings.Take(3).ToList();
            
            // Check if last 3 readings are in ideal range (bonus +10%)
            var allInIdealRange = lastThreeReadings.All(r =>
                r.SoilMoisture >= 40 && r.SoilMoisture <= 80 &&  // Example ideal range
                r.TemperatureC >= 15 && r.TemperatureC <= 28);   // Example ideal range

            if (allInIdealRange)
            {
                score += 10;
                _logger.LogDebug("Applied +10% bonus: last 3 readings in ideal range for seed {SeedId}.", seed.Id);
            }

            // Check for declining trend in moisture (penalty –15%)
            var hasDecliningTrend = lastThreeReadings.Count >= 3 &&
                lastThreeReadings[0].SoilMoisture < lastThreeReadings[1].SoilMoisture &&
                lastThreeReadings[1].SoilMoisture < lastThreeReadings[2].SoilMoisture;

            if (hasDecliningTrend)
            {
                score -= 15;
                _logger.LogDebug("Applied –15% penalty: declining moisture trend for seed {SeedId}.", seed.Id);
            }
        }

        // Clamp final score to 0–100
        score = Math.Max(0, Math.Min(100, score));

        _logger.LogDebug(
            "Calculated harvest readiness: seed={SeedId}, plant={PlantName}, baseScore={BaseScore}, finalScore={FinalScore}",
            seed.Id, seed.PlantName, baseScore, score);

        // Emit histogram metric
        _readinessScoreHistogram.Record(score);

        return score;
    }

    /// <inheritdoc/>
    public string MapScoreToCategory(int score)
    {
        return score switch
        {
            >= 91 => "ready-now",
            >= 71 => "ready-soon",
            >= 31 => "ripening",
            _ => "not-ready",
        };
    }

    /// <inheritdoc/>
    public string DetermineHealthStatus(
        int averageReadiness,
        SensorReading? latestReading,
        double idealMoistureMin,
        double idealMoistureMax,
        double idealTempMinC,
        double idealTempMaxC)
    {
        // Healthy: average readiness ≥ 70 AND latest moisture/temp in ideal range
        if (averageReadiness >= 70 && latestReading is not null &&
            latestReading.SoilMoisture >= idealMoistureMin &&
            latestReading.SoilMoisture <= idealMoistureMax &&
            latestReading.TemperatureC >= idealTempMinC &&
            latestReading.TemperatureC <= idealTempMaxC)
        {
            return "healthy";
        }

        // Critical: average readiness < 30 OR extreme out-of-range readings
        if (averageReadiness < 30 || (latestReading is not null &&
            (latestReading.SoilMoisture < idealMoistureMin - 10 ||
             latestReading.SoilMoisture > idealMoistureMax + 10 ||
             latestReading.TemperatureC < idealTempMinC - 5 ||
             latestReading.TemperatureC > idealTempMaxC + 5)))
        {
            return "critical";
        }

        // Warning: everything else (30–69 readiness or slight variance)
        return "warning";
    }
}

