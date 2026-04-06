namespace GardenAI.Infrastructure.HomeAssistant.Common.Exceptions;

/// <summary>Represents failures when communicating with the Home Assistant API.</summary>
public sealed class HomeAssistantApiException : Exception
{
    public HomeAssistantApiException(string message)
        : base(message)
    {
    }

    public HomeAssistantApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

