using BotanicalBuddy.API.Models;
using BotanicalBuddy.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BotanicalBuddy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlantsController : ControllerBase
{
    private readonly TrefleApiService _trefleApiService;
    private readonly ILogger<PlantsController> _logger;

    public PlantsController(TrefleApiService trefleApiService, ILogger<PlantsController> logger)
    {
        _trefleApiService = trefleApiService;
        _logger = logger;
    }

    /// <summary>
    /// List all plants with pagination
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    [HttpGet]
    public async Task<IActionResult> ListPlants([FromQuery] int page = 1)
    {
        try
        {
            _logger.LogInformation("Listing plants - page: {Page}", page);

            var result = await _trefleApiService.ListPlantsAsync(page);

            return Ok(new ApiResponse<List<TreplePlant>>
            {
                Success = true,
                Data = result.Data,
                Meta = result.Meta,
                Links = result.Links
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing plants");
            return StatusCode(500, new ApiResponse<List<TreplePlant>>
            {
                Success = false,
                Error = "Failed to retrieve plants",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Search for plants by query
    /// </summary>
    /// <param name="q">Search query (common name, scientific name, etc.)</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    [HttpGet("search")]
    public async Task<IActionResult> SearchPlants([FromQuery] string q, [FromQuery] int page = 1)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new ApiResponse<List<TreplePlant>>
                {
                    Success = false,
                    Error = "Missing required parameter: q (query)"
                });
            }

            _logger.LogInformation("Searching plants - query: {Query}, page: {Page}", q, page);

            var result = await _trefleApiService.SearchPlantsAsync(q, page);

            return Ok(new ApiResponse<List<TreplePlant>>
            {
                Success = true,
                Query = q,
                Data = result.Data,
                Meta = result.Meta,
                Links = result.Links
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching plants");
            return StatusCode(500, new ApiResponse<List<TreplePlant>>
            {
                Success = false,
                Error = "Failed to search plants",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get detailed information about a specific plant by ID
    /// </summary>
    /// <param name="id">The Trefle plant ID</param>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlantById(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<TreplePlantDetail>
                {
                    Success = false,
                    Error = "Invalid plant ID"
                });
            }

            _logger.LogInformation("Fetching plant details - ID: {PlantId}", id);

            var result = await _trefleApiService.GetPlantByIdAsync(id);

            return Ok(new ApiResponse<TreplePlantDetail>
            {
                Success = true,
                Data = result.Data,
                Meta = result.Meta
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching plant {PlantId}", id);
            return StatusCode(500, new ApiResponse<TreplePlantDetail>
            {
                Success = false,
                Error = "Failed to retrieve plant details",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Filter plants by common name
    /// </summary>
    /// <param name="name">Common name to filter by</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    [HttpGet("filter/common-name")]
    public async Task<IActionResult> FilterByCommonName([FromQuery] string name, [FromQuery] int page = 1)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new ApiResponse<List<TreplePlant>>
                {
                    Success = false,
                    Error = "Missing required parameter: name (common name)"
                });
            }

            _logger.LogInformation("Filtering plants by common name - name: {CommonName}, page: {Page}", name, page);

            var result = await _trefleApiService.GetPlantsByCommonNameAsync(name, page);

            return Ok(new ApiResponse<List<TreplePlant>>
            {
                Success = true,
                Filter = new { common_name = name },
                Data = result.Data,
                Meta = result.Meta,
                Links = result.Links
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering plants by common name");
            return StatusCode(500, new ApiResponse<List<TreplePlant>>
            {
                Success = false,
                Error = "Failed to filter plants",
                Message = ex.Message
            });
        }
    }
}
