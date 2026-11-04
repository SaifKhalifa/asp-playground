using AspPlayground.Health;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspPlayground.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthChecks;

    public HealthController(HealthCheckService healthChecks)
    {
        _healthChecks = healthChecks;
    }

    /// <summary>Reports whether the API is running and whether its database is reachable.</summary>
    /// <remarks>Returns 503 when a registered health check is unhealthy, so load balancers and monitors can act on it.</remarks>
    [HttpGet]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<HealthResponse>> Get(CancellationToken cancellationToken)
    {
        HealthReport report = await _healthChecks.CheckHealthAsync(cancellationToken);

        string databaseStatus = report.Entries.TryGetValue(HealthCheckNames.Database, out HealthReportEntry entry)
            ? entry.Status.ToString()
            : "Unknown";

        // If this code runs, the API itself is up.
        HealthResponse response = new HealthResponse(nameof(HealthStatus.Healthy), databaseStatus, DateTime.UtcNow);

        if (report.Status == HealthStatus.Unhealthy)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }

        return Ok(response);
    }

    public record HealthResponse(string ApiStatus, string DbStatus, DateTime Timestamp);
}
