using System.ComponentModel.DataAnnotations;

namespace HomeCare.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        [StringLength(50)]
        public string NotificationType { get; set; } = "System"; // Warranty, Service, Maintenance, System

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(250)]
        public string? LinkUrl { get; set; }

        // Foreign Key
        public int UserId { get; set; }

        // Relationship
        public User User { get; set; } = null!;
    }
}
