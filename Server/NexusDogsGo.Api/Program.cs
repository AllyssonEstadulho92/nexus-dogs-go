using NexusDogsGo.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<DogCatalogService>();
builder.Services.AddSingleton<SpawnService>();

var app = builder.Build();
app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "nexus-dogs-go-api", utc = DateTimeOffset.UtcNow }));

app.MapGet("/api/v1/dogs", (DogCatalogService catalog) => Results.Ok(catalog.All));

app.MapGet("/api/v1/spawns", (double lat, double lon, int? count, DogCatalogService catalog, SpawnService spawns) =>
{
    if (lat is < -90 or > 90 || lon is < -180 or > 180)
        return Results.BadRequest(new { error = "invalid_coordinate" });

    return Results.Ok(spawns.Generate(lat, lon, Math.Clamp(count ?? 8, 1, 25), catalog.All));
});

app.Run();
