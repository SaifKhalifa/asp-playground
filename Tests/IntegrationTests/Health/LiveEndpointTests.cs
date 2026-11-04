using System.Net;
using AspPlayground.Tests.Helpers;

namespace AspPlayground.Tests.IntegrationTests.Health;

// GET /health/live  ("is the API running?")
public class LiveEndpointTests
{
    [Fact]
    public async Task Get_Returns200Healthy()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
