using HomeAssistant.Domain.Common.Abstractions;
using HomeAssistant.Domain.Common.Handlers;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Application.Maintenance.Commands.ResetPersistedData;

/// <summary>Handles persisted-data reset requests.</summary>
public sealed class ResetPersistedDataCommandHandler : ICommandHandler<ResetPersistedDataCommand>
{
    private readonly IPersistedDataResetRepository _resetRepository;
    private readonly ILogger<ResetPersistedDataCommandHandler> _logger;

    /// <summary>Creates a new <see cref="ResetPersistedDataCommandHandler"/>.</summary>
    public ResetPersistedDataCommandHandler(
        IPersistedDataResetRepository resetRepository,
        ILogger<ResetPersistedDataCommandHandler> logger)
    {
        _resetRepository = resetRepository ?? throw new ArgumentNullException(nameof(resetRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task HandleAsync(ResetPersistedDataCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        _logger.LogWarning("Resetting all persisted application data.");
        await _resetRepository.ResetAsync(ct).ConfigureAwait(false);
        _logger.LogInformation("Persisted application data reset completed.");
    }
}

