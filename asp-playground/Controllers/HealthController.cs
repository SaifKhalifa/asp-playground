using Microsoft.AspNetCore.Mvc;

namespace asp_playground.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    /// <summary>Reports that the API is running.</summary>
    [HttpGet]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    public ActionResult<HealthResponse> Get()
    {
        HealthResponse response = new HealthResponse("Healthy", DateTime.UtcNow);
        return Ok(response);
    }

    public record HealthResponse(string Status, DateTime Timestamp);
}
