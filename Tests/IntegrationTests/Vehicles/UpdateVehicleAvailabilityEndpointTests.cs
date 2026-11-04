using System.Net;
using System.Net.Http.Json;
using AspPlayground.DTOs;
using AspPlayground.Entities;
using AspPlayground.Tests.Helpers;

namespace AspPlayground.Tests.IntegrationTests.Vehicles;

// PUT /api/Vehicles/{id}/availability?newState=...
public class UpdateVehicleAvailabilityEndpointTests
{
    [Theory]
    [InlineData("Reserved", AvailabilityStatus.Reserved)]
    [InlineData("Sold", AvailabilityStatus.Sold)]
    public async Task UpdateAvailability_VehicleExists_Returns200WithTheNewStatus(string newState, AvailabilityStatus expected)
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        int id = factory.AddVehicle(TestData.CreateVehicle());
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PutAsync($"/api/Vehicles/{id}/availability?newState={newState}", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        VehicleResponse? vehicle = await response.Content.ReadFromJsonAsync<VehicleResponse>();
        Assert.NotNull(vehicle);
        Assert.Equal(expected, vehicle.AvailabilityStatus);
    }

    [Fact]
    public async Task UpdateAvailability_VehicleExists_ChangeIsSaved()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        int id = factory.AddVehicle(TestData.CreateVehicle());
        HttpClient client = factory.CreateClient();

        await client.PutAsync($"/api/Vehicles/{id}/availability?newState=Sold", null);

        VehicleResponse? vehicle = await client.GetFromJsonAsync<VehicleResponse>($"/api/Vehicles/{id}");
        Assert.NotNull(vehicle);
        Assert.Equal(AvailabilityStatus.Sold, vehicle.AvailabilityStatus);
    }

    [Fact]
    public async Task UpdateAvailability_VehicleDoesNotExist_Returns404()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PutAsync("/api/Vehicles/999/availability?newState=Sold", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAvailability_UnknownStatus_Returns400()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        int id = factory.AddVehicle(TestData.CreateVehicle());
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PutAsync($"/api/Vehicles/{id}/availability?newState=Bogus", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
