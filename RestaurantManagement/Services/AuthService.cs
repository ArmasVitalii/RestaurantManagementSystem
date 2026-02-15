using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Helpers;
using RestaurantManagement.Models;

namespace RestaurantManagement.Services;

public class AuthService
{
    private readonly RestaurantDbContext _dbContext;

    public AuthService()
    {
        _dbContext = new RestaurantDbContext();
    }

    public async Task<User?> LoginAsync(string email, string password)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null) return null;

        bool isPasswordValid = PasswordHasher.VerifyPassword(password, user.PasswordHash);
        return isPasswordValid ? user : null;
    }

    public async Task<User> RegisterAsync(User user, string password)
    {
        // Hash password before storing
        user.PasswordHash = PasswordHasher.HashPassword(password);
        
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    public User CreateGuestUser()
    {
        // Create a temporary guest user that isn't saved in the database
        return new User
        {
            UserID = -1, // Temporary ID
            FirstName = "Guest",
            LastName = "User",
            UserType = "Guest"
        };
    }
} 