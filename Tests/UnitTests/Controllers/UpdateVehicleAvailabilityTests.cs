using AspPlayground.Controllers;
using AspPlayground.DTOs;
using AspPlayground.Entities;
using AspPlayground.Services;
using AspPlayground.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AspPlayground.Tests.UnitTests.Controllers;

// PUT api/Vehicles/{id}/availability
public class UpdateVehicleAvailabilityTests
{
    private readonly Mock<IVehicleService> _serviceMock;
    private readonly VehiclesController _controller;

    public UpdateVehicleAvailabilityTests()
    {
        _serviceMock = new Mock<IVehicleService>();
        _controller = new VehiclesController(_serviceMock.Object);
    }

    [Theory]
    [InlineData(AvailabilityStatus.Available)]
    [InlineData(AvailabilityStatus.Reserved)]
    [InlineData(AvailabilityStatus.Sold)]
    public async Task UpdateVehicleAvailabilityAsync_VehicleExists_ReturnsOk(AvailabilityStatus newStatus)
    {
        // Arrange
        VehicleResponse updated = TestData.CreateResponse(2);
        _serviceMock.Setup(service => service.UpdateAvailabilityStatusAsync(2, newStatus)).ReturnsAsync(updated);

        // Act
        ActionResult<VehicleResponse?> result = await _controller.UpdateVehicleAvailabilityAsync(2, newStatus);

        // Assert
        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(updated, ok.Value);
    }

    [Fact]
    public async Task UpdateVehicleAvailabilityAsync_VehicleDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _serviceMock.Setup(service => service.UpdateAvailabilityStatusAsync(99, AvailabilityStatus.Sold)).ReturnsAsync((VehicleResponse?)null);

        // Act
        ActionResult<VehicleResponse?> result = await _controller.UpdateVehicleAvailabilityAsync(99, AvailabilityStatus.Sold);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
}
