using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HomeAssistant.Composition.DependencyInjection;

/// <summary>
/// Registers the root composition facade for the HomeAssistant host.
/// </summary>
public static class HomeAssistantCompositionServiceCollectionExtensions
{
	/// <summary>
	/// Registers HomeAssistant composition services by delegating to infrastructure composition.
	/// </summary>
	/// <param name="services">Service collection.</param>
	/// <param name="environment">Current host environment.</param>
	/// <param name="configuration">Application configuration root.</param>
	/// <returns>The same service collection for chaining.</returns>
	public static IServiceCollection AddHomeAssistantComposition(
		this IServiceCollection services,
		IHostEnvironment environment,
		IConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(environment);
		ArgumentNullException.ThrowIfNull(configuration);

		return services.AddInfrastructureComposition(environment, configuration);
	}
}

