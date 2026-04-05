using FluentValidation;
using HomeAssistant.Application.Maintenance.Commands.ResetPersistedData;
using HomeAssistant.Application.PotConfigurations.Commands;
using HomeAssistant.Application.PotConfigurations.DTOs;
using HomeAssistant.Application.PotConfigurations.Queries;
using HomeAssistant.Application.PotConfigurations.Services;
using HomeAssistant.Application.PotConfigurations.Validators;
using HomeAssistant.Application.Dispatching;
using HomeAssistant.Domain.Common.Handlers;
using HomeAssistant.Domain.Common.Abstractions;
using HomeAssistant.Domain.PotConfigurations.Abstractions;
using HomeAssistant.Presentation.GardenAdvisor.Mappings;

namespace HomeAssistant.Presentation.Configuration;

/// <summary>Extension methods for CQRS command and query handler configuration.</summary>
internal static class CqrsConfiguration
{
    /// <summary>Registers CQRS dispatcher, command handlers, query handlers, validators, and mappers.</summary>
    internal static IServiceCollection AddCqrsServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // CQRS Dispatcher (singleton to maintain shared queue across requests)
        // Dispatcher runs registered IValidator<TCommand> automatically before handler execution
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();

        // Command Handlers
        services.AddScoped<ICommandHandler<SavePotConfigurationCommand>, SavePotConfigurationCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateSeedStatusCommand>, UpdateSeedStatusCommandHandler>();
        services.AddScoped<ICommandHandler<ResetPersistedDataCommand>, ResetPersistedDataCommandHandler>();

        // Query Handlers
        services.AddScoped<IQueryHandler<GetHarvestReadinessQuery, IReadOnlyList<HarvestReadinessDto>>, GetHarvestReadinessQueryHandler>();
        services.AddScoped<IQueryHandler<GetDashboardAggregationQuery, DashboardAggregationDto>, GetDashboardAggregationQueryHandler>();
        services.AddScoped<IQueryHandler<GetRoomSummaryQuery, RoomSummaryDto>, GetRoomSummaryQueryHandler>();

        // Application Services
        services.AddScoped<IHarvestReadinessCalculator, HarvestReadinessCalculator>();

        // FluentValidation: scan Application assembly for all IValidator<T> implementations
        services.AddValidatorsFromAssemblyContaining<SavePotConfigurationCommandValidator>(
            lifetime: ServiceLifetime.Scoped);

        // Mappers: Domain DTOs → Presentation contracts
        services.AddScoped<IGardenPlannerMapper, GardenPlannerMapper>();

        return services;
    }
}

