using BotanicalBuddy.API.Data;
using BotanicalBuddy.API.Data.Entities;
using BotanicalBuddy.API.Models;
using BotanicalBuddy.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BotanicalBuddy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserPlantsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserService _userService;
    private readonly ILogger<UserPlantsController> _logger;

    public UserPlantsController(
        ApplicationDbContext context,
        UserService userService,
        ILogger<UserPlantsController> logger)
    {
        _context = context;
        _userService = userService;
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
    /// Get all plants for the authenticated user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUserPlants()
    {
        try
        {
            var userId = GetUserId();

            var plants = await _context.UserPlants
                .Include(p => p.Address)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new UserPlantDto
                {
                    Id = p.Id,
                    AddressId = p.AddressId,
                    TreflePlantId = p.TreflePlantId,
                    CommonName = p.CommonName,
                    ScientificName = p.ScientificName,
                    Nickname = p.Nickname,
                    DatePlanted = p.DatePlanted,
                    Location = p.Location,
                    Notes = p.Notes,
                    PhotoUrl = p.PhotoUrl,
                    CreatedAt = p.CreatedAt,
                    Address = p.Address != null ? new AddressDto
                    {
                        Id = p.Address.Id,
                        AddressLine1 = p.Address.AddressLine1,
                        AddressLine2 = p.Address.AddressLine2,
                        City = p.Address.City,
                        State = p.Address.State,
                        Country = p.Address.Country,
                        PostalCode = p.Address.PostalCode,
                        Latitude = p.Address.Latitude,
                        Longitude = p.Address.Longitude,
                        Timezone = p.Address.Timezone,
                        CreatedAt = p.Address.CreatedAt
                    } : null
                })
                .ToListAsync();

            return Ok(new { success = true, data = plants, count = plants.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user plants");
            return StatusCode(500, new { error = "Failed to retrieve plants" });
        }
    }

    /// <summary>
    /// Get a specific plant by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserPlant(Guid id)
    {
        try
        {
            var userId = GetUserId();

            var plant = await _context.UserPlants
                .Include(p => p.Address)
                .Where(p => p.Id == id && p.UserId == userId)
                .Select(p => new UserPlantDto
                {
                    Id = p.Id,
                    AddressId = p.AddressId,
                    TreflePlantId = p.TreflePlantId,
                    CommonName = p.CommonName,
                    ScientificName = p.ScientificName,
                    Nickname = p.Nickname,
                    DatePlanted = p.DatePlanted,
                    Location = p.Location,
                    Notes = p.Notes,
                    PhotoUrl = p.PhotoUrl,
                    CreatedAt = p.CreatedAt,
                    Address = p.Address != null ? new AddressDto
                    {
                        Id = p.Address.Id,
                        AddressLine1 = p.Address.AddressLine1,
                        AddressLine2 = p.Address.AddressLine2,
                        City = p.Address.City,
                        State = p.Address.State,
                        Country = p.Address.Country,
                        PostalCode = p.Address.PostalCode,
                        Latitude = p.Address.Latitude,
                        Longitude = p.Address.Longitude,
                        Timezone = p.Address.Timezone,
                        CreatedAt = p.Address.CreatedAt
                    } : null
                })
                .FirstOrDefaultAsync();

            if (plant == null)
            {
                return NotFound(new { error = "Plant not found" });
            }

            return Ok(new { success = true, data = plant });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving plant {PlantId}", id);
            return StatusCode(500, new { error = "Failed to retrieve plant" });
        }
    }

    /// <summary>
    /// Create a new plant
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUserPlant([FromBody] CreateUserPlantRequest request)
    {
        try
        {
            var userId = GetUserId();

            // Check if user can add more plants based on subscription tier
            if (!await _userService.CanAddPlantAsync(userId))
            {
                return BadRequest(new { error = "Plant limit reached for your subscription tier. Please upgrade to add more plants." });
            }

            // Validate address belongs to user if specified
            if (request.AddressId.HasValue)
            {
                var addressExists = await _context.Addresses
                    .AnyAsync(a => a.Id == request.AddressId.Value && a.UserId == userId);

                if (!addressExists)
                {
                    return BadRequest(new { error = "Invalid address" });
                }
            }

            var plant = new UserPlant
            {
                UserId = userId,
                AddressId = request.AddressId,
                TreflePlantId = request.TreflePlantId,
                CommonName = request.CommonName,
                ScientificName = request.ScientificName,
                Nickname = request.Nickname,
                DatePlanted = request.DatePlanted,
                Location = request.Location,
                Notes = request.Notes
            };

            _context.UserPlants.Add(plant);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Plant created for user {UserId}: {PlantId}", userId, plant.Id);

            return CreatedAtAction(nameof(GetUserPlant), new { id = plant.Id }, new
            {
                success = true,
                data = new UserPlantDto
                {
                    Id = plant.Id,
                    AddressId = plant.AddressId,
                    TreflePlantId = plant.TreflePlantId,
                    CommonName = plant.CommonName,
                    ScientificName = plant.ScientificName,
                    Nickname = plant.Nickname,
                    DatePlanted = plant.DatePlanted,
                    Location = plant.Location,
                    Notes = plant.Notes,
                    PhotoUrl = plant.PhotoUrl,
                    CreatedAt = plant.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating plant");
            return StatusCode(500, new { error = "Failed to create plant" });
        }
    }

    /// <summary>
    /// Update a plant
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserPlant(Guid id, [FromBody] UpdateUserPlantRequest request)
    {
        try
        {
            var userId = GetUserId();

            var plant = await _context.UserPlants
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (plant == null)
            {
                return NotFound(new { error = "Plant not found" });
            }

            // Validate address if specified
            if (request.AddressId.HasValue)
            {
                var addressExists = await _context.Addresses
                    .AnyAsync(a => a.Id == request.AddressId.Value && a.UserId == userId);

                if (!addressExists)
                {
                    return BadRequest(new { error = "Invalid address" });
                }
                plant.AddressId = request.AddressId;
            }

            if (request.Nickname != null) plant.Nickname = request.Nickname;
            if (request.DatePlanted.HasValue) plant.DatePlanted = request.DatePlanted;
            if (request.Location != null) plant.Location = request.Location;
            if (request.Notes != null) plant.Notes = request.Notes;

            plant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Plant updated: {PlantId}", id);

            return Ok(new { success = true, message = "Plant updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating plant {PlantId}", id);
            return StatusCode(500, new { error = "Failed to update plant" });
        }
    }

    /// <summary>
    /// Delete a plant
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserPlant(Guid id)
    {
        try
        {
            var userId = GetUserId();

            var plant = await _context.UserPlants
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (plant == null)
            {
                return NotFound(new { error = "Plant not found" });
            }

            _context.UserPlants.Remove(plant);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Plant deleted: {PlantId}", id);

            return Ok(new { success = true, message = "Plant deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting plant {PlantId}", id);
            return StatusCode(500, new { error = "Failed to delete plant" });
        }
    }
}
