using BotanicalBuddy.API.Models;
using Microsoft.Extensions.Caching.Memory;

namespace BotanicalBuddy.API.Services;

public class CachedTrefleApiService
{
    private readonly TrefleApiService _trefleApiService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedTrefleApiService> _logger;
    private const int CacheExpirationHours = 24;

    public CachedTrefleApiService(
        TrefleApiService trefleApiService,
        IMemoryCache cache,
        ILogger<CachedTrefleApiService> logger)
    {
        _trefleApiService = trefleApiService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<TrefleSearchResponse> SearchPlantsAsync(string query, int page = 1)
    {
        var cacheKey = $"trefle_search_{query}_{page}";

        if (_cache.TryGetValue(cacheKey, out TrefleSearchResponse? cachedResult) && cachedResult != null)
        {
            _logger.LogInformation("Cache hit for search query: {Query}, page: {Page}", query, page);
            return cachedResult;
        }

        _logger.LogInformation("Cache miss for search query: {Query}, page: {Page}", query, page);
        var result = await _trefleApiService.SearchPlantsAsync(query, page);

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(CacheExpirationHours));

        _cache.Set(cacheKey, result, cacheOptions);

        return result;
    }

    public async Task<TrefleDetailResponse> GetPlantByIdAsync(int plantId)
    {
        var cacheKey = $"trefle_plant_{plantId}";

        if (_cache.TryGetValue(cacheKey, out TrefleDetailResponse? cachedResult) && cachedResult != null)
        {
            _logger.LogInformation("Cache hit for plant ID: {PlantId}", plantId);
            return cachedResult;
        }

        _logger.LogInformation("Cache miss for plant ID: {PlantId}", plantId);
        var result = await _trefleApiService.GetPlantByIdAsync(plantId);

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(CacheExpirationHours));

        _cache.Set(cacheKey, result, cacheOptions);

        return result;
    }

    public async Task<TrefleSearchResponse> ListPlantsAsync(int page = 1)
    {
        var cacheKey = $"trefle_list_{page}";

        if (_cache.TryGetValue(cacheKey, out TrefleSearchResponse? cachedResult) && cachedResult != null)
        {
            _logger.LogInformation("Cache hit for plant list, page: {Page}", page);
            return cachedResult;
        }

        _logger.LogInformation("Cache miss for plant list, page: {Page}", page);
        var result = await _trefleApiService.ListPlantsAsync(page);

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(CacheExpirationHours));

        _cache.Set(cacheKey, result, cacheOptions);

        return result;
    }

    public async Task<TrefleSearchResponse> GetPlantsByCommonNameAsync(string commonName, int page = 1)
    {
        var cacheKey = $"trefle_common_{commonName}_{page}";

        if (_cache.TryGetValue(cacheKey, out TrefleSearchResponse? cachedResult) && cachedResult != null)
        {
            _logger.LogInformation("Cache hit for common name: {CommonName}, page: {Page}", commonName, page);
            return cachedResult;
        }

        _logger.LogInformation("Cache miss for common name: {CommonName}, page: {Page}", commonName, page);
        var result = await _trefleApiService.GetPlantsByCommonNameAsync(commonName, page);

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(CacheExpirationHours));

        _cache.Set(cacheKey, result, cacheOptions);

        return result;
    }

    public CacheStatistics GetCacheStatistics()
    {
        // Note: IMemoryCache doesn't expose statistics by default
        // This is a placeholder for future implementation with a custom cache wrapper
        return new CacheStatistics
        {
            Message = "Cache statistics require custom implementation"
        };
    }
}

public class CacheStatistics
{
    public string Message { get; set; } = string.Empty;
    public int? TotalEntries { get; set; }
    public long? MemoryUsageBytes { get; set; }
}
