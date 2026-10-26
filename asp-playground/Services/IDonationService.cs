using asp_playground.DTOs;

namespace asp_playground.Services;

public interface IDonationService
{
    Task<PagedResponse<DonationResponse>> GetAllAsync(PaginationQuery query);
    Task<DonationResponse?> GetByIdAsync(int id);
    Task<DonationResponse> CreateAsync(DonationRequest request);
    Task<DonationResponse?> UpdateAsync(int id, DonationRequest request);
    Task<bool> DeleteAsync(int id);
}
