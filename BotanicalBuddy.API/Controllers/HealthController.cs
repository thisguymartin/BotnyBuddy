using Microsoft.AspNetCore.Mvc;
using BotanicalBuddy.API.Data;

namespace BotanicalBuddy.API.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        try
        {
            // Check database connection
            var canConnect = _context.Database.CanConnect();

            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                database = canConnect ? "connected" : "disconnected",
                version = "1.0.0"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new
            {
                status = "unhealthy",
                timestamp = DateTime.UtcNow,
                error = ex.Message
            });
        }
    }
}
