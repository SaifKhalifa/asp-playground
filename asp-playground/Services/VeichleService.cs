using AspPlayground.Data;
using AspPlayground.DTOs;
using AspPlayground.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspPlayground.Services;

public class VehicleService : IVehicleService
{
    private readonly AppDbContext _db;
    public VehicleService(AppDbContext db)
    {
        _db = db;
    }

    // helpers
    private async Task<Vehicle>? GetVehicleByIdAsync(int id)
    {
        return await _db.Vehicles.FindAsync(id);
    }
    private async Task<(int totalCount, int totalPages, int skip)> CalculatePaginationQuery(PaginationQuery query, bool includeSkip = true)
    {
        int totalCount = await _db.Vehicles.CountAsync();
        int totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);
        int skip = 0;

        skip = includeSkip ? (query.Page - 1) * query.PageSize : 0;

        return (totalCount, totalPages, skip);
    }


    public async Task<PagedResponse<VehicleResponse>> GetAllAsync(PaginationQuery query)
    {
        (int totalCount, int totalPages, int skip) = await CalculatePaginationQuery(query);

        List<Vehicle> Vehicles = await _db.Vehicles
           .AsNoTracking()
           .OrderByDescending(v => v.ManufacturingYear)
           .ThenByDescending(v => v.Id)
           .Skip(skip)
           .Take(query.PageSize)
           .ToListAsync();

        List<VehicleResponse> items = new List<VehicleResponse>();
        foreach (Vehicle Vehicle in Vehicles)
        {
            items.Add(VehicleResponse.From(Vehicle));
        }

        return new PagedResponse<VehicleResponse>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
    public async Task<VehicleResponse?> GetByIdAsync(int id)
    {
        Vehicle? Vehicle = await _db.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);

        return Vehicle != null ? VehicleResponse.From(Vehicle) : null;
    }
    public async Task<VehicleResponse> CreateAsync(VehicleRequest request)
    {
        Vehicle Vehicle = new Vehicle
        {
            Brand = request.Brand,
            Model = request.Model,
            ManufacturingYear = request.ManufacturingYear,
            Price = request.Price,
            Color = request.Color,
            Mileage = request.Mileage,
            AvailabilityStatus = request.AvailabilityStatus
        };

        _db.Vehicles.Add(Vehicle);
        await _db.SaveChangesAsync();

        return VehicleResponse.From(Vehicle);
    }
    public async Task<VehicleResponse?> UpdateAsync(int id, VehicleRequest request)
    {
        Vehicle? Vehicle = await GetVehicleByIdAsync(id);
        if (Vehicle == null)
        {
            return null;
        }

        Vehicle.Brand = request.Brand;
        Vehicle.Model = request.Model;
        Vehicle.ManufacturingYear = request.ManufacturingYear;
        Vehicle.Price = request.Price;
        Vehicle.Color = request.Color;
        Vehicle.Mileage = request.Mileage;
        Vehicle.AvailabilityStatus = request.AvailabilityStatus;

        await _db.SaveChangesAsync();

        return VehicleResponse.From(Vehicle);
    }
    public async Task<bool> DeleteAsync(int id)
    {
        Vehicle? Vehicle = await GetVehicleByIdAsync(id);
        if (Vehicle == null)
        {
            return false;
        }

        _db.Vehicles.Remove(Vehicle);
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<PagedResponse<VehicleResponse>> FilterByBrandAndModelAsync(string? brand, string? model, PaginationQuery query)
    {
        IQueryable<Vehicle> filtered = _db.Vehicles
            .AsNoTracking()
            .Where(v => (brand == null || v.Brand == brand) && (model == null || v.Model == model));

        // Totals describe the filtered result set, not the whole table.
        int totalCount = await filtered.CountAsync();
        int totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);
        int skip = (query.Page - 1) * query.PageSize;

        List<Vehicle> Vehicles = await filtered
            .OrderBy(v => v.Id)
            .Skip(skip)
            .Take(query.PageSize)
            .ToListAsync();

        List<VehicleResponse> items = new List<VehicleResponse>();
        foreach (Vehicle Vehicle in Vehicles)
        {
            items.Add(VehicleResponse.From(Vehicle));
        }

        return new PagedResponse<VehicleResponse>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages

        };
    }

    public async Task<VehicleResponse?> UpdateAvailabilityStatusAsync(int id, AvailabilityStatus newStatus)
    {
        Vehicle? Vehicle = await GetVehicleByIdAsync(id);
        if (Vehicle == null)
        {
            return null;
        }

        Vehicle.AvailabilityStatus = newStatus;
        await _db.SaveChangesAsync();

        return VehicleResponse.From(Vehicle);
    }
}
