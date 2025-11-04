using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BotanicalBuddy.API.Data.Entities;

public class PlantCareLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserPlantId { get; set; }

    [Required]
    [MaxLength(50)]
    public string CareType { get; set; } = string.Empty; // 'Watering', 'Fertilizing', 'Pruning', etc.

    [Required]
    public DateTime DateTime { get; set; }

    [MaxLength(50)]
    public string? Amount { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserPlantId")]
    public UserPlant UserPlant { get; set; } = null!;
}
