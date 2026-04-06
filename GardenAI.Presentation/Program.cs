using GardenAI.Application.Common.Sync.Abstractions;
using GardenAI.Application.Common.Sync.Configuration;
using GardenAI.Application.Weather.Abstractions;
using GardenAI.Application.Weather.Configuration;
using GardenAI.Domain.Assistant.Abstractions;
using GardenAI.Domain.Common.Abstractions;
using GardenAI.Infrastructure.HomeAssistant.Common.Contracts;
using GardenAI.Infrastructure.HomeAssistant.Events.Handlers;
using GardenAI.Infrastructure.HomeAssistant.Rest.Clients;
using GardenAI.Infrastructure.HomeAssistant.Sync.Services;
using GardenAI.Infrastructure.HomeAssistant.WebSockets.Services;
using GardenAI.Infrastructure.Messaging;
using GardenAI.Infrastructure.Persistence.Assistant.Repositories;
using GardenAI.Infrastructure.Persistence.Database;
using GardenAI.Integrations.OpenMeteo.Forecast.Clients;
using GardenAI.Presentation.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register Services
builder.Services.AddOpenApi();

// Persistence
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IChatSessionRepository, ChatSessionRepository>();

// External Adapters - MQTT
builder.Services.AddMqttClient(builder.Configuration);

// External Adapters - OpenMeteo
var openMeteoOptions = new OpenMeteoClientOptions();
builder.Configuration.GetSection("OpenMeteo").Bind(openMeteoOptions);
builder.Services.AddSingleton(openMeteoOptions);
builder.Services.AddHttpClient<IOpenMeteoForecastClient, OpenMeteoForecastClient>((_, client) =>
{
    client.BaseAddress = new Uri(openMeteoOptions.BaseUrl, UriKind.Absolute);
    client.Timeout = TimeSpan.FromSeconds(20);
});

// Home Assistant sync options and services
var homeAssistantOptions = new HomeAssistantOptions();
builder.Configuration.GetSection(HomeAssistantOptions.SectionName).Bind(homeAssistantOptions);
builder.Services.AddSingleton(homeAssistantOptions);

builder.Services.AddSingleton<IHomeAssistantWebSocketClient, HomeAssistantWebSocketClient>();
builder.Services.AddHttpClient<IHomeAssistantRestClient, HomeAssistantRestClient>((sp, client) =>
{
    var options = sp.GetRequiredService<HomeAssistantOptions>();
    client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddSingleton<ISyncOrchestrator, SyncOrchestrator>();

builder.Services.AddKeyedScoped<IRegistryEventHandler, AreaRegistryEventHandler>(HaEventTypes.AreaRegistryUpdated);
builder.Services.AddKeyedScoped<IRegistryEventHandler, DeviceRegistryEventHandler>(HaEventTypes.DeviceRegistryUpdated);
builder.Services.AddKeyedScoped<IRegistryEventHandler, EntityRegistryEventHandler>(HaEventTypes.EntityRegistryUpdated);

builder.Services.AddHostedService<HomeAssistantSyncBackgroundService>();

builder.Services.AddCqrsServices(builder.Configuration);
builder.Services.AddExternalClients(builder.Configuration);

var app = builder.Build();

// Initialize and Configure Application
await app.ConfigureMiddlewareAsync();
app.ConfigurePipeline();
app.MapRoutes();

app.Run();

