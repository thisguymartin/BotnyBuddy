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
public class AddressesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AddressesController> _logger;

    public AddressesController(
        ApplicationDbContext context,
        ILogger<AddressesController> logger)
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
    /// Get all addresses for the authenticated user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAddresses()
    {
        try
        {
            var userId = GetUserId();

            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AddressDto
                {
                    Id = a.Id,
                    AddressLine1 = a.AddressLine1,
                    AddressLine2 = a.AddressLine2,
                    City = a.City,
                    State = a.State,
                    Country = a.Country,
                    PostalCode = a.PostalCode,
                    Latitude = a.Latitude,
                    Longitude = a.Longitude,
                    Timezone = a.Timezone,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(new { success = true, data = addresses, count = addresses.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving addresses");
            return StatusCode(500, new { error = "Failed to retrieve addresses" });
        }
    }

    /// <summary>
    /// Get a specific address by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAddress(Guid id)
    {
        try
        {
            var userId = GetUserId();

            var address = await _context.Addresses
                .Where(a => a.Id == id && a.UserId == userId)
                .Select(a => new AddressDto
                {
                    Id = a.Id,
                    AddressLine1 = a.AddressLine1,
                    AddressLine2 = a.AddressLine2,
                    City = a.City,
                    State = a.State,
                    Country = a.Country,
                    PostalCode = a.PostalCode,
                    Latitude = a.Latitude,
                    Longitude = a.Longitude,
                    Timezone = a.Timezone,
                    CreatedAt = a.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (address == null)
            {
                return NotFound(new { error = "Address not found" });
            }

            return Ok(new { success = true, data = address });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving address {AddressId}", id);
            return StatusCode(500, new { error = "Failed to retrieve address" });
        }
    }

    /// <summary>
    /// Create a new address
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAddress([FromBody] CreateAddressRequest request)
    {
        try
        {
            var userId = GetUserId();

            var address = new Address
            {
                UserId = userId,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                City = request.City,
                State = request.State,
                Country = request.Country,
                PostalCode = request.PostalCode
            };

            // TODO: Integrate geocoding service to get lat/long from address
            // For now, these will be null and can be updated later

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Address created for user {UserId}: {AddressId}", userId, address.Id);

            return CreatedAtAction(nameof(GetAddress), new { id = address.Id }, new
            {
                success = true,
                data = new AddressDto
                {
                    Id = address.Id,
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    City = address.City,
                    State = address.State,
                    Country = address.Country,
                    PostalCode = address.PostalCode,
                    Latitude = address.Latitude,
                    Longitude = address.Longitude,
                    Timezone = address.Timezone,
                    CreatedAt = address.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating address");
            return StatusCode(500, new { error = "Failed to create address" });
        }
    }

    /// <summary>
    /// Update an address
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] UpdateAddressRequest request)
    {
        try
        {
            var userId = GetUserId();

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (address == null)
            {
                return NotFound(new { error = "Address not found" });
            }

            if (request.AddressLine1 != null) address.AddressLine1 = request.AddressLine1;
            if (request.AddressLine2 != null) address.AddressLine2 = request.AddressLine2;
            if (request.City != null) address.City = request.City;
            if (request.State != null) address.State = request.State;
            if (request.Country != null) address.Country = request.Country;
            if (request.PostalCode != null) address.PostalCode = request.PostalCode;

            address.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Address updated: {AddressId}", id);

            return Ok(new { success = true, message = "Address updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating address {AddressId}", id);
            return StatusCode(500, new { error = "Failed to update address" });
        }
    }

    /// <summary>
    /// Delete an address
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(Guid id)
    {
        try
        {
            var userId = GetUserId();

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (address == null)
            {
                return NotFound(new { error = "Address not found" });
            }

            // Check if address is being used by any plants
            var plantsUsingAddress = await _context.UserPlants
                .AnyAsync(p => p.AddressId == id);

            if (plantsUsingAddress)
            {
                return BadRequest(new { error = "Cannot delete address that is being used by plants. Please update or delete those plants first." });
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Address deleted: {AddressId}", id);

            return Ok(new { success = true, message = "Address deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting address {AddressId}", id);
            return StatusCode(500, new { error = "Failed to delete address" });
        }
    }
}
