namespace asp_playground.Entities;

public enum UserRole
{
    Admin,
    User
}
public class UserProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
}
