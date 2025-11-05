using BotanicalBuddy.API.Models;
using BotanicalBuddy.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BotanicalBuddy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtTokenService _jwtTokenService;
    private readonly UserService _userService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        JwtTokenService jwtTokenService,
        UserService userService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _jwtTokenService = jwtTokenService;
        _userService = userService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Generate a JWT token
    /// </summary>
    /// <remarks>
    /// Simple authentication endpoint for demo purposes.
    /// In production, verify credentials against a database.
    /// </remarks>
    [HttpPost("token")]
    public IActionResult GenerateToken([FromBody] TokenRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest(new TokenResponse
                {
                    Success = false,
                    Error = "Missing or invalid username"
                });
            }

            // Simple validation - in production, verify against database
            var expectedApiKey = _configuration["Auth:ApiKey"] ?? "demo-api-key";

            if (!string.IsNullOrEmpty(request.ApiKey) && request.ApiKey != expectedApiKey)
            {
                _logger.LogWarning("Failed authentication attempt for user: {Username}", request.Username);
                return Unauthorized(new TokenResponse
                {
                    Success = false,
                    Error = "Invalid API key"
                });
            }

            var token = _jwtTokenService.GenerateToken(request.Username);

            return Ok(new TokenResponse
            {
                Success = true,
                Token = token,
                ExpiresIn = "24 hours",
                TokenType = "Bearer",
                Usage = "Include in Authorization header as: Bearer <token>"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token");
            return StatusCode(500, new TokenResponse
            {
                Success = false,
                Error = "Failed to generate token",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Refresh an existing JWT token
    /// </summary>
    [HttpPost("refresh")]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest(new TokenResponse
                {
                    Success = false,
                    Error = "Missing or invalid username"
                });
            }

            var newToken = _jwtTokenService.GenerateToken(request.Username);

            _logger.LogInformation("Token refreshed for user: {Username}", request.Username);

            return Ok(new TokenResponse
            {
                Success = true,
                Token = newToken,
                ExpiresIn = "24 hours",
                TokenType = "Bearer"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, new TokenResponse
            {
                Success = false,
                Error = "Failed to refresh token",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Verify a JWT token
    /// </summary>
    [HttpGet("verify")]
    public IActionResult VerifyToken()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return Unauthorized(new TokenVerifyResponse
            {
                Success = false,
                Valid = false,
                Error = "Missing or invalid authorization header"
            });
        }

        try
        {
            var token = authHeader.Substring(7);
            var principal = _jwtTokenService.ValidateToken(token);

            if (principal == null)
            {
                return Unauthorized(new TokenVerifyResponse
                {
                    Success = false,
                    Valid = false,
                    Error = "Invalid token"
                });
            }

            return Ok(new TokenVerifyResponse
            {
                Success = true,
                Valid = true,
                Message = "Token is valid"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying token");
            return Unauthorized(new TokenVerifyResponse
            {
                Success = false,
                Valid = false,
                Error = "Invalid token",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Error = "Invalid input",
                    Message = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });
            }

            var user = await _userService.CreateUserAsync(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName
            );

            var token = _jwtTokenService.GenerateTokenForUser(user);

            _logger.LogInformation("New user registered: {Email}", user.Email);

            return Ok(new AuthResponse
            {
                Success = true,
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    SubscriptionTier = user.SubscriptionTier,
                    EmailVerified = user.EmailVerified,
                    CreatedAt = user.CreatedAt
                },
                Message = "Registration successful"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration failed: {Error}", ex.Message);
            return BadRequest(new AuthResponse
            {
                Success = false,
                Error = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new AuthResponse
            {
                Success = false,
                Error = "Registration failed",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Error = "Invalid input"
                });
            }

            var user = await _userService.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Email}", request.Email);
                return Unauthorized(new AuthResponse
                {
                    Success = false,
                    Error = "Invalid email or password"
                });
            }

            if (!_userService.VerifyPassword(user, request.Password))
            {
                _logger.LogWarning("Invalid password attempt for user: {Email}", request.Email);
                return Unauthorized(new AuthResponse
                {
                    Success = false,
                    Error = "Invalid email or password"
                });
            }

            var token = _jwtTokenService.GenerateTokenForUser(user);

            _logger.LogInformation("User logged in: {Email}", user.Email);

            return Ok(new AuthResponse
            {
                Success = true,
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    SubscriptionTier = user.SubscriptionTier,
                    EmailVerified = user.EmailVerified,
                    CreatedAt = user.CreatedAt
                },
                Message = "Login successful"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new AuthResponse
            {
                Success = false,
                Error = "Login failed",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid token" });
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(new
            {
                success = true,
                user = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    SubscriptionTier = user.SubscriptionTier,
                    EmailVerified = user.EmailVerified,
                    CreatedAt = user.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { error = "Failed to get user profile" });
        }
    }
}
