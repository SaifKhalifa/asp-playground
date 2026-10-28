using asp_playground.Entities;

namespace asp_playground.Services;

public interface IUserService
{
    // register = returns null when the email is already taken
    Task<UserProfile?> CreateUserAsync(string name, string email, string password, UserRole role);

    // get profile by id
    Task<UserProfile?> GetUserByIdAsync(int id);

    // for authentication = returns null when email or password is wrong
    Task<UserProfile?> AuthenticateUser(string email, string password);
}
