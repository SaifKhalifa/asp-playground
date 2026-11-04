using AspPlayground.Entities;

namespace AspPlayground.DTOs;

public record VehicleResponse(
    int Id,
    string Brand,
    string Model,
    int ManufacturingYear,
    decimal Price,
    string Color,
    int Mileage,
    AvailabilityStatus AvailabilityStatus
)
{
    public static VehicleResponse From(Vehicle Vehicle)
    {
        return new VehicleResponse(
            Vehicle.Id,
            Vehicle.Brand,
            Vehicle.Model,
            Vehicle.ManufacturingYear,
            Vehicle.Price,
            Vehicle.Color,
            Vehicle.Mileage,
            Vehicle.AvailabilityStatus
        );
    }
}
