using System.ComponentModel.DataAnnotations;

namespace BotanicalBuddy.API.Data.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [MaxLength(50)]
    public string SubscriptionTier { get; set; } = "Free";

    [MaxLength(255)]
    public string? StripeCustomerId { get; set; }

    public bool EmailVerified { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<UserPlant> UserPlants { get; set; } = new List<UserPlant>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
