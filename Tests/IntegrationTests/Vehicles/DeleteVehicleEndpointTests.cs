using System.Net;
using AspPlayground.Tests.Helpers;

namespace AspPlayground.Tests.IntegrationTests.Vehicles;

// DELETE /api/Vehicles/{id}
public class DeleteVehicleEndpointTests
{
    [Fact]
    public async Task Delete_VehicleExists_Returns200()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        int id = factory.AddVehicle(TestData.CreateVehicle());
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.DeleteAsync($"/api/Vehicles/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Delete_VehicleExists_VehicleIsGoneAfterwards()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        int id = factory.AddVehicle(TestData.CreateVehicle());
        HttpClient client = factory.CreateClient();

        await client.DeleteAsync($"/api/Vehicles/{id}");

        HttpResponseMessage getResponse = await client.GetAsync($"/api/Vehicles/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_VehicleDoesNotExist_Returns404()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.DeleteAsync("/api/Vehicles/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_IdIsNotANumber_Returns400()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.DeleteAsync("/api/Vehicles/abc");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
