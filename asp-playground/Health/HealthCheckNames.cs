namespace AspPlayground.Health;

/// <summary>Names shared between the health check registration, the endpoints and the HealthController.</summary>
public static class HealthCheckNames
{
    /// <summary>Name of the EF Core database connectivity check.</summary>
    public const string Database = "database";

    /// <summary>Tag put on every check that must pass before the API can serve traffic (readiness).</summary>
    public const string ReadyTag = "ready";
}
