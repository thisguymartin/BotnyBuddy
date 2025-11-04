using BotanicalBuddy.API.Models;
using BotanicalBuddy.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BotanicalBuddy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        JwtTokenService jwtTokenService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _jwtTokenService = jwtTokenService;
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
}
