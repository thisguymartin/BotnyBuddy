using System.ComponentModel.DataAnnotations;

namespace BotanicalBuddy.API.Models;

public class TokenRequest
{
    public string Username { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
}

public class TokenResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? ExpiresIn { get; set; }
    public string? TokenType { get; set; }
    public string? Usage { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
}

public class RefreshTokenRequest
{
    public string Username { get; set; } = string.Empty;
}

public class TokenVerifyResponse
{
    public bool Success { get; set; }
    public bool Valid { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
}

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public UserDto? User { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string SubscriptionTier { get; set; } = "Free";
    public bool EmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}
