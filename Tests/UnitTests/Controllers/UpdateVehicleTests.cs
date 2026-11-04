using AspPlayground.Controllers;
using AspPlayground.DTOs;
using AspPlayground.Services;
using AspPlayground.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AspPlayground.Tests.UnitTests.Controllers;

// PUT api/Vehicles/{id}
public class UpdateVehicleTests
{
    private readonly Mock<IVehicleService> _serviceMock;
    private readonly VehiclesController _controller;

    public UpdateVehicleTests()
    {
        _serviceMock = new Mock<IVehicleService>();
        _controller = new VehiclesController(_serviceMock.Object);
    }

    [Fact]
    public async Task UpdateVehicleAsync_VehicleExists_ReturnsOkWithTheUpdatedVehicle()
    {
        // Arrange
        VehicleRequest request = TestData.CreateRequest();
        VehicleResponse updated = TestData.CreateResponse(3);
        _serviceMock.Setup(service => service.UpdateAsync(3, request)).ReturnsAsync(updated);

        // Act
        ActionResult<VehicleResponse> result = await _controller.UpdateVehicleAsync(3, request);

        // Assert
        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(updated, ok.Value);
    }

    [Fact]
    public async Task UpdateVehicleAsync_VehicleDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        VehicleRequest request = TestData.CreateRequest();
        _serviceMock.Setup(service => service.UpdateAsync(99, request)).ReturnsAsync((VehicleResponse?)null);

        // Act
        ActionResult<VehicleResponse> result = await _controller.UpdateVehicleAsync(99, request);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
}
