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

        public DateTime ReminderDate { get; set; }

        [Required]
        [StringLength(50)]
        public string ReminderType { get; set; } = string.Empty;

        public bool IsCompleted { get; set; } = false;

        // Foreign Key
        public int ApplianceId { get; set; }

        // Relationship
        public Appliance Appliance { get; set; } = null!;
    }
}