using AspPlayground.Data;
using AspPlayground.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspPlayground.Tests.Helpers;

/// <summary>
/// Starts the real API inside the test, but with an empty in-memory SQLite database instead of SQL Server.
/// Create a new one at the start of every integration test, so each test begins with an empty database.
/// </summary>
/// This entire file was built by Claude Code, based on the official Microsoft docs for testing with SQLite in memory.
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // An in-memory SQLite database disappears when its connection closes, so we keep one connection open.
    private readonly SqliteConnection _connection = new SqliteConnection("DataSource=:memory:");

    public CustomWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // In this environment Program.cs does not call Database.Migrate()
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // 1. Remove the SQL Server setup that Program.cs registered for AppDbContext
            List<ServiceDescriptor> sqlServerSetup = services
                .Where(d => d.ServiceType.FullName != null && d.ServiceType.FullName.Contains("AppDbContext"))
                .ToList();

            foreach (ServiceDescriptor descriptor in sqlServerSetup)
            {
                services.Remove(descriptor);
            }

            // 2. Use SQLite in memory instead
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        IHost host = base.CreateHost(builder);

        // Create the empty tables
        using IServiceScope scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();

        return host;
    }

    /// <summary>Saves a vehicle straight into the test database and returns its new id.</summary>
    public int AddVehicle(Vehicle vehicle)
    {
        using IServiceScope scope = Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Vehicles.Add(vehicle);
        db.SaveChanges();

        return vehicle.Id;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
