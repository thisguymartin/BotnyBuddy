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
