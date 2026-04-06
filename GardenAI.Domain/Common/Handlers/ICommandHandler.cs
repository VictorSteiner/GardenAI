using GardenAI.Domain.Common.Markers;

namespace GardenAI.Domain.Common.Handlers;

/// <summary>Handles a command of type <typeparamref name="TCommand"/>.</summary>
public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    /// <summary>Executes the command asynchronously.</summary>
    Task HandleAsync(TCommand command, CancellationToken ct = default);
}
