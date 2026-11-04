using AspPlayground.DTOs;
using AspPlayground.Entities;
using AspPlayground.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AspPlayground.Controllers;

/*
1. [x] Add a new vehicle to the inventory.
2. [x] Update existing vehicle details.
3. [x] Delete a vehicle record.
4. [x] Retrieve a specific vehicle by ID.
5. [x] Retrieve all available vehicles.
6. [x] Filter vehicles dynamically by brand (Chery, OMODA, JAECOO) and model.
7. [x] Update vehicle availability status.
*/
[ApiController]
public class VehiclesController : ControllerBase
{
    /// <summary>Route name used by <see cref="CreateVehicleAsync"/> to build the Location header.</summary>
    public const string GetByIdRouteName = "GetVehicleById";

    private readonly IVehicleService _VehicleService;

    public VehiclesController(IVehicleService VehicleService)
    {
        _VehicleService = VehicleService;
    }

    // get endoints

    [HttpGet("api/Vehicles")]
    [ProducesResponseType<PagedResponse<VehicleResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<VehicleResponse>>> GetAllAsync([FromQuery] PaginationQuery query)
    {
        PagedResponse<VehicleResponse> page = await _VehicleService.GetAllAsync(query);
        return Ok(page);
    }

    [HttpGet("api/Vehicles/{id}", Name = GetByIdRouteName)]
    [ProducesResponseType<VehicleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleResponse>> GetByIdAsync(int id)
    {
        VehicleResponse? Vehicle = await _VehicleService.GetByIdAsync(id);
        if (Vehicle == null)
        {
            return NotFound();
        }

        return Ok(Vehicle);
    }

    // delete endpoint

    [HttpDelete("api/Vehicles/{id}")]
    [ProducesResponseType<bool>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> DeleteVehicleAsync(int id)
    {
        bool isDeleted = await _VehicleService.DeleteAsync(id);
        
        if (!isDeleted)
        {
            return NotFound();
        }

        return Ok(isDeleted);
    }

    // filter endpoints

    [HttpGet("api/Vehicles/filter")]
    [ProducesResponseType<PagedResponse<VehicleResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<VehicleResponse>>> FilterByBrandAndModelAsync(string? brand, string? model, [FromQuery] PaginationQuery query)
    {
        PagedResponse<VehicleResponse> page = await _VehicleService.FilterByBrandAndModelAsync(brand, model, query);
        return Ok(page);
    }

    // update endpoints
    [HttpPut("api/Vehicles/{id}")]
    [ProducesResponseType<VehicleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleResponse>> UpdateVehicleAsync(int id, VehicleRequest request)
    {
        VehicleResponse? Vehicle = await _VehicleService.UpdateAsync(id, request);
        if (Vehicle == null)
        {
            return NotFound();
        }

        return Ok(Vehicle);
    }

    [HttpPut("api/Vehicles/{id}/availability")]
    [ProducesResponseType<VehicleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleResponse?>> UpdateVehicleAvailabilityAsync(int id, AvailabilityStatus newState)
    {
        VehicleResponse? Vehicle = await _VehicleService.UpdateAvailabilityStatusAsync(id, newState);
        if (Vehicle == null)
        {
            return NotFound();
        }

        return Ok(Vehicle);
    }

    // create endpoint
    [HttpPost("api/Vehicles")]
    [ProducesResponseType<VehicleResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VehicleResponse>> CreateVehicleAsync(VehicleRequest request)
    {
        VehicleResponse Vehicle = await _VehicleService.CreateAsync(request);
        return CreatedAtRoute(GetByIdRouteName, new { id = Vehicle.Id }, Vehicle);
    }
}
