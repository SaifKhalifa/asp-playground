using AspPlayground.Data;
using AspPlayground.Health;
using AspPlayground.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IVehicleService, VehicleService>();

// Real database check (EF Core CanConnectAsync). The "ready" tag decides which probe includes it.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(name: HealthCheckNames.Database, tags: new[] { HealthCheckNames.ReadyTag });

var app = builder.Build();

// Apply pending migrations on startup so a fresh clone runs with no manual DB step.
// Skipped in the "Testing" environment: integration tests swap in SQLite and create the schema themselves.
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
    }
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOpenApi();
app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "Vehicle API v1"));

app.MapControllers();

// Liveness: the process is up and answering (no dependency checks).
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

// Readiness: dependencies tagged "ready" (the database) are reachable. 503 when they are not.
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains(HealthCheckNames.ReadyTag)
});

app.Run();

// Makes the entry point visible to WebApplicationFactory<Program> in the test project.
public partial class Program { }
