using AspPlayground.DTOs;
using AspPlayground.Entities;

namespace AspPlayground.Tests.Helpers;

public static class TestData
{
    public static VehicleRequest CreateRequest()
    {
        return new VehicleRequest
        {
            Brand = "Chery",
            Model = "Tiggo 8 Pro",
            ManufacturingYear = 2023,
            Price = 30000,
            Color = "White",
            Mileage = 0,
            AvailabilityStatus = AvailabilityStatus.Available
        };
    }

    public static Vehicle CreateVehicle(string brand = "Chery", string model = "Tiggo 8 Pro", int year = 2023)
    {
        return new Vehicle
        {
            Brand = brand,
            Model = model,
            ManufacturingYear = year,
            Price = 30000,
            Color = "White",
            Mileage = 0,
            AvailabilityStatus = AvailabilityStatus.Available
        };
    }

    public static VehicleResponse CreateResponse(int id = 1)
    {
        return new VehicleResponse(id, "Chery", "Tiggo 8 Pro", 2023, 30000, "White", 0, AvailabilityStatus.Available);
    }

    public static PagedResponse<VehicleResponse> CreatePage(params VehicleResponse[] items)
    {
        return new PagedResponse<VehicleResponse>
        {
            Items = items.ToList(),
            Page = 1,
            PageSize = 10,
            TotalCount = items.Length,
            TotalPages = 1
        };
    }
}
