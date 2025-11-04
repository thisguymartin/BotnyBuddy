using BotanicalBuddy.API.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BotanicalBuddy.API.Services;

public class TrefleApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiToken;
    private readonly ILogger<TrefleApiService> _logger;
    private const string BaseUrl = "https://trefle.io/api/v1";

    public TrefleApiService(HttpClient httpClient, IConfiguration configuration, ILogger<TrefleApiService> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _apiToken = configuration["Trefle:ApiToken"] ?? throw new ArgumentNullException("Trefle:ApiToken is not configured");
        _logger = logger;
        _logger.LogInformation("TrefleApiService initialized");
    }

    /// <summary>
    /// Search for plants by query string
    /// </summary>
    public async Task<TrefleSearchResponse> SearchPlantsAsync(string query, int page = 1)
    {
        try
        {
            _logger.LogInformation("Searching plants with query: {Query}, page: {Page}", query, page);

            var url = $"/plants/search?q={Uri.EscapeDataString(query)}&page={page}&token={_apiToken}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TrefleSearchResponse>(content, GetJsonSettings());

            _logger.LogInformation("Found {Total} plants", result?.Meta?.Total ?? 0);
            return result ?? new TrefleSearchResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching plants");
            throw new Exception($"Failed to search plants: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Get detailed information about a specific plant by ID
    /// </summary>
    public async Task<TrefleDetailResponse> GetPlantByIdAsync(int plantId)
    {
        try
        {
            _logger.LogInformation("Fetching plant details for ID: {PlantId}", plantId);

            var url = $"/plants/{plantId}?token={_apiToken}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TrefleDetailResponse>(content, GetJsonSettings());

            _logger.LogInformation("Retrieved plant: {ScientificName}", result?.Data?.ScientificName);
            return result ?? new TrefleDetailResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching plant {PlantId}", plantId);
            throw new Exception($"Failed to fetch plant details: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// List all plants with pagination
    /// </summary>
    public async Task<TrefleSearchResponse> ListPlantsAsync(int page = 1)
    {
        try
        {
            _logger.LogInformation("Listing plants, page: {Page}", page);

            var url = $"/plants?page={page}&token={_apiToken}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TrefleSearchResponse>(content, GetJsonSettings());

            _logger.LogInformation("Retrieved {Count} plants", result?.Data?.Count ?? 0);
            return result ?? new TrefleSearchResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing plants");
            throw new Exception($"Failed to list plants: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Get plants by common name filter
    /// </summary>
    public async Task<TrefleSearchResponse> GetPlantsByCommonNameAsync(string commonName, int page = 1)
    {
        try
        {
            _logger.LogInformation("Filtering plants by common name: {CommonName}", commonName);

            var url = $"/plants?filter[common_name]={Uri.EscapeDataString(commonName)}&page={page}&token={_apiToken}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TrefleSearchResponse>(content, GetJsonSettings());

            _logger.LogInformation("Found {Total} plants matching '{CommonName}'", result?.Meta?.Total ?? 0, commonName);
            return result ?? new TrefleSearchResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering plants by common name");
            throw new Exception($"Failed to filter plants: {ex.Message}", ex);
        }
    }

    private static JsonSerializerSettings GetJsonSettings()
    {
        return new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            NullValueHandling = NullValueHandling.Ignore
        };
    }
}
