using System.ComponentModel.DataAnnotations;
using AspPlayground.DTOs;
using AspPlayground.Tests.Helpers;

namespace AspPlayground.Tests.UnitTests.Validation;

public class VehicleRequestValidationTests
{
    private static bool IsValid(VehicleRequest request)
    {
        List<ValidationResult> errors = new List<ValidationResult>();
        return Validator.TryValidateObject(request, new ValidationContext(request), errors, validateAllProperties: true);
    }

    [Fact]
    public void ValidRequest_IsValid()
    {
        Assert.True(IsValid(TestData.CreateRequest()));
    }

    [Fact]
    public void EmptyBrand_IsInvalid()
    {
        VehicleRequest request = TestData.CreateRequest();
        request.Brand = "";

        Assert.False(IsValid(request));
    }

    [Fact]
    public void BrandLongerThan100Characters_IsInvalid()
    {
        VehicleRequest request = TestData.CreateRequest();
        request.Brand = new string('a', 101);

        Assert.False(IsValid(request));
    }

    [Fact]
    public void EmptyModel_IsInvalid()
    {
        VehicleRequest request = TestData.CreateRequest();
        request.Model = "";

        Assert.False(IsValid(request));
    }

    [Fact]
    public void ModelLongerThan100Characters_IsInvalid()
    {
        VehicleRequest request = TestData.CreateRequest();
        request.Model = new string('a', 101);

        Assert.False(IsValid(request));
    }

    [Fact]
    public void EmptyColor_IsInvalid()
    {
        VehicleRequest request = TestData.CreateRequest();
        request.Color = "";

        Assert.False(IsValid(request));
    }

    [Theory]
    [InlineData(1886)]
    [InlineData(2000)]
    [InlineData(2023)]
    public void ValidYear_IsValid(int year)
    {
        VehicleRequest request = TestData.CreateRequest();
        request.ManufacturingYear = year;

        Assert.True(IsValid(request));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1885)]
    [InlineData(20211)]
    public void InvalidYear_IsInvalid(int year)
    {
        VehicleRequest request = TestData.CreateRequest();
        request.ManufacturingYear = year;

        Assert.False(IsValid(request));
    }

    [Fact]
    public void YearTwoYearsInTheFuture_IsInvalid()
    {
        VehicleRequest request = TestData.CreateRequest();
        request.ManufacturingYear = DateTime.UtcNow.Year + 2;

        Assert.False(IsValid(request));
    }

    [Fact]
    public void ZeroPrice_IsInvalid()
    {
        VehicleRequest request = TestData.CreateRequest();
        request.Price = 0;

        Assert.False(IsValid(request));
    }

    [Fact]
    public void NegativePrice_IsInvalid()
    {
        VehicleRequest request = TestData.CreateRequest();
        request.Price = -1;

        Assert.False(IsValid(request));
    }

    [Fact]
    public void NegativeMileage_IsInvalid()
    {
        VehicleRequest request = TestData.CreateRequest();
        request.Mileage = -1;

        Assert.False(IsValid(request));
    }

    [Fact]
    public void ZeroMileage_IsValid()
    {
        VehicleRequest request = TestData.CreateRequest();
        request.Mileage = 0;

        Assert.True(IsValid(request));
    }
}
