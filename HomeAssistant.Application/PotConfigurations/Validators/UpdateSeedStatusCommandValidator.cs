using FluentValidation;
using HomeAssistant.Application.PotConfigurations.Commands;

namespace HomeAssistant.Application.PotConfigurations.Validators;

/// <summary>Validates UpdateSeedStatusCommand to fail-fast before dispatch.</summary>
public sealed class UpdateSeedStatusCommandValidator : AbstractValidator<UpdateSeedStatusCommand>
{
    private static readonly string[] ValidStatuses = ["growing", "mature", "harvested", "removed"];

    /// <summary>Initializes the validator with rules.</summary>
    public UpdateSeedStatusCommandValidator()
    {
        RuleFor(x => x.PotId)
            .NotEmpty()
            .WithMessage("Pot ID is required.");

        RuleFor(x => x.SeedId)
            .NotEmpty()
            .WithMessage("Seed ID is required.");

        RuleFor(x => x.NewStatus)
            .NotEmpty()
            .WithMessage("New status is required.")
            .Must(s => ValidStatuses.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Status must be one of: growing, mature, harvested, removed.");
    }
}

