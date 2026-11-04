using AspPlayground.Controllers;
using AspPlayground.DTOs;
using AspPlayground.Services;
using AspPlayground.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AspPlayground.Tests.UnitTests.Controllers;

// GET api/Vehicles
public class GetAllVehiclesTests
{
    private readonly Mock<IVehicleService> _serviceMock;
    private readonly VehiclesController _controller;

    public GetAllVehiclesTests()
    {
        _serviceMock = new Mock<IVehicleService>();
        _controller = new VehiclesController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOkWithThePage()
    {
        // Arrange
        PaginationQuery query = new PaginationQuery();
        PagedResponse<VehicleResponse> page = TestData.CreatePage(TestData.CreateResponse(1), TestData.CreateResponse(2));
        _serviceMock.Setup(service => service.GetAllAsync(query)).ReturnsAsync(page);

        // Act
        ActionResult<PagedResponse<VehicleResponse>> result = await _controller.GetAllAsync(query);

        // Assert
        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(page, ok.Value);
    }

    [Fact]
    public async Task GetAllAsync_CallsTheServiceOnce()
    {
        // Arrange
        PaginationQuery query = new PaginationQuery();
        _serviceMock.Setup(service => service.GetAllAsync(query)).ReturnsAsync(TestData.CreatePage());

        // Act
        await _controller.GetAllAsync(query);

        // Assert
        _serviceMock.Verify(service => service.GetAllAsync(query), Times.Once);
    }
}
