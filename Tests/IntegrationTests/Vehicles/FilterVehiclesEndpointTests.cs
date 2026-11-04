using System.Net;
using System.Net.Http.Json;
using AspPlayground.DTOs;
using AspPlayground.Tests.Helpers;

namespace AspPlayground.Tests.IntegrationTests.Vehicles;

// GET /api/Vehicles/filter
public class FilterVehiclesEndpointTests
{
    [Fact]
    public async Task Filter_ByBrand_ReturnsOnlyThatBrand()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        factory.AddVehicle(TestData.CreateVehicle(brand: "Chery"));
        factory.AddVehicle(TestData.CreateVehicle(brand: "Chery"));
        factory.AddVehicle(TestData.CreateVehicle(brand: "OMODA"));
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/Vehicles/filter?brand=Chery");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PagedResponse<VehicleResponse>? page = await response.Content.ReadFromJsonAsync<PagedResponse<VehicleResponse>>();
        Assert.NotNull(page);
        Assert.Equal(2, page.Items.Count);
        Assert.All(page.Items, vehicle => Assert.Equal("Chery", vehicle.Brand));
    }

    [Fact]
    public async Task Filter_ByBrandAndModel_ReturnsOnlyThatModel()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        factory.AddVehicle(TestData.CreateVehicle(brand: "Chery", model: "Tiggo 8 Pro"));
        factory.AddVehicle(TestData.CreateVehicle(brand: "Chery", model: "Tiggo 7 Pro"));
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/Vehicles/filter?brand=Chery&model=Tiggo 7 Pro");

        PagedResponse<VehicleResponse>? page = await response.Content.ReadFromJsonAsync<PagedResponse<VehicleResponse>>();
        Assert.NotNull(page);
        VehicleResponse vehicle = Assert.Single(page.Items);
        Assert.Equal("Tiggo 7 Pro", vehicle.Model);
    }

    [Fact]
    public async Task Filter_NothingMatches_Returns200WithEmptyList()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        factory.AddVehicle(TestData.CreateVehicle(brand: "Chery"));
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/Vehicles/filter?brand=Tesla");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PagedResponse<VehicleResponse>? page = await response.Content.ReadFromJsonAsync<PagedResponse<VehicleResponse>>();
        Assert.NotNull(page);
        Assert.Empty(page.Items);
    }

    [Fact]
    public async Task Filter_InvalidPaging_Returns400()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/Vehicles/filter?brand=Chery&page=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
