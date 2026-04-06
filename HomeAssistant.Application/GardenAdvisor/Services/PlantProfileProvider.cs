using HomeAssistant.Application.GardenAdvisor.Abstractions;
using HomeAssistant.Application.GardenAdvisor.Contracts;

namespace HomeAssistant.Application.GardenAdvisor.Services;

/// <summary>Provides static seed and plant assignments for each monitored pot.</summary>
public sealed class PlantProfileProvider : IPlantProfileProvider
{
    private static readonly IReadOnlyDictionary<Guid, PlantProfile> Profiles =
        new Dictionary<Guid, PlantProfile>
        {
            [Guid.Parse("00000000-0000-0000-0000-000000000001")] = new(Guid.Parse("00000000-0000-0000-0000-000000000001"), 1, "Pot 1", "Tomato", "Moneymaker", 45, 70, 18, 26),
            [Guid.Parse("00000000-0000-0000-0000-000000000002")] = new(Guid.Parse("00000000-0000-0000-0000-000000000002"), 2, "Pot 2", "Cucumber", "Marketmore", 50, 75, 20, 28),
            [Guid.Parse("00000000-0000-0000-0000-000000000003")] = new(Guid.Parse("00000000-0000-0000-0000-000000000003"), 3, "Pot 3", "Basil", "Genovese", 40, 65, 18, 28),
            [Guid.Parse("00000000-0000-0000-0000-000000000004")] = new(Guid.Parse("00000000-0000-0000-0000-000000000004"), 4, "Pot 4", "Carrot", "Nantes", 35, 60, 12, 24),
            [Guid.Parse("00000000-0000-0000-0000-000000000005")] = new(Guid.Parse("00000000-0000-0000-0000-000000000005"), 5, "Pot 5", "Lettuce", "Little Gem", 45, 70, 10, 22),
            [Guid.Parse("00000000-0000-0000-0000-000000000006")] = new(Guid.Parse("00000000-0000-0000-0000-000000000006"), 6, "Pot 6", "Pepper", "California Wonder", 40, 65, 18, 30),
        };

    /// <inheritdoc />
    public IReadOnlyDictionary<Guid, PlantProfile> GetProfiles() => Profiles;
}

