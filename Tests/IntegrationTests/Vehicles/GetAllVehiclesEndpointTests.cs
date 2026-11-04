using System.Net;
using System.Net.Http.Json;
using AspPlayground.DTOs;
using AspPlayground.Tests.Helpers;

namespace AspPlayground.Tests.IntegrationTests.Vehicles;

// GET /api/Vehicles
public class GetAllVehiclesEndpointTests
{
    [Fact]
    public async Task GetAll_NoVehicles_Returns200WithEmptyList()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/Vehicles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PagedResponse<VehicleResponse>? page = await response.Content.ReadFromJsonAsync<PagedResponse<VehicleResponse>>();
        Assert.NotNull(page);
        Assert.Empty(page.Items);
    }

    [Fact]
    public async Task GetAll_TwoVehicles_Returns200WithBoth()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        factory.AddVehicle(TestData.CreateVehicle(brand: "Chery"));
        factory.AddVehicle(TestData.CreateVehicle(brand: "OMODA"));
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/Vehicles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PagedResponse<VehicleResponse>? page = await response.Content.ReadFromJsonAsync<PagedResponse<VehicleResponse>>();
        Assert.NotNull(page);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal(2, page.TotalCount);
    }

    [Fact]
    public async Task GetAll_WithPageSize_ReturnsOnlyThatManyVehicles()
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        for (int i = 0; i < 5; i++)
        {
            factory.AddVehicle(TestData.CreateVehicle(year: 2000 + i));
        }
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/Vehicles?page=1&pageSize=2");

        PagedResponse<VehicleResponse>? page = await response.Content.ReadFromJsonAsync<PagedResponse<VehicleResponse>>();
        Assert.NotNull(page);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal(5, page.TotalCount);
        Assert.Equal(3, page.TotalPages);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=101")]
    public async Task GetAll_InvalidPaging_Returns400(string queryString)
    {
        using CustomWebApplicationFactory factory = new CustomWebApplicationFactory();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/Vehicles?" + queryString);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
