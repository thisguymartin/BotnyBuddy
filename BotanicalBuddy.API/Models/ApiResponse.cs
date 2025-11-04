namespace BotanicalBuddy.API.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
    public object? Meta { get; set; }
    public object? Links { get; set; }
    public string? Query { get; set; }
    public object? Filter { get; set; }
}
