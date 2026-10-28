using asp_playground.Data;
using asp_playground.Entities;
using Microsoft.EntityFrameworkCore;

namespace asp_playground.Services;

public class UserServiceImpl(AppDbcontext db) : IUserService
{
    public async Task<UserProfile?> CreateUserAsync(string name, string email, string password, UserRole role)
    {
        email = email.Trim().ToLowerInvariant();

        if (await db.UserProfiles.AnyAsync(u => u.Email == email))
            return null;

        var user = new UserProfile { Name = name.Trim(), Email = email, Role = role };

        user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        db.UserProfiles.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public Task<UserProfile?> GetUserByIdAsync(int id)
    {
        return db.UserProfiles.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<UserProfile?> AuthenticateUser(string email, string password)
    {
        email = email.Trim().ToLowerInvariant();

        var user = await db.UserProfiles.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.HashedPassword))
            return null;

        return user;
    }
}
