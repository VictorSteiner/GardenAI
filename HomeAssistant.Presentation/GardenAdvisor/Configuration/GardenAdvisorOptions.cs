namespace HomeAssistant.Presentation.GardenAdvisor.Configuration;

/// <summary>Configuration options for garden advisory generation and scheduling.</summary>
public sealed class GardenAdvisorOptions
{
    /// <summary>Human-friendly region label used in advice generation.</summary>
    public string RegionName { get; init; } = "Unknown region";

    /// <summary>Latitude used for weather forecast queries.</summary>
    public double Latitude { get; init; }

    /// <summary>Longitude used for weather forecast queries.</summary>
    public double Longitude { get; init; }

    /// <summary>Timezone passed to weather forecast APIs.</summary>
    public string Timezone { get; init; } = "auto";

    /// <summary>MQTT topic for summary advice payload publication.</summary>
    public string AdviceSummaryTopic { get; init; } = "homeassistant/garden/advice/summary";

    /// <summary>MQTT topic prefix for per-pot advisory payloads.</summary>
    public string PotInsightTopicPrefix { get; init; } = "homeassistant/garden/pots";

    /// <summary>Whether automatic periodic advice generation is enabled.</summary>
    public bool EnableScheduledAdvice { get; init; } = true;

    /// <summary>MQTT topic for publishing garden planner chat responses.</summary>
    public string PlannerResponseTopic { get; init; } = "homeassistant/garden/planner/response";

    /// <summary>MQTT topic for publishing the full garden planner conversation history.</summary>
    public string PlannerHistoryTopic { get; init; } = "homeassistant/garden/planner/history";

    /// <summary>Maximum tool-calling iterations the planner agent may execute per chat turn.</summary>
    public int PlannerMaxIterations { get; init; } = 2;
}

