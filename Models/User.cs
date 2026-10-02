using System.ComponentModel.DataAnnotations;

namespace HomeCare.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(50)]
        public string Role { get; set; } = "User";

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public ICollection<Home> Homes { get; set; } = new List<Home>();

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
    }
}