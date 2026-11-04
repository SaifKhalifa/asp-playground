using AspPlayground.Controllers;
using AspPlayground.Health;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;

namespace AspPlayground.Tests.UnitTests.Controllers;

// GET api/health
public class HealthControllerTests
{
    private readonly Mock<HealthCheckService> _healthCheckServiceMock;
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        _healthCheckServiceMock = new Mock<HealthCheckService>();
        _controller = new HealthController(_healthCheckServiceMock.Object);
    }

    // Makes the fake health check service report the given status for the "database" check
    private void SetupDatabaseStatus(HealthStatus status)
    {
        Dictionary<string, HealthReportEntry> entries = new Dictionary<string, HealthReportEntry>();
        entries[HealthCheckNames.Database] = new HealthReportEntry(status, null, TimeSpan.Zero, null, null);

        HealthReport report = new HealthReport(entries, TimeSpan.Zero);

        _healthCheckServiceMock
            .Setup(service => service.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);
    }

    [Fact]
    public async Task Get_DatabaseHealthy_ReturnsOkWithHealthyStatus()
    {
        // Arrange
        SetupDatabaseStatus(HealthStatus.Healthy);

        // Act
        ActionResult<HealthController.HealthResponse> result = await _controller.Get(CancellationToken.None);

        // Assert
        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        HealthController.HealthResponse body = Assert.IsType<HealthController.HealthResponse>(ok.Value);
        Assert.Equal("Healthy", body.ApiStatus);
        Assert.Equal("Healthy", body.DbStatus);
    }

    [Fact]
    public async Task Get_DatabaseUnhealthy_Returns503()
    {
        // Arrange
        SetupDatabaseStatus(HealthStatus.Unhealthy);

        // Act
        ActionResult<HealthController.HealthResponse> result = await _controller.Get(CancellationToken.None);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(503, objectResult.StatusCode);
        HealthController.HealthResponse body = Assert.IsType<HealthController.HealthResponse>(objectResult.Value);
        Assert.Equal("Unhealthy", body.DbStatus);
    }
}
