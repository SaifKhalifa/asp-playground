using AspPlayground.Controllers;
using AspPlayground.DTOs;
using AspPlayground.Services;
using AspPlayground.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AspPlayground.Tests.UnitTests.Controllers;

// GET api/Vehicles/filter
public class FilterVehiclesTests
{
    private readonly Mock<IVehicleService> _serviceMock;
    private readonly VehiclesController _controller;

    public FilterVehiclesTests()
    {
        _serviceMock = new Mock<IVehicleService>();
        _controller = new VehiclesController(_serviceMock.Object);
    }

    [Fact]
    public async Task FilterByBrandAndModelAsync_ReturnsOkWithThePage()
    {
        // Arrange
        PaginationQuery query = new PaginationQuery();
        PagedResponse<VehicleResponse> page = TestData.CreatePage(TestData.CreateResponse());
        _serviceMock.Setup(service => service.FilterByBrandAndModelAsync("Chery", "Tiggo 8 Pro", query)).ReturnsAsync(page);

        // Act
        ActionResult<PagedResponse<VehicleResponse>> result = await _controller.FilterByBrandAndModelAsync("Chery", "Tiggo 8 Pro", query);

        // Assert
        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(page, ok.Value);
    }

    [Fact]
    public async Task FilterByBrandAndModelAsync_PassesTheBrandAndModelToTheService()
    {
        // Arrange
        PaginationQuery query = new PaginationQuery();
        _serviceMock.Setup(service => service.FilterByBrandAndModelAsync("OMODA", null, query)).ReturnsAsync(TestData.CreatePage());

        // Act
        await _controller.FilterByBrandAndModelAsync("OMODA", null, query);

        // Assert
        _serviceMock.Verify(service => service.FilterByBrandAndModelAsync("OMODA", null, query), Times.Once);
    }
}
