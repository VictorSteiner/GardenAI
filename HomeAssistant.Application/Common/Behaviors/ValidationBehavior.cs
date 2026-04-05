using FluentValidation;
using FluentValidation.Results;
using HomeAssistant.Domain.Common.Abstractions;
using HomeAssistant.Domain.Common.Markers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Application.Common.Behaviors;

/// <summary>Validator helper for CQRS commands, implementing fail-fast validation pattern.</summary>
public sealed class CommandValidationHelper
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandValidationHelper> _logger;

    /// <summary>Creates a new validation helper instance.</summary>
    public CommandValidationHelper(IServiceProvider serviceProvider, ILogger<CommandValidationHelper> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Validates a command before dispatching, returning validation result.</summary>
    /// <remarks>
    /// Usage: Call this in endpoint handlers before dispatching command to fail-fast.
    /// Example: var validation = await _validator.ValidateCommandAsync(cmd, ct);
    ///          if (!validation.IsValid) return Results.BadRequest(validation.Errors);
    /// </remarks>
    public async Task<ValidationResult> ValidateCommandAsync<TCommand>(TCommand command, CancellationToken ct = default)
        where TCommand : ICommand
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(command.GetType());
        var validator = _serviceProvider.GetService(validatorType) as IValidator;

        if (validator is null)
        {
            _logger.LogDebug("No validator registered for command type {CommandType}.", command.GetType().Name);
            return new ValidationResult(); // No validator = valid
        }

        var validationContext = new ValidationContext<object>(command);
        var result = await validator.ValidateAsync(validationContext, ct).ConfigureAwait(false);

        if (!result.IsValid)
        {
            _logger.LogWarning(
                "Command validation failed for {CommandType}. Errors: {ErrorCount}",
                command.GetType().Name,
                result.Errors.Count);
        }

        return result;
    }
}



