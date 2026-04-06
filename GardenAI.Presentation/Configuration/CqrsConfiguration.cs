namespace GardenAI.Presentation.Configuration;

/// <summary>Extension methods for CQRS command and query handler configuration.</summary>
/// <remarks>
/// This class is a placeholder for future CQRS service registration (command handlers, query handlers, validators, mappers).
/// Currently, handlers are registered on-demand; this method can be extended to support bulk registration patterns.
/// </remarks>
internal static class CqrsConfiguration
{
    /// <summary>Registers CQRS dispatcher, command handlers, query handlers, validators, and mappers.</summary>
    /// <remarks>Placeholder for future CQRS handler registration. Extend this method as needed to register:</remarks>
    /// <remarks>- ICommandHandler<TCommand> implementations</remarks>
    /// <remarks>- IQueryHandler<TQuery, TResult> implementations</remarks>
    /// <remarks>- Validators and mappings</remarks>
    internal static IServiceCollection AddCqrsServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Note: CQRS handlers are currently registered inline in Program.cs or via explicit calls.
        // Future enhancement: implement assembly scanning to auto-register all ICommandHandler<> and IQueryHandler<> implementations.
        return services;
    }
}

