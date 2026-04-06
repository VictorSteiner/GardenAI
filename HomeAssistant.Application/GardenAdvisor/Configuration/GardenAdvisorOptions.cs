namespace HomeAssistant.Application.GardenAdvisor.Configuration;

/// <summary>Configuration options for garden advisory generation and scheduling.</summary>
public sealed class GardenAdvisorOptions
{
    public string RegionName { get; init; } = "Unknown region";

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public string Timezone { get; init; } = "auto";

    public string AdviceSummaryTopic { get; init; } = "homeassistant/garden/advice/summary";

    public string PotInsightTopicPrefix { get; init; } = "homeassistant/garden/pots";

    public bool EnableScheduledAdvice { get; init; } = true;

    public string PlannerResponseTopic { get; init; } = "homeassistant/garden/planner/response";

    public string PlannerHistoryTopic { get; init; } = "homeassistant/garden/planner/history";

    public int PlannerMaxIterations { get; init; } = 2;
}

