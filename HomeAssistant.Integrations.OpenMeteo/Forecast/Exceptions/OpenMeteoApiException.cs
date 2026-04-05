using System.Net;

namespace HomeAssistant.Integrations.OpenMeteo.Forecast.Exceptions;

/// <summary>Represents a non-success response from the Open-Meteo API.</summary>
public sealed class OpenMeteoApiException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="OpenMeteoApiException"/> class.</summary>
    public OpenMeteoApiException(HttpStatusCode statusCode, string requestPath, string responseBody)
        : base($"Open-Meteo API request failed with status {(int)statusCode} for '{requestPath}'.")
    {
        StatusCode = statusCode;
        RequestPath = requestPath;
        ResponseBody = responseBody;
    }

    /// <summary>HTTP status code returned by the API.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>Relative request path that failed.</summary>
    public string RequestPath { get; }

    /// <summary>Raw response body returned by the API.</summary>
    public string ResponseBody { get; }

    /// <summary>Returns a detailed string representation of the exception.</summary>
    public override string ToString() =>
        $"{base.ToString()}{Environment.NewLine}StatusCode: {(int)StatusCode}{Environment.NewLine}RequestPath: {RequestPath}{Environment.NewLine}ResponseBody: {ResponseBody}";
}
