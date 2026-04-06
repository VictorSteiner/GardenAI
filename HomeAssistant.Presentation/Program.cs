using HomeAssistant.Application.Messaging.Abstractions;
using HomeAssistant.Application.Messaging.Configuration;
using HomeAssistant.Application.Weather.Abstractions;
using HomeAssistant.Application.Weather.Configuration;
using HomeAssistant.Domain.Assistant.Abstractions;
using HomeAssistant.Domain.Common.Abstractions;
using HomeAssistant.Infrastructure.Messaging.Messaging.Services;
using HomeAssistant.Infrastructure.Persistence.Assistant.Repositories;
using HomeAssistant.Infrastructure.Persistence.Database;
using HomeAssistant.Integrations.OpenMeteo.Forecast.Clients;
using Microsoft.EntityFrameworkCore;
using HomeAssistant.Presentation.Configuration;

var builder = WebApplication.CreateBuilder(args);

// ── Register Services ──────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// Persistence
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IChatSessionRepository, ChatSessionRepository>();

// External Adapters - MQTT
var mqttOptions = new MqttClientOptions();
builder.Configuration.GetSection("Mqtt").Bind(mqttOptions);
builder.Services.AddSingleton(mqttOptions);
builder.Services.AddSingleton<IMqttClient, MqttClientService>();

// External Adapters - OpenMeteo
var openMeteoOptions = new OpenMeteoClientOptions();
builder.Configuration.GetSection("OpenMeteo").Bind(openMeteoOptions);
builder.Services.AddSingleton(openMeteoOptions);
builder.Services.AddHttpClient<IOpenMeteoForecastClient, OpenMeteoForecastClient>((_, client) =>
{
    client.BaseAddress = new Uri(openMeteoOptions.BaseUrl, UriKind.Absolute);
    client.Timeout = TimeSpan.FromSeconds(20);
});

builder.Services.AddCqrsServices(builder.Configuration);
builder.Services.AddExternalClients(builder.Configuration);

var app = builder.Build();

// ── Initialize and Configure Application ───────────────────────────────────
await app.ConfigureMiddlewareAsync();
app.ConfigurePipeline();
app.MapRoutes();

app.Run();

