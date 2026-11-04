using System.Net;
using System.Net.Http.Json;
using AspPlayground.DTOs;
using AspPlayground.Tests.Helpers;

namespace AspPlayground.Tests.IntegrationTests.Vehicles;

// PUT /api/Vehicles/{id}
public class UpdateVehicleEndpointTests
{
    [Fact]
    public async Task Update_VehicleExists_Returns200WithTheNewValues()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        int id = factory.AddVehicle(TestData.CreateVehicle(brand: "Chery"));
        HttpClient client = factory.CreateClient();
        VehicleRequest request = TestData.CreateRequest();
        request.Brand = "JAECOO";
        request.Mileage = 500;

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/Vehicles/{id}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        VehicleResponse? vehicle = await response.Content.ReadFromJsonAsync<VehicleResponse>();
        Assert.NotNull(vehicle);
        Assert.Equal(id, vehicle.Id);
        Assert.Equal("JAECOO", vehicle.Brand);
        Assert.Equal(500, vehicle.Mileage);
    }

    [Fact]
    public async Task Update_VehicleExists_ChangeIsSaved()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        int id = factory.AddVehicle(TestData.CreateVehicle(brand: "Chery"));
        HttpClient client = factory.CreateClient();
        VehicleRequest request = TestData.CreateRequest();
        request.Brand = "JAECOO";

        await client.PutAsJsonAsync($"/api/Vehicles/{id}", request);

        VehicleResponse? vehicle = await client.GetFromJsonAsync<VehicleResponse>($"/api/Vehicles/{id}");
        Assert.NotNull(vehicle);
        Assert.Equal("JAECOO", vehicle.Brand);
    }

    [Fact]
    public async Task Update_VehicleDoesNotExist_Returns404()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PutAsJsonAsync("/api/Vehicles/999", TestData.CreateRequest());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_InvalidRequest_Returns400()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        int id = factory.AddVehicle(TestData.CreateVehicle());
        HttpClient client = factory.CreateClient();
        VehicleRequest request = TestData.CreateRequest();
        request.Price = -1;

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/Vehicles/{id}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
