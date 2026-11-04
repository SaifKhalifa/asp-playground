using System.Net;
using System.Net.Http.Json;
using AspPlayground.DTOs;
using AspPlayground.Tests.Helpers;

namespace AspPlayground.Tests.IntegrationTests.Vehicles;

// POST /api/Vehicles
public class CreateVehicleEndpointTests
{
    [Fact]
    public async Task Create_ValidRequest_Returns201WithTheNewVehicle()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/Vehicles", TestData.CreateRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        VehicleResponse? vehicle = await response.Content.ReadFromJsonAsync<VehicleResponse>();
        Assert.NotNull(vehicle);
        Assert.True(vehicle.Id > 0);
        Assert.Equal("Chery", vehicle.Brand);
    }

    [Fact]
    public async Task Create_ValidRequest_LocationHeaderPointsToTheNewVehicle()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/Vehicles", TestData.CreateRequest());
        Uri? location = createResponse.Headers.Location;

        Assert.NotNull(location);
        HttpResponseMessage getResponse = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task Create_EmptyBrand_Returns400()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        HttpClient client = factory.CreateClient();
        VehicleRequest request = TestData.CreateRequest();
        request.Brand = "";

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/Vehicles", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_NegativePrice_Returns400()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        HttpClient client = factory.CreateClient();
        VehicleRequest request = TestData.CreateRequest();
        request.Price = -1;

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/Vehicles", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidYear_Returns400()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        HttpClient client = factory.CreateClient();
        VehicleRequest request = TestData.CreateRequest();
        request.ManufacturingYear = 20211;

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/Vehicles", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidRequest_DoesNotSaveAnything()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        HttpClient client = factory.CreateClient();
        VehicleRequest request = TestData.CreateRequest();
        request.Brand = "";

        await client.PostAsJsonAsync("/api/Vehicles", request);

        HttpResponseMessage response = await client.GetAsync("/api/Vehicles");
        PagedResponse<VehicleResponse>? page = await response.Content.ReadFromJsonAsync<PagedResponse<VehicleResponse>>();
        Assert.NotNull(page);
        Assert.Empty(page.Items);
    }
}
