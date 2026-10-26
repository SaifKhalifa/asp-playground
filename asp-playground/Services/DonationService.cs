using asp_playground.Data;
using asp_playground.DTOs;
using asp_playground.Entities;
using Microsoft.EntityFrameworkCore;

namespace asp_playground.Services;

public class DonationService : IDonationService
{
    private readonly AppDbContext _db;

    public DonationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResponse<DonationResponse>> GetAllAsync(PaginationQuery query)
    {
        int totalCount = await _db.Donations.CountAsync();
        int totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);
        int skip = (query.Page - 1) * query.PageSize;

        List<Donation> donations = await _db.Donations
            .AsNoTracking()
            .OrderByDescending(d => d.CreatedAt)
            .ThenByDescending(d => d.Id)
            .Skip(skip)
            .Take(query.PageSize)
            .ToListAsync();

        List<DonationResponse> items = new List<DonationResponse>();
        foreach (Donation donation in donations)
        {
            items.Add(DonationResponse.From(donation));
        }

        return new PagedResponse<DonationResponse>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<DonationResponse?> GetByIdAsync(int id)
    {
        Donation? donation = await _db.Donations
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (donation == null)
        {
            return null;
        }

        return DonationResponse.From(donation);
    }

    public async Task<DonationResponse> CreateAsync(DonationRequest request)
    {
        Donation donation = new Donation
        {
            Amount = request.Amount!.Value,
            DonorName = request.DonorName!.Trim(),
            IsAnonymous = request.IsAnonymous!.Value,
            CreatedAt = DateTime.UtcNow
        };

        _db.Donations.Add(donation);
        await _db.SaveChangesAsync();

        return DonationResponse.From(donation);
    }

    public async Task<DonationResponse?> UpdateAsync(int id, DonationRequest request)
    {
        Donation? donation = await _db.Donations.FirstOrDefaultAsync(d => d.Id == id);

        if (donation == null)
        {
            return null;
        }

        donation.Amount = request.Amount!.Value;
        donation.DonorName = request.DonorName!.Trim();
        donation.IsAnonymous = request.IsAnonymous!.Value;
        await _db.SaveChangesAsync();

        return DonationResponse.From(donation);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Donation? donation = await _db.Donations.FirstOrDefaultAsync(d => d.Id == id);

        if (donation == null)
        {
            return false;
        }

        _db.Donations.Remove(donation);
        await _db.SaveChangesAsync();

        return true;
    }
}
