using BotanicalBuddy.API.Data;
using BotanicalBuddy.API.Data.Entities;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace BotanicalBuddy.API.Services;

public class UserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<User> CreateUserAsync(string email, string password, string? firstName = null, string? lastName = null)
    {
        // Check if user already exists
        var existingUser = await GetUserByEmailAsync(email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Hash the password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            SubscriptionTier = "Free",
            EmailVerified = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public bool VerifyPassword(User user, string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<int> GetUserPlantCountAsync(Guid userId)
    {
        return await _context.UserPlants.CountAsync(up => up.UserId == userId);
    }

    public async Task<bool> CanAddPlantAsync(Guid userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null) return false;

        var plantCount = await GetUserPlantCountAsync(userId);

        return user.SubscriptionTier switch
        {
            "Free" => plantCount < 5,
            "Basic" => plantCount < 25,
            "Premium" => true,
            _ => false
        };
    }
}
