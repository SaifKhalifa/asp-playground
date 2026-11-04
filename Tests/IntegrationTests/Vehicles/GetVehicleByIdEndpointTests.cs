using System.Net;
using System.Net.Http.Json;
using AspPlayground.DTOs;
using AspPlayground.Tests.Helpers;

namespace AspPlayground.Tests.IntegrationTests.Vehicles;

// GET /api/Vehicles/{id}
public class GetVehicleByIdEndpointTests
{
    [Fact]
    public async Task GetById_VehicleExists_Returns200WithTheVehicle()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        int id = factory.AddVehicle(TestData.CreateVehicle(brand: "OMODA", model: "OMODA 5"));
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync($"/api/Vehicles/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        VehicleResponse? vehicle = await response.Content.ReadFromJsonAsync<VehicleResponse>();
        Assert.NotNull(vehicle);
        Assert.Equal(id, vehicle.Id);
        Assert.Equal("OMODA", vehicle.Brand);
        Assert.Equal("OMODA 5", vehicle.Model);
    }

    [Fact]
    public async Task GetById_VehicleDoesNotExist_Returns404()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/Vehicles/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_IdIsNotANumber_Returns400()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/Vehicles/abc");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
