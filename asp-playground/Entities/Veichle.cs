namespace AspPlayground.Entities;

public enum AvailabilityStatus
{
    Available,
    Reserved,
    Sold
}
public class Vehicle
{
    public int Id { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int ManufacturingYear { get; set; }
    public decimal Price { get; set; }
    public string Color { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public AvailabilityStatus AvailabilityStatus { get; set; }
}