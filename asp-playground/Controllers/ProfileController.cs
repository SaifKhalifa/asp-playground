using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace asp_playground.Controllers;

[ApiController]
[Route("api")]
[Authorize] // every endpoint here needs authorization
public class ProfileController : ControllerBase
{
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        return Ok(new
        {
            Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Name = User.FindFirstValue(ClaimTypes.Name),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Role = User.FindFirstValue(ClaimTypes.Role)
        });
    }

    [HttpGet("user")]
    [Authorize(Roles = "User,Admin")]
    public IActionResult UserArea()
    {
        return Ok(new { message = $"Hello {User.Identity?.Name}, you have User/Admin access." });
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminArea()
    {
        return Ok(new { message = $"Hello {User.Identity?.Name}, you have Admin access." });
    }
}
