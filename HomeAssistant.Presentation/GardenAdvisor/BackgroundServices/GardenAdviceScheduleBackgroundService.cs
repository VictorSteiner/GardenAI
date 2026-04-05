using HomeAssistant.Presentation.GardenAdvisor.Abstractions;
using HomeAssistant.Presentation.GardenAdvisor.Configuration;
using Microsoft.Extensions.Options;

namespace HomeAssistant.Presentation.GardenAdvisor.BackgroundServices;

/// <summary>Runs scheduled advisory generation every 3 hours between 08:00 and 21:00 local time.</summary>
public sealed class GardenAdviceScheduleBackgroundService : BackgroundService
{
    private static readonly HashSet<int> ScheduledHours = [8, 11, 14, 17, 20];

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly GardenAdvisorOptions _options;
    private readonly ILogger<GardenAdviceScheduleBackgroundService> _logger;
    private string? _lastSlotKey;

    /// <summary>Creates the scheduler service.</summary>
    public GardenAdviceScheduleBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<GardenAdvisorOptions> options,
        ILogger<GardenAdviceScheduleBackgroundService> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.EnableScheduledAdvice)
        {
            _logger.LogInformation("Scheduled garden advice generation is disabled by configuration.");
            return;
        }

        _logger.LogInformation("Garden advice scheduler started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var localNow = DateTimeOffset.Now;
                if (ScheduledHours.Contains(localNow.Hour))
                {
                    var slot = $"{localNow:yyyy-MM-dd-HH}";
                    if (!string.Equals(slot, _lastSlotKey, StringComparison.Ordinal))
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var advisorService = scope.ServiceProvider.GetRequiredService<IGardenAdvisorService>();

                        await advisorService.GenerateAdviceAsync(publishToMqtt: true, stoppingToken);
                        _lastSlotKey = slot;
                        _logger.LogInformation("Scheduled garden advice generated for slot {Slot}.", slot);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown.
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Garden advice scheduler iteration failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
