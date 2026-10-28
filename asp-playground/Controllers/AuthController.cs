using asp_playground.DTOs;
using asp_playground.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace asp_playground.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController(IUserService userService, JwtTokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(ProfileCreationRequest request)
    {
        var user = await userService.CreateUserAsync(request.Name, request.Email, request.Password, request.Role);

        if (user == null)
            return BadRequest(new { message = "Email is already registered." });

        return StatusCode(StatusCodes.Status201Created,
            new { user.Id, user.Name, user.Email, Role = user.Role.ToString() });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await userService.AuthenticateUser(request.Email, request.Password);

        if (user == null)
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(new { token = tokenService.CreateToken(user) });
    }
}
