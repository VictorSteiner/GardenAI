namespace HomeAssistant.Presentation.Garden.Configuration;
/// <summary>Configuration options for the garden advisor and planner.</summary>
public sealed class GardenOptions
{
    public string RegionName { get; init; } = "Unknown";
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string Timezone { get; init; } = "UTC";
    public string AdviceSummaryTopic { get; init; } = string.Empty;
    public string PotInsightTopicPrefix { get; init; } = string.Empty;
    public string PlannerResponseTopic { get; init; } = string.Empty;
    public string PlannerHistoryTopic { get; init; } = string.Empty;
    public int PlannerMaxIterations { get; init; } = 2;
    public bool EnableScheduledAdvice { get; init; } = true;
}
