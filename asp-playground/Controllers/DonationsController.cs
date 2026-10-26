using asp_playground.DTOs;
using asp_playground.Services;
using Microsoft.AspNetCore.Mvc;

namespace asp_playground.Controllers;

[ApiController]
[Route("api/donations")]
[Produces("application/json")]
public class DonationsController : ControllerBase
{
    private readonly IDonationService _service;

    public DonationsController(IDonationService service)
    {
        _service = service;
    }

    /// <summary>Get a page of donations, newest first (donor names of anonymous donations are hidden).</summary>
    [HttpGet]
    [ProducesResponseType<PagedResponse<DonationResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<DonationResponse>>> GetAll([FromQuery] PaginationQuery query)
    {
        PagedResponse<DonationResponse> page = await _service.GetAllAsync(query);
        return Ok(page);
    }

    /// <summary>Get a donation by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType<DonationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DonationResponse>> GetById(int id)
    {
        DonationResponse? donation = await _service.GetByIdAsync(id);

        if (donation == null)
        {
            return NotFound();
        }

        return Ok(donation);
    }

    /// <summary>Create a donation.</summary>
    [HttpPost]
    [ProducesResponseType<DonationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DonationResponse>> Create(DonationRequest request)
    {
        DonationResponse created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Update a donation.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType<DonationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DonationResponse>> Update(int id, DonationRequest request)
    {
        DonationResponse? updated = await _service.UpdateAsync(id, request);

        if (updated == null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    /// <summary>Delete a donation.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        bool deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
