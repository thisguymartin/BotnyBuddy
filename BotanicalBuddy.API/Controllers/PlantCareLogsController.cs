using BotanicalBuddy.API.Data;
using BotanicalBuddy.API.Data.Entities;
using BotanicalBuddy.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BotanicalBuddy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlantCareLogsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PlantCareLogsController> _logger;

    public PlantCareLogsController(
        ApplicationDbContext context,
        ILogger<PlantCareLogsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }

    /// <summary>
    /// Get all care logs for a specific plant
    /// </summary>
    [HttpGet("plant/{plantId}")]
    public async Task<IActionResult> GetPlantCareLogs(Guid plantId)
    {
        try
        {
            var userId = GetUserId();

            // Verify the plant belongs to the user
            var plantExists = await _context.UserPlants
                .AnyAsync(p => p.Id == plantId && p.UserId == userId);

            if (!plantExists)
            {
                return NotFound(new { error = "Plant not found" });
            }

            var logs = await _context.PlantCareLogs
                .Where(l => l.UserPlantId == plantId)
                .OrderByDescending(l => l.DateTime)
                .Select(l => new PlantCareLogDto
                {
                    Id = l.Id,
                    UserPlantId = l.UserPlantId,
                    CareType = l.CareType,
                    DateTime = l.DateTime,
                    Amount = l.Amount,
                    Notes = l.Notes,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return Ok(new { success = true, data = logs, count = logs.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving care logs for plant {PlantId}", plantId);
            return StatusCode(500, new { error = "Failed to retrieve care logs" });
        }
    }

    /// <summary>
    /// Get a specific care log by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCareLog(Guid id)
    {
        try
        {
            var userId = GetUserId();

            var log = await _context.PlantCareLogs
                .Include(l => l.UserPlant)
                .Where(l => l.Id == id && l.UserPlant.UserId == userId)
                .Select(l => new PlantCareLogDto
                {
                    Id = l.Id,
                    UserPlantId = l.UserPlantId,
                    CareType = l.CareType,
                    DateTime = l.DateTime,
                    Amount = l.Amount,
                    Notes = l.Notes,
                    CreatedAt = l.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (log == null)
            {
                return NotFound(new { error = "Care log not found" });
            }

            return Ok(new { success = true, data = log });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving care log {LogId}", id);
            return StatusCode(500, new { error = "Failed to retrieve care log" });
        }
    }

    /// <summary>
    /// Create a new care log
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCareLog([FromBody] CreatePlantCareLogRequest request)
    {
        try
        {
            var userId = GetUserId();

            // Verify the plant belongs to the user
            var plantExists = await _context.UserPlants
                .AnyAsync(p => p.Id == request.UserPlantId && p.UserId == userId);

            if (!plantExists)
            {
                return BadRequest(new { error = "Invalid plant" });
            }

            var log = new PlantCareLog
            {
                UserPlantId = request.UserPlantId,
                CareType = request.CareType,
                DateTime = request.DateTime ?? DateTime.UtcNow,
                Amount = request.Amount,
                Notes = request.Notes
            };

            _context.PlantCareLogs.Add(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Care log created for plant {PlantId}: {LogId}", request.UserPlantId, log.Id);

            return CreatedAtAction(nameof(GetCareLog), new { id = log.Id }, new
            {
                success = true,
                data = new PlantCareLogDto
                {
                    Id = log.Id,
                    UserPlantId = log.UserPlantId,
                    CareType = log.CareType,
                    DateTime = log.DateTime,
                    Amount = log.Amount,
                    Notes = log.Notes,
                    CreatedAt = log.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating care log");
            return StatusCode(500, new { error = "Failed to create care log" });
        }
    }

    /// <summary>
    /// Delete a care log
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCareLog(Guid id)
    {
        try
        {
            var userId = GetUserId();

            var log = await _context.PlantCareLogs
                .Include(l => l.UserPlant)
                .FirstOrDefaultAsync(l => l.Id == id && l.UserPlant.UserId == userId);

            if (log == null)
            {
                return NotFound(new { error = "Care log not found" });
            }

            _context.PlantCareLogs.Remove(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Care log deleted: {LogId}", id);

            return Ok(new { success = true, message = "Care log deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting care log {LogId}", id);
            return StatusCode(500, new { error = "Failed to delete care log" });
        }
    }

    /// <summary>
    /// Get care statistics for a plant
    /// </summary>
    [HttpGet("plant/{plantId}/statistics")]
    public async Task<IActionResult> GetPlantCareStatistics(Guid plantId)
    {
        try
        {
            var userId = GetUserId();

            // Verify the plant belongs to the user
            var plantExists = await _context.UserPlants
                .AnyAsync(p => p.Id == plantId && p.UserId == userId);

            if (!plantExists)
            {
                return NotFound(new { error = "Plant not found" });
            }

            var logs = await _context.PlantCareLogs
                .Where(l => l.UserPlantId == plantId)
                .ToListAsync();

            var statistics = logs
                .GroupBy(l => l.CareType)
                .Select(g => new
                {
                    careType = g.Key,
                    count = g.Count(),
                    lastEntry = g.Max(l => l.DateTime),
                    firstEntry = g.Min(l => l.DateTime)
                })
                .ToList();

            return Ok(new
            {
                success = true,
                data = new
                {
                    totalLogs = logs.Count,
                    careTypes = statistics
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving care statistics for plant {PlantId}", plantId);
            return StatusCode(500, new { error = "Failed to retrieve care statistics" });
        }
    }
}
