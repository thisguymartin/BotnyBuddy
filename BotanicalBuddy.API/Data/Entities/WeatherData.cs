using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BotanicalBuddy.API.Data.Entities;

public class WeatherData
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid AddressId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Temperature { get; set; }

    public int? Humidity { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Precipitation { get; set; }

    [MaxLength(100)]
    public string? Conditions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("AddressId")]
    public Address Address { get; set; } = null!;
}
