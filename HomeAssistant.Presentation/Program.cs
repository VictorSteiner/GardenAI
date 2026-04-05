using HomeAssistant.Presentation.Configuration;

var builder = WebApplication.CreateBuilder(args);

// ── Register Services ──────────────────────────────────────────────────────
builder.Services.AddOpenApi();
builder.Services.AddDatabaseServices(builder.Configuration);
builder.Services.AddCqrsServices();
builder.Services.AddExternalClients(builder.Configuration);
builder.Services.AddGardenAdvisorServices(builder.Configuration);
builder.Services.AddSensorProvider(builder.Environment, builder.Configuration);

var app = builder.Build();

// ── Initialize and Configure Application ───────────────────────────────────
await app.ConfigureMiddlewareAsync();
app.ConfigurePipeline();
app.MapRoutes();

app.Run();

