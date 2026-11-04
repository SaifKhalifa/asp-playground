using AspPlayground.Controllers;
using AspPlayground.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AspPlayground.Tests.UnitTests.Controllers;

// DELETE api/Vehicles/{id}
public class DeleteVehicleTests
{
    private readonly Mock<IVehicleService> _serviceMock;
    private readonly VehiclesController _controller;

    public DeleteVehicleTests()
    {
        _serviceMock = new Mock<IVehicleService>();
        _controller = new VehiclesController(_serviceMock.Object);
    }

    [Fact]
    public async Task DeleteVehicleAsync_VehicleExists_ReturnsOk()
    {
        // Arrange
        _serviceMock.Setup(service => service.DeleteAsync(4)).ReturnsAsync(true);

        // Act
        ActionResult<bool> result = await _controller.DeleteVehicleAsync(4);

        // Assert
        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(true, ok.Value);
    }

    [Fact]
    public async Task DeleteVehicleAsync_VehicleDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _serviceMock.Setup(service => service.DeleteAsync(99)).ReturnsAsync(false);

        // Act
        ActionResult<bool> result = await _controller.DeleteVehicleAsync(99);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
}
