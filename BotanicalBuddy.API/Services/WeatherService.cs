using BotanicalBuddy.API.Data;
using BotanicalBuddy.API.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace BotanicalBuddy.API.Services;

public class WeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        IHttpClientFactory httpClientFactory,
        ApplicationDbContext context,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<WeatherService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _context = context;
        _cache = cache;
        _apiKey = configuration["WeatherApi:ApiKey"] ?? throw new ArgumentNullException("WeatherApi:ApiKey is not configured");
        _baseUrl = configuration["WeatherApi:BaseUrl"] ?? "https://api.openweathermap.org/data/2.5";
        _logger = logger;
    }

    public async Task<WeatherData?> GetWeatherForAddressAsync(Guid addressId)
    {
        try
        {
            var address = await _context.Addresses.FindAsync(addressId);
            if (address == null || !address.Latitude.HasValue || !address.Longitude.HasValue)
            {
                _logger.LogWarning("Address {AddressId} not found or missing coordinates", addressId);
                return null;
            }

            var cacheKey = $"weather_{addressId}_{DateTime.UtcNow:yyyy-MM-dd}";

            // Check cache first
            if (_cache.TryGetValue(cacheKey, out WeatherData? cachedWeather) && cachedWeather != null)
            {
                _logger.LogInformation("Cache hit for weather at address {AddressId}", addressId);
                return cachedWeather;
            }

            // Check database for today's weather
            var today = DateTime.UtcNow.Date;
            var dbWeather = await _context.WeatherData
                .FirstOrDefaultAsync(w => w.AddressId == addressId && w.Date.Date == today);

            if (dbWeather != null)
            {
                _cache.Set(cacheKey, dbWeather, TimeSpan.FromHours(1));
                return dbWeather;
            }

            // Fetch from API
            var url = $"{_baseUrl}/weather?lat={address.Latitude}&lon={address.Longitude}&appid={_apiKey}&units=metric";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var weatherResponse = JsonConvert.DeserializeObject<OpenWeatherMapResponse>(content);

            if (weatherResponse == null)
            {
                return null;
            }

            // Save to database
            var weatherData = new WeatherData
            {
                AddressId = addressId,
                Date = DateTime.UtcNow.Date,
                Temperature = (decimal?)weatherResponse.Main?.Temp,
                Humidity = weatherResponse.Main?.Humidity,
                Precipitation = 0, // OpenWeatherMap free tier doesn't provide this directly
                Conditions = weatherResponse.Weather?.FirstOrDefault()?.Description
            };

            _context.WeatherData.Add(weatherData);
            await _context.SaveChangesAsync();

            // Cache it
            _cache.Set(cacheKey, weatherData, TimeSpan.FromHours(1));

            _logger.LogInformation("Weather fetched and cached for address {AddressId}", addressId);

            return weatherData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather for address {AddressId}", addressId);
            return null;
        }
    }

    public async Task<IEnumerable<WeatherData>> GetWeatherHistoryAsync(Guid addressId, int days = 7)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        return await _context.WeatherData
            .Where(w => w.AddressId == addressId && w.Date >= startDate)
            .OrderByDescending(w => w.Date)
            .ToListAsync();
    }
}

// OpenWeatherMap API response models
public class OpenWeatherMapResponse
{
    public MainData? Main { get; set; }
    public List<WeatherDescription>? Weather { get; set; }
}

public class MainData
{
    public double? Temp { get; set; }
    public int? Humidity { get; set; }
}

public class WeatherDescription
{
    public string? Description { get; set; }
}
