using System.ComponentModel.DataAnnotations;

namespace HomeCare.Models
{
    public class Home
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key
        public int UserId { get; set; }

        // Relationship
        public User User { get; set; } = null!;

        public ICollection<Appliance> Appliances { get; set; } = new List<Appliance>();
    }
}