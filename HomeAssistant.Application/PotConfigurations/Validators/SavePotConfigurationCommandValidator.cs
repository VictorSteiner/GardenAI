using FluentValidation;
using HomeAssistant.Application.PotConfigurations.Commands;

namespace HomeAssistant.Application.PotConfigurations.Validators;

/// <summary>Validates SavePotConfigurationCommand to fail-fast before dispatch.</summary>
public sealed class SavePotConfigurationCommandValidator : AbstractValidator<SavePotConfigurationCommand>
{
    /// <summary>Initializes the validator with rules.</summary>
    public SavePotConfigurationCommandValidator()
    {
        RuleFor(x => x.PotId)
            .NotEmpty()
            .WithMessage("Pot ID is required");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Configuration request is required");

        RuleFor(x => x.Request.RoomAreaId)
            .NotEmpty()
            .WithMessage("Room area ID is required")
            .MaximumLength(100)
            .WithMessage("Room area ID must not exceed 100 characters");

        RuleFor(x => x.Request.RoomName)
            .NotEmpty()
            .WithMessage("Room name is required")
            .MaximumLength(100)
            .WithMessage("Room name must not exceed 100 characters");

        RuleFor(x => x.Request.Seeds)
            .NotEmpty()
            .WithMessage("At least one seed assignment is required");

        RuleForEach(x => x.Request.Seeds)
            .ChildRules(seed =>
            {
                seed.RuleFor(s => s.PlantName)
                    .NotEmpty()
                    .WithMessage("Plant name is required")
                    .MaximumLength(100)
                    .WithMessage("Plant name must not exceed 100 characters");

                seed.RuleFor(s => s.SeedName)
                    .NotEmpty()
                    .WithMessage("Seed name is required")
                    .MaximumLength(100)
                    .WithMessage("Seed name must not exceed 100 characters");

                seed.RuleFor(s => s.Status)
                    .NotEmpty()
                    .WithMessage("Status is required")
                    .Must(s => new[] { "growing", "mature", "harvested", "removed" }.Contains(s, StringComparer.OrdinalIgnoreCase))
                    .WithMessage("Status must be one of: growing, mature, harvested, removed");
            });
    }
}

