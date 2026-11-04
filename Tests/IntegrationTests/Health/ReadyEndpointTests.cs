using System.Net;
using AspPlayground.Tests.Helpers;

namespace AspPlayground.Tests.IntegrationTests.Health;

// GET /health/ready  ("can the API reach its database?")
public class ReadyEndpointTests
{
    [Fact]
    public async Task Get_DatabaseWorks_Returns200Healthy()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
