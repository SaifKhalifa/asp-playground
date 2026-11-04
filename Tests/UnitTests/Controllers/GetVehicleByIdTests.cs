using AspPlayground.Controllers;
using AspPlayground.DTOs;
using AspPlayground.Services;
using AspPlayground.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AspPlayground.Tests.UnitTests.Controllers;

// GET api/Vehicles/{id}
public class GetVehicleByIdTests
{
    private readonly Mock<IVehicleService> _serviceMock;
    private readonly VehiclesController _controller;

    public GetVehicleByIdTests()
    {
        _serviceMock = new Mock<IVehicleService>();
        _controller = new VehiclesController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_VehicleExists_ReturnsOkWithTheVehicle()
    {
        // Arrange
        VehicleResponse vehicle = TestData.CreateResponse(5);
        _serviceMock.Setup(service => service.GetByIdAsync(5)).ReturnsAsync(vehicle);

        // Act
        ActionResult<VehicleResponse> result = await _controller.GetByIdAsync(5);

        // Assert
        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(vehicle, ok.Value);
    }

    [Fact]
    public async Task GetByIdAsync_VehicleDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _serviceMock.Setup(service => service.GetByIdAsync(99)).ReturnsAsync((VehicleResponse?)null);

        // Act
        ActionResult<VehicleResponse> result = await _controller.GetByIdAsync(99);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
}
