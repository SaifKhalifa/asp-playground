using AspPlayground.Entities;
using AspPlayground.Validation;
using System.ComponentModel.DataAnnotations;

namespace AspPlayground.DTOs;

public class VehicleRequest
{
    [Required]
    [MaxLength(100)]
    public string Brand { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [ManufacturingYearRange(1886)]
    public int ManufacturingYear { get; set; }

    [Required]
    [Range(0.01, 999999999999.99, ErrorMessage = "Price must be between 0.01 and 999,999,999,999.99")]
    public decimal Price { get; set; }

    [Required]
    public string Color { get; set; } = string.Empty;

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Mileage must be a non negative integer.")]
    public int Mileage { get; set; }

    [Required]
    public AvailabilityStatus AvailabilityStatus { get; set; } = AvailabilityStatus.Available;
}
