using System.ComponentModel.DataAnnotations;
using asp_playground.Entities;

namespace asp_playground.DTOs;

public class ProfileCreationRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;
}
