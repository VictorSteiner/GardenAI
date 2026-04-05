using HomeAssistant.Domain.Common.Markers;

namespace HomeAssistant.Application.Dispatching;

/// <summary>Dispatches commands through a bounded channel with a concurrency ceiling suited to Raspberry Pi 5.</summary>
public interface ICommandDispatcher
{
    /// <summary>Enqueues a command for asynchronous execution.</summary>
    Task DispatchAsync<TCommand>(TCommand command, CancellationToken ct = default)
        where TCommand : ICommand;
}
