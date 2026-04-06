using HomeAssistant.Application.GardenAdvisor.Abstractions;
using HomeAssistant.Application.GardenAdvisor.Contracts;

namespace HomeAssistant.Application.GardenAdvisor.Services;

/// <summary>Thread-safe in-memory storage for the most recent garden advice payload.</summary>
public sealed class GardenAdviceStateStore : IGardenAdviceStateStore
{
    private readonly object _sync = new();
    private GardenAdviceResponse? _latest;

    /// <inheritdoc />
    public GardenAdviceResponse? GetLatest()
    {
        lock (_sync)
        {
            return _latest;
        }
    }

    /// <inheritdoc />
    public void SetLatest(GardenAdviceResponse advice)
    {
        ArgumentNullException.ThrowIfNull(advice);

        lock (_sync)
        {
            _latest = advice;
        }
    }
}

