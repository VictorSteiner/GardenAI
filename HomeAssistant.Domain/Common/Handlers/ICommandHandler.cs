using HomeAssistant.Domain.Common.Markers;

namespace HomeAssistant.Domain.Common.Handlers;

/// <summary>Handles a command of type <typeparamref name="TCommand"/>.</summary>
public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    /// <summary>Executes the command asynchronously.</summary>
    Task HandleAsync(TCommand command, CancellationToken ct = default);
}
