using System.ComponentModel.DataAnnotations;

namespace asp_playground.DTOs;

/// <summary>Payload used to create or update a donation.</summary>
public class DonationRequest
{
    /// <summary>Donation amount; must be greater than zero.</summary>
    [Required]
    [Range(typeof(decimal), "0.01", "999999999999", ErrorMessage = "Amount must be greater than zero.")]
    public decimal? Amount { get; set; }

    /// <summary>Name of the donor. Always stored, but never returned when the donation is anonymous.</summary>
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string? DonorName { get; set; }

    /// <summary>Whether the donor name must be hidden in responses.</summary>
    [Required]
    public bool? IsAnonymous { get; set; }
}
