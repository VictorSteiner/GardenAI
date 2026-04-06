using HomeAssistant.Domain.PotConfigurations.Entities;
using HomeAssistant.Domain.SensorReadings.Entities;

namespace HomeAssistant.Domain.PotConfigurations.Abstractions;

/// <summary>Computes harvest readiness scores (0–100) based on seed lifecycle and sensor trends.</summary>
public interface IHarvestReadinessCalculator
{
    /// <summary>
    /// Calculates a harvest readiness score for a given seed assignment.
    /// 
    /// Formula:
    /// - Base: (days_since_planting / days_to_harvest) * 100, clamped 0–100
    /// - Bonus: +10% if moisture/temp in ideal range for last 3 readings
    /// - Penalty: –15% if declining trend detected in last 3 readings
    /// - Final score clamped at 100
    /// 
    /// Returns a score from 0–100.
    /// </summary>
    /// <param name="seed">The seed assignment to evaluate.</param>
    /// <param name="recentReadings">Recent sensor readings for this pot (ordered by timestamp descending).</param>
    /// <returns>Harvest readiness score (0–100).</returns>
    int CalculateReadinessScore(SeedAssignment seed, IReadOnlyList<SensorReading> recentReadings);

    /// <summary>
    /// Maps a readiness score to a category label.
    /// 
    /// Categories:
    /// - 0–30: "not-ready"
    /// - 31–70: "ripening"
    /// - 71–90: "ready-soon"
    /// - 91–100: "ready-now"
    /// </summary>
    /// <param name="score">The readiness score (0–100).</param>
    /// <returns>Category string ("not-ready", "ripening", "ready-soon", or "ready-now").</returns>
    string MapScoreToCategory(int score);

    /// <summary>
    /// Determines health status based on average readiness and sensor ranges.
    /// 
    /// Health statuses:
    /// - "healthy": average readiness ≥ 70 AND latest moisture/temp in ideal range
    /// - "warning": average readiness 30–69 OR slight range variance
    /// - "critical": average readiness &lt; 30 OR extreme out-of-range readings
    /// </summary>
    /// <param name="averageReadiness">Average readiness score across seeds in the pot.</param>
    /// <param name="latestReading">Most recent sensor reading (if any).</param>
    /// <param name="idealMoistureMin">Ideal minimum soil moisture (%).</param>
    /// <param name="idealMoistureMax">Ideal maximum soil moisture (%).</param>
    /// <param name="idealTempMinC">Ideal minimum temperature (°C).</param>
    /// <param name="idealTempMaxC">Ideal maximum temperature (°C).</param>
    /// <returns>Health status string ("healthy", "warning", or "critical").</returns>
    string DetermineHealthStatus(
        int averageReadiness,
        SensorReading latestReading,
        double idealMoistureMin,
        double idealMoistureMax,
        double idealTempMinC,
        double idealTempMaxC);
}

