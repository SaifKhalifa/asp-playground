using AspPlayground.DTOs;
using AspPlayground.Entities;

namespace AspPlayground.Services;

/*
1. [x] Add a new vehicle to the inventory.
2. [x] Update existing vehicle details.
3. [x] Delete a vehicle record.
4. [x] Retrieve a specific vehicle by ID.
5. [x] Retrieve all available vehicles.
6. [x] Filter vehicles dynamically by brand (Chery, OMODA, JAECOO) and model.
7. [x] Update vehicle availability status.
*/
public interface IVehicleService
{
    Task<PagedResponse<VehicleResponse>> GetAllAsync(PaginationQuery query);
    Task<VehicleResponse?> GetByIdAsync(int id);
    Task<VehicleResponse> CreateAsync(VehicleRequest request);
    Task<VehicleResponse?> UpdateAsync(int id, VehicleRequest request);
    Task<bool> DeleteAsync(int id);
    Task<PagedResponse<VehicleResponse>> FilterByBrandAndModelAsync(string? brand, string? model, PaginationQuery query);
    Task<VehicleResponse?> UpdateAvailabilityStatusAsync(int id, AvailabilityStatus newStatus);    
}
