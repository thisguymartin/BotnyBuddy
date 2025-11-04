using System.ComponentModel.DataAnnotations;

namespace BotanicalBuddy.API.Models;

// Address DTOs
public class AddressDto
{
    public Guid Id { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string Country { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Timezone { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAddressRequest
{
    [Required]
    public string AddressLine1 { get; set; } = string.Empty;

    public string? AddressLine2 { get; set; }

    [Required]
    public string City { get; set; } = string.Empty;

    public string? State { get; set; }

    [Required]
    public string Country { get; set; } = string.Empty;

    public string? PostalCode { get; set; }
}

public class UpdateAddressRequest
{
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
}

// UserPlant DTOs
public class UserPlantDto
{
    public Guid Id { get; set; }
    public Guid? AddressId { get; set; }
    public int? TreflePlantId { get; set; }
    public string? CommonName { get; set; }
    public string? ScientificName { get; set; }
    public string? Nickname { get; set; }
    public DateTime? DatePlanted { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public AddressDto? Address { get; set; }
}

public class CreateUserPlantRequest
{
    public Guid? AddressId { get; set; }

    public int? TreflePlantId { get; set; }

    public string? CommonName { get; set; }

    public string? ScientificName { get; set; }

    [MaxLength(100)]
    public string? Nickname { get; set; }

    public DateTime? DatePlanted { get; set; }

    [MaxLength(255)]
    public string? Location { get; set; }

    public string? Notes { get; set; }
}

public class UpdateUserPlantRequest
{
    public Guid? AddressId { get; set; }
    public string? Nickname { get; set; }
    public DateTime? DatePlanted { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

// PlantCareLog DTOs
public class PlantCareLogDto
{
    public Guid Id { get; set; }
    public Guid UserPlantId { get; set; }
    public string CareType { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public string? Amount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePlantCareLogRequest
{
    [Required]
    public Guid UserPlantId { get; set; }

    [Required]
    public string CareType { get; set; } = string.Empty; // 'Watering', 'Fertilizing', 'Pruning', etc.

    public DateTime? DateTime { get; set; }

    public string? Amount { get; set; }

    public string? Notes { get; set; }
}

// Weather DTOs
public class WeatherDataDto
{
    public Guid Id { get; set; }
    public Guid AddressId { get; set; }
    public DateTime Date { get; set; }
    public decimal? Temperature { get; set; }
    public int? Humidity { get; set; }
    public decimal? Precipitation { get; set; }
    public string? Conditions { get; set; }
}
