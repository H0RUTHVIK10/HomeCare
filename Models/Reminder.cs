using System.ComponentModel.DataAnnotations;

namespace HomeCare.Models
{
    public class Reminder
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime ReminderDate { get; set; }

        [Required]
        [StringLength(50)]
        public string ReminderType { get; set; } = "Maintenance"; // Maintenance, Warranty, Service, Inspection, Custom

        public bool IsCompleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key
        public int ApplianceId { get; set; }

        // Optional direct user FK for fast querying and isolation
        public int? UserId { get; set; }

        // Relationships
        public Appliance Appliance { get; set; } = null!;

        public User? User { get; set; }
    }
}