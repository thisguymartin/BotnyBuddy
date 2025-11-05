using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BotanicalBuddy.API.Data.Entities;

public class UserPlant
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    public Guid? AddressId { get; set; }

    public int? TreflePlantId { get; set; }

    [MaxLength(255)]
    public string? CommonName { get; set; }

    [MaxLength(255)]
    public string? ScientificName { get; set; }

    [MaxLength(100)]
    public string? Nickname { get; set; }

    public DateTime? DatePlanted { get; set; }

    [MaxLength(255)]
    public string? Location { get; set; }

    public string? Notes { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    [ForeignKey("AddressId")]
    public Address? Address { get; set; }

    public ICollection<PlantCareLog> CareLogs { get; set; } = new List<PlantCareLog>();
}
