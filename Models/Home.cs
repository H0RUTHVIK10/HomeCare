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

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? ZipCode { get; set; }

        [StringLength(50)]
        public string HomeType { get; set; } = "Primary Residence"; // Primary Residence, Rental Apartment, Vacation Home, Office

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key
        public int UserId { get; set; }

        // Relationship
        public User User { get; set; } = null!;

        public ICollection<Appliance> Appliances { get; set; } = new List<Appliance>();
    }
}