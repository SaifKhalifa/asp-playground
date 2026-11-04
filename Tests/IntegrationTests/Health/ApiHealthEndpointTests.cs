using System.Net;
using System.Net.Http.Json;
using AspPlayground.Controllers;
using AspPlayground.Tests.Helpers;

namespace AspPlayground.Tests.IntegrationTests.Health;

// GET /api/health
public class ApiHealthEndpointTests
{
    [Fact]
    public async Task Get_DatabaseWorks_Returns200WithHealthyStatuses()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        HealthController.HealthResponse? body = await response.Content.ReadFromJsonAsync<HealthController.HealthResponse>();
        Assert.NotNull(body);
        Assert.Equal("Healthy", body.ApiStatus);
        Assert.Equal("Healthy", body.DbStatus);
    }
}
