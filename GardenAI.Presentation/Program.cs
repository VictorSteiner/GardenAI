using GardenAI.Application.Weather.Abstractions;
using GardenAI.Application.Weather.Configuration;
using GardenAI.Domain.Assistant.Abstractions;
using GardenAI.Domain.Common.Abstractions;
using GardenAI.Infrastructure.Messaging;
using GardenAI.Infrastructure.Persistence.Assistant.Repositories;
using GardenAI.Infrastructure.Persistence.Database;
using GardenAI.Integrations.OpenMeteo.Forecast.Clients;
using Microsoft.EntityFrameworkCore;
using GardenAI.Presentation.Configuration;

var builder = WebApplication.CreateBuilder(args);

// ── Register Services ──────────────────────────────────────────────────────
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

builder.Services.AddCqrsServices(builder.Configuration);
builder.Services.AddExternalClients(builder.Configuration);

var app = builder.Build();

// ── Initialize and Configure Application ───────────────────────────────────
await app.ConfigureMiddlewareAsync();
app.ConfigurePipeline();
app.MapRoutes();

app.Run();

