using AspPlayground.Controllers;
using AspPlayground.DTOs;
using AspPlayground.Services;
using AspPlayground.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AspPlayground.Tests.UnitTests.Controllers;

// POST api/Vehicles
public class CreateVehicleTests
{
    private readonly Mock<IVehicleService> _serviceMock;
    private readonly VehiclesController _controller;

    public CreateVehicleTests()
    {
        _serviceMock = new Mock<IVehicleService>();
        _controller = new VehiclesController(_serviceMock.Object);
    }

    [Fact]
    public async Task CreateVehicleAsync_ValidRequest_ReturnsCreatedWithTheNewVehicle()
    {
        // Arrange
        VehicleRequest request = TestData.CreateRequest();
        VehicleResponse created = TestData.CreateResponse(7);
        _serviceMock.Setup(service => service.CreateAsync(request)).ReturnsAsync(created);

        // Act
        ActionResult<VehicleResponse> result = await _controller.CreateVehicleAsync(request);

        // Assert
        CreatedAtRouteResult createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(created, createdResult.Value);
    }

    [Fact]
    public async Task CreateVehicleAsync_ValidRequest_CallsTheServiceOnce()
    {
        // Arrange
        VehicleRequest request = TestData.CreateRequest();
        _serviceMock.Setup(service => service.CreateAsync(request)).ReturnsAsync(TestData.CreateResponse());

        // Act
        await _controller.CreateVehicleAsync(request);

        // Assert
        _serviceMock.Verify(service => service.CreateAsync(request), Times.Once);
    }
}
