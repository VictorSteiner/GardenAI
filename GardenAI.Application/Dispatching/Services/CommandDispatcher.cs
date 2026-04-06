using System.Threading.Channels;
using FluentValidation;
using GardenAI.Application.Dispatching.Abstractions;
using GardenAI.Domain.Common.Handlers;
using GardenAI.Domain.Common.Markers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GardenAI.Application.Dispatching.Services;


/// <summary>Channel-backed command dispatcher with a semaphore limiting concurrency to 4.
/// Runs any registered <see cref="IValidator{T}"/> inside the handler scope before executing the command.
/// </summary>
public sealed class CommandDispatcher : ICommandDispatcher, IAsyncDisposable
{
    private readonly Channel<Func<CancellationToken, Task>> _channel;
    private readonly SemaphoreSlim _semaphore = new(4, 4);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CommandDispatcher> _logger;
    private readonly Task _processorTask;
    private readonly CancellationTokenSource _cts = new();

    /// <summary>Initialises a new <see cref="CommandDispatcher"/> with a bounded channel capacity of 64.</summary>
    public CommandDispatcher(IServiceScopeFactory scopeFactory, ILogger<CommandDispatcher> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        _channel = Channel.CreateBounded<Func<CancellationToken, Task>>(
            new BoundedChannelOptions(64)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true
            });

        _processorTask = ProcessAsync(_cts.Token);
    }

    /// <inheritdoc/>
    public async Task DispatchAsync<TCommand>(TCommand command, CancellationToken ct = default)
        where TCommand : ICommand
    {
        ArgumentNullException.ThrowIfNull(command);

        await _channel.Writer.WriteAsync(async token =>
        {
            await _semaphore.WaitAsync(token);
            try
            {
                using var scope = _scopeFactory.CreateScope();

                // Fail-fast: validate before invoking the handler if a validator is registered
                var validator = scope.ServiceProvider.GetService<IValidator<TCommand>>();
                if (validator is not null)
                {
                    var result = await validator.ValidateAsync(command, token).ConfigureAwait(false);
                    if (!result.IsValid)
                    {
                        var errors = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
                        _logger.LogWarning(
                            "Command {CommandType} failed validation and was not dispatched. Errors: {Errors}",
                            typeof(TCommand).Name, errors);
                        return;
                    }
                }

                var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand>>();
                await handler.HandleAsync(command, token).ConfigureAwait(false);
                _logger.LogInformation("Command {CommandType} handled successfully.", typeof(TCommand).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling command {CommandType}.", typeof(TCommand).Name);
            }
            finally
            {
                _semaphore.Release();
            }
        }, ct);
    }

    private async Task ProcessAsync(CancellationToken ct)
    {
        await foreach (var work in _channel.Reader.ReadAllAsync(ct))
        {
            await work(ct);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        _channel.Writer.Complete();
        await _cts.CancelAsync();
        await _processorTask.ConfigureAwait(false);
        _semaphore.Dispose();
        _cts.Dispose();
    }
}


