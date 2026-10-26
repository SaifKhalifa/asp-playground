using asp_playground.Entities;

namespace asp_playground.DTOs;

/// <summary>Public view of a donation. The donor name is null for anonymous donations.</summary>
public record DonationResponse(int Id, decimal Amount, string? DonorName, bool IsAnonymous, DateTime CreatedAt)
{
    /// <summary>Single mapping point from entity to response; this is where anonymity is enforced.</summary>
    public static DonationResponse From(Donation donation)
    {
        string? donorName = donation.DonorName;
        if (donation.IsAnonymous)
        {
            donorName = null;
        }

        return new DonationResponse(donation.Id, donation.Amount, donorName, donation.IsAnonymous, donation.CreatedAt);
    }
}
