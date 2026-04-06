namespace HomeAssistant.Application.GardenAdvisor.Contracts.Advice;

/// <summary>Generated advisory summary for current pot readings and forecast context.</summary>
public sealed record GardenAdviceResponse(
    DateTimeOffset GeneratedAtUtc,
    string Region,
    GardenWeatherSnapshotResponse Weather,
    IReadOnlyList<GardenPotInsightResponse> Pots,
    string RecommendationSummary);

