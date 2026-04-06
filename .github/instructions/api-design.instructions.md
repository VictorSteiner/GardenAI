---
applyTo: "GardenAI.Presentation/Program.cs,GardenAI.Presentation/Endpoints/**/*.cs,GardenAI.Presentation/**/*.http"
---

# API Design Instructions

## Minimal APIs (No MVC Controllers)

All HTTP endpoints use **ASP.NET Core Minimal APIs** defined in `Program.cs` or extension methods.

```csharp
// ? Never use controller classes
public class PlantPotsController : ControllerBase { }

// ? Use Minimal API endpoints
app.MapGet("/pots", ...)
   .WithName("GetPlantPots")
   .Produces<IReadOnlyList<PlantPotResponse>>();
```

---

## Typed Results Pattern

Every endpoint returns **typed results** with explicit type annotations for OpenAPI generation.

```csharp
// ? Correct
app.MapGet("/pots/{id:guid}", async (Guid id, IQueryHandler<GetPotByIdQuery, PlantPotDto?> handler, CancellationToken ct) =>
{
    var result = await handler.HandleAsync(new GetPotByIdQuery(id), ct);
    return result is null
        ? Results.NotFound()
        : Results.Ok(result);
})
.Produces<PlantPotResponse>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status404NotFound)
.WithName("GetPlantPotById")
.WithOpenApi();

// ? Wrong
app.MapGet("/pots/{id:guid}", (Guid id) => new { id, label = "test" });

// ? Wrong
app.MapGet("/pots/{id:guid}", (Guid id) => Results.Ok(new { id }));
```

---

## Result Types

```csharp
// Success responses
Results.Ok<PlantPotResponse>(pot)                    // 200
Results.Created($"/pots/{id}", pot)                  // 201
Results.Accepted()                                   // 202 (fire-and-forget command)
Results.NoContent()                                  // 204

// Client errors
Results.BadRequest()                                 // 400
Results.NotFound()                                   // 404
Results.Conflict()                                   // 409

// Server errors
Results.Problem(detail: "Database unavailable")      // 500
Results.InternalServerError()                        // 500
```

---

## Endpoint Structure

```csharp
app.MapGet("/pots/{id:guid}", HandleGetPotById)
   .Produces<PlantPotResponse>(StatusCodes.Status200OK)
   .ProducesProblem(StatusCodes.Status404NotFound)
   .WithName("GetPlantPotById")
   .WithOpenApi()
   .WithSummary("Retrieve a single plant pot")
   .WithDescription("Returns pot details including current sensor readings")
   .WithTags("PlantPots");

static async Task<IResult> HandleGetPotById(
    Guid id,
    IQueryHandler<GetPotByIdQuery, PlantPotDto?> handler,
    ILogger<Program> logger,
    CancellationToken ct)
{
    logger.LogInformation("Fetching pot {PotId}", id);
    var result = await handler.HandleAsync(new GetPotByIdQuery(id), ct);
    
    return result is null
        ? Results.NotFound()
        : Results.Ok(result);
}
```

### Metadata Annotations

- `.Produces<T>()` – Success response type and status code
- `.ProducesProblem()` – Error response types
- `.WithName()` – Operation ID for OpenAPI
- `.WithOpenApi()` – Include in OpenAPI schema
- `.WithSummary()` – Short description
- `.WithDescription()` – Detailed description
- `.WithTags()` – Grouping in API docs

---

## Route Builders and Endpoint Folders

Keep `Program.cs` clean by using domain route builders and per-endpoint folders:

```csharp
// GardenAI.Presentation/PlantPots/RouteBuilders/PlantPotRouteBuilder.cs
public static class PlantPotRouteBuilder
{
    public static IEndpointRouteBuilder MapPlantPotRoutes(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/pots")
            .WithTags("PlantPots")
            .WithOpenApi();

        GetAllPlantPotsEndpoint.Map(group);
        GetPlantPotByIdEndpoint.Map(group);
        CreatePlantPotEndpoint.Map(group);

        return endpoints;
    }
}

// GardenAI.Presentation/PlantPots/Endpoints/GetAllPlantPots/GetAllPlantPotsEndpoint.cs
internal static class GetAllPlantPotsEndpoint
{
    internal static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", async (
                IQueryHandler<GetAllPotsQuery, IReadOnlyList<PlantPotResponse>> handler,
                CancellationToken ct) =>
                Results.Ok(await handler.HandleAsync(new GetAllPotsQuery(), ct)))
            .WithName("GetAllPlantPots")
            .Produces<IReadOnlyList<PlantPotResponse>>();
}

// In Program.cs
app.MapPlantPotRoutes();
```

### Folder Pattern

For a feature with multiple endpoints, prefer:

```text
GardenAI.Presentation/<Domain>/
  RouteBuilders/
    <Domain>RouteBuilder.cs
  Endpoints/
    <EndpointName>/
      Contracts/
        <EndpointName>Request.cs
        <EndpointName>Response.cs
      <EndpointName>Endpoint.cs
  Contracts/
    <SharedContract>.cs   # only when reused by multiple endpoints
```

Rules:
- Do not keep multiple endpoints flattened into a single folder when the feature has several endpoints.
- Use one folder per endpoint.
- `<Domain>RouteBuilder` gathers endpoint mappings for that domain/feature.
- `Program.cs` should remain the composition root and delegate route mapping to route builders.
- Endpoint-local request/response contracts belong under the owning endpoint folder.
- Keep domain-level `Contracts/` files only for models reused across several endpoints.

---

## DTOs (Data Transfer Objects)

Define response DTOs as **records** (immutable, fast, concise):

```csharp
// Application/PlantPots/Responses/PlantPotResponse.cs
public sealed record PlantPotResponse(
    Guid Id,
    string Label,
    double SoilMoisture,
    double TemperatureC);

// Request DTOs for command input
public sealed record CreatePlantPotRequest(
    string Label,
    Guid SpeciesId);
```

---

## OpenAPI in Development Only

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();  // /openapi/v1.json
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/openapi/v1.json", "GardenAI API v1");
    });
}
```

---

## .http Testing File

Create `GardenAI.Presentation.http` for quick manual testing:

```http
@HostAddress = http://localhost:5064

### Get all plant pots
GET {{HostAddress}}/pots
Accept: application/json

###

### Get single pot
GET {{HostAddress}}/pots/550e8400-e29b-41d4-a716-446655440000
Accept: application/json

###

### Create pot
POST {{HostAddress}}/pots
Content-Type: application/json

{
  "label": "Tomato Plant",
  "speciesId": "550e8400-e29b-41d4-a716-446655440001"
}

###

### Get sensor readings
GET {{HostAddress}}/sensor-readings
Accept: application/json
```

---

## Error Handling

Return structured errors with `ProblemDetails`:

```csharp
app.MapPost("/pots", async (CreatePlantPotRequest req, CommandDispatcher dispatcher, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.Label))
        return Results.BadRequest(new { error = "Label is required" });
    
    var cmd = new CreatePlantPotCommand(req.Label, req.SpeciesId);
    await dispatcher.DispatchAsync(cmd, ct);
    return Results.Accepted();
})
.Produces(StatusCodes.Status202Accepted)
.ProducesProblem(StatusCodes.Status400BadRequest);
```

---

## Cancellation Tokens

Always accept and forward `CancellationToken`:

```csharp
app.MapGet("/data", async (CancellationToken ct) =>
{
    var data = await FetchDataAsync(ct);  // Pass through
    return Results.Ok(data);
});
```

---

## See Also

- **cqrs.instructions.md** – How endpoints dispatch commands and queries
- **dependency-injection.instructions.md** – DI setup for handlers
- **AGENTS.md** – Logging and API conventions for the repository

